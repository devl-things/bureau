#!/usr/bin/env bash
set -euo pipefail

########################################
# CONFIG – paths for your setup        #
########################################

# Nuc environment root (has docker-compose.yml and /nginx)
NUC_ROOT="/c/0001_docker_ran/nuc"

# This script lives in: C:\030220_repos\bureau\server\deploy
# Repo root is one level above:
SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

# Get repo URL from git config (no need to hardcode)
REPO_URL=$(git -C "$REPO_ROOT" config --get remote.origin.url)

# Paths inside the repo (relative to repo root)
CHORES_NGINX_CONF_REL="deploy/nginx/https/niles-chores.conf"
CHORES_COMPOSE_REL="deploy/docker-compose.yml"

# Path to the canonical .env (local-only, NOT in git)
CANONICAL_ENV_FILE="$REPO_ROOT/deploy/.env"

# Nginx container name in the nuc environment
NGINX_CONTAINER_NAME="nginx"

########################################
# ARG: git tag to deploy               #
########################################

TAG="${1:-}"

if [ -z "$TAG" ]; then
  echo "Usage: $0 <git-tag>"
  echo "Example: $0 v0.1.0  or  $0 niles-chores-v0.1.0"
  exit 1
fi

echo "==> Deploying niles-chores tag: $TAG"
echo "    Repo root: $REPO_ROOT"
echo "    Remote URL: $REPO_URL"

if [ ! -f "$CANONICAL_ENV_FILE" ]; then
  echo "ERROR: Expected deploy .env not found at:"
  echo "  $CANONICAL_ENV_FILE"
  echo "Create it with your appsettings/env values first."
  exit 1
fi

########################################
# 1) Clone into a temp directory       #
########################################

TEMP_DIR=$(mktemp -d "/tmp/niles-chores-${TAG//\//-}-XXXX")
echo "==> Cloning into temp dir: $TEMP_DIR"

git clone "$REPO_URL" "$TEMP_DIR"
git -C "$TEMP_DIR" fetch --all --tags
git -C "$TEMP_DIR" checkout "$TAG"

SOURCE_CONF="$TEMP_DIR/$CHORES_NGINX_CONF_REL"
COMPOSE_FILE="$TEMP_DIR/$CHORES_COMPOSE_REL"
TEMP_ENV_FILE="$TEMP_DIR/deploy/.env"

if [ ! -f "$SOURCE_CONF" ]; then
  echo "ERROR: nginx conf not found at:"
  echo "  $SOURCE_CONF"
  echo "Check that the tag '$TAG' contains deploy/nginx/https/niles-chores.conf"
  rm -rf "$TEMP_DIR"
  exit 1
fi

if [ ! -f "$COMPOSE_FILE" ]; then
  echo "ERROR: docker-compose.yml not found at:"
  echo "  $COMPOSE_FILE"
  rm -rf "$TEMP_DIR"
  exit 1
fi

########################################
# 2) Copy .env into temp deploy folder #
########################################

echo "==> Copying deploy .env into temp clone"
echo "    $CANONICAL_ENV_FILE"
echo " -> $TEMP_ENV_FILE"

cp "$CANONICAL_ENV_FILE" "$TEMP_ENV_FILE"

########################################
# 3) Copy nginx conf into nuc env      #
########################################

NGINX_HTTPS_DIR="$NUC_ROOT/nginx/conf/https"
TARGET_CONF="$NGINX_HTTPS_DIR/niles-chores.conf"

echo "==> Copying nginx config:"
echo "    $SOURCE_CONF"
echo " -> $TARGET_CONF"

mkdir -p "$NGINX_HTTPS_DIR"
cp "$SOURCE_CONF" "$TARGET_CONF"

########################################
# 4) Ensure nuc-network exists         #
########################################

if ! docker network inspect nuc-network >/dev/null 2>&1; then
  echo "WARNING: Docker network 'nuc-network' not found."
  echo "Make sure the nuc environment is up at least once:"
  echo "  cd $NUC_ROOT && docker compose up -d"
fi

########################################
# 5) Bring up niles-chores stack       #
########################################

echo "==> Starting niles-chores stack from tag $TAG"
echo "    Using compose file: $COMPOSE_FILE"

# docker compose will auto-load the .env from the same directory as COMPOSE_FILE
docker compose -f "$COMPOSE_FILE" up -d --build

########################################
# 6) Reload main nginx in nuc env      #
########################################

echo "==> Reloading nginx in nuc environment"
docker exec "$NGINX_CONTAINER_NAME" nginx -t
docker exec "$NGINX_CONTAINER_NAME" nginx -s reload

########################################
# 7) Cleanup temp directory            #
########################################

echo "==> Cleaning up temp dir: $TEMP_DIR"
rm -rf "$TEMP_DIR"

echo "==> Deployment of tag '$TAG' completed."
