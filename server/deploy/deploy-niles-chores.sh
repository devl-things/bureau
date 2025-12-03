#!/usr/bin/env bash
set -euo pipefail

########################################
# CONFIG – static paths for your setup #
########################################

# Nuc environment root (has docker-compose.yml and /nginx)
NUC_ROOT="/c/0001_docker_ran/nuc"

# Nginx container name in the nuc environment
NGINX_CONTAINER_NAME="nginx"

# Git repo URL for bureau/server
REPO_URL="https://github.com/devl-things/bureau.git"

# Script directory (where this file lives: C:\030220_repos\bureau\server\deploy)
SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)

# Repo root is parent of /server (i.e. /bureau)
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

# Canonical .env lives next to this script (server/deploy/.env)
CANONICAL_ENV_FILE="$SCRIPT_DIR/.env"

########################################
# ARG: git tag to deploy               #
########################################

TAG="${1:-}"

if [ -z "$TAG" ]; then
  echo "Usage: $0 <git-tag>"
  echo "Example: $0 niles-chores-v0.1.0"
  exit 1
fi

echo "==> Deploying niles-chores tag: $TAG"
echo "    Script dir:  $SCRIPT_DIR"
echo "    Repo root:   $REPO_ROOT"
echo "    Repo URL:    $REPO_URL"

if [ ! -f "$CANONICAL_ENV_FILE" ]; then
  echo "ERROR: Expected deploy .env not found at:"
  echo "  $CANONICAL_ENV_FILE"
  echo "Create it with your ConnectionStrings__NilesDB, Api__*, Logging__*, etc."
  exit 1
fi

########################################
# 1) Deployment root = current dir     #
########################################

# You will run this from C:\030221_deployment
DEPLOY_ROOT=$(pwd)
echo "    Deployment root (current dir): $DEPLOY_ROOT"

# Temp folder named after the tag inside deployment root
TEMP_DIR="$DEPLOY_ROOT/$TAG"

# If it already exists, wipe it
if [ -d "$TEMP_DIR" ]; then
  echo "==> Removing existing temp dir: $TEMP_DIR"
  rm -rf "$TEMP_DIR"
fi

mkdir -p "$TEMP_DIR"

########################################
# 2) Clone repo into TEMP_DIR          #
########################################

echo "==> Cloning into temp dir: $TEMP_DIR"

git clone "$REPO_URL" "$TEMP_DIR"
git -C "$TEMP_DIR" fetch --all --tags
git -C "$TEMP_DIR" checkout "$TAG"

########################################
# 3) Locate nginx conf in cloned repo  #
########################################

# New path (under server/deploy)
CANDIDATE_CONF1="$TEMP_DIR/server/deploy/nginx/https/niles-chores.conf"
# Old path (under server/deploy/nginx)
CANDIDATE_CONF2="$TEMP_DIR/server/deploy/nginx/niles-chores.conf"

if [ -f "$CANDIDATE_CONF1" ]; then
  SOURCE_CONF="$CANDIDATE_CONF1"
elif [ -f "$CANDIDATE_CONF2" ]; then
  SOURCE_CONF="$CANDIDATE_CONF2"
else
  echo "ERROR: nginx conf not found in either path:"
  echo "  $CANDIDATE_CONF1"
  echo "  $CANDIDATE_CONF2"
  echo "Check that the tag '$TAG' contains a niles-chores nginx config."
  rm -rf "$TEMP_DIR"
  exit 1
fi

COMPOSE_FILE="$TEMP_DIR/server/deploy/docker-compose.yml"

if [ ! -f "$COMPOSE_FILE" ] || [ ! -s "$COMPOSE_FILE" ]; then
  echo "ERROR: docker-compose.yml not found or empty at:"
  echo "  $COMPOSE_FILE"
  rm -rf "$TEMP_DIR"
  exit 1
fi

echo "==> Using nginx config from:"
echo "    $SOURCE_CONF"
echo "==> Using compose file:"
echo "    $COMPOSE_FILE"

########################################
# 4) Copy .env into temp deploy folder #
########################################

TEMP_ENV_FILE="$TEMP_DIR/server/deploy/.env"

echo "==> Copying deploy .env into temp clone"
echo "    $CANONICAL_ENV_FILE"
echo " -> $TEMP_ENV_FILE"

cp "$CANONICAL_ENV_FILE" "$TEMP_ENV_FILE"

########################################
# 5) Copy nginx conf into nuc env      #
########################################

NGINX_HTTPS_DIR="$NUC_ROOT/nginx/conf/https"
TARGET_CONF="$NGINX_HTTPS_DIR/niles-chores.conf"

echo "==> Copying nginx config into nuc environment:"
echo "    $SOURCE_CONF"
echo " -> $TARGET_CONF"

mkdir -p "$NGINX_HTTPS_DIR"
cp "$SOURCE_CONF" "$TARGET_CONF"

########################################
# 6) Ensure nuc-network exists         #
########################################

if ! docker network inspect nuc-network >/dev/null 2>&1; then
  echo "WARNING: Docker network 'nuc-network' not found."
  echo "Make sure the nuc environment is up at least once:"
  echo "  cd $NUC_ROOT && docker compose up -d"
fi

########################################
# 7) Bring up niles-chores stack       #
########################################

echo "==> Starting niles-chores stack from tag $TAG"
echo "    Running: docker compose -f \"$COMPOSE_FILE\" up -d --build"

docker compose -f "$COMPOSE_FILE" up -d --build

########################################
# 8) Reload main nginx in nuc env      #
########################################

echo "==> Reloading nginx in nuc environment"
docker exec "$NGINX_CONTAINER_NAME" nginx -t
docker exec "$NGINX_CONTAINER_NAME" nginx -s reload

########################################
# 9) Cleanup temp directory            #
########################################

echo "==> Cleaning up temp dir: $TEMP_DIR"
rm -rf "$TEMP_DIR"

echo "==> Deployment of tag '$TAG' completed."
