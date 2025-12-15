#!/usr/bin/env bash
set -euo pipefail

########################################
# Script directory
########################################
SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)

########################################
# Load canonical .env (excluded in git)
########################################
CANONICAL_ENV_FILE="$SCRIPT_DIR/.env"

if [[ ! -f "$CANONICAL_ENV_FILE" ]]; then
  echo "ERROR: Expected deploy .env not found at:" >&2
  echo "  $CANONICAL_ENV_FILE" >&2
  exit 1
fi

# Export variables from .env (bash-compatible KEY=value lines)
set -a
# shellcheck disable=SC1090
source <(grep -E '^DEPLOY__[A-Za-z0-9_]*=' "$CANONICAL_ENV_FILE")
set +a

########################################
# Deployment config from .env
########################################
: "${DEPLOY__ROOT:?DEPLOY__ROOT must be set in $CANONICAL_ENV_FILE}"
: "${DEPLOY__NGINX_CONTAINER_NAME:?DEPLOY__NGINX_CONTAINER_NAME must be set in $CANONICAL_ENV_FILE}"

NUC_ROOT="$DEPLOY__ROOT"
NGINX_CONTAINER_NAME="$DEPLOY__NGINX_CONTAINER_NAME"
REPO_URL="${DEPLOY__REPO_URL:-}"

# Allow overriding target dirs (optional)
TARGET_HTTP_DIR="${DEPLOY__NGINX_HTTP_DIR:-$NUC_ROOT/nginx/conf/https}"
TARGET_SNIPPETS_DIR="${DEPLOY__NGINX_SNIPPETS_TPL_DIR:-$NUC_ROOT/nginx/conf/snippets}"

########################################
# ARG: git tag to deploy
########################################
TAG="${1:-}"
if [[ -z "$TAG" ]]; then
  echo "Usage: $(basename "$0") <git-tag>" >&2
  echo "Example: $(basename "$0") v0.1.0" >&2
  exit 1
fi

########################################
# Repo URL fallback (if not set)
########################################
if [[ -z "$REPO_URL" ]]; then
  if git -C "$SCRIPT_DIR/../.." rev-parse --is-inside-work-tree >/dev/null 2>&1; then
    REPO_URL=$(git -C "$SCRIPT_DIR/../.." remote get-url origin)
  else
    echo "ERROR: DEPLOY__REPO_URL not set and cannot infer git remote." >&2
    exit 1
  fi
fi

echo "==> Deploying tag: $TAG"
echo "    Repo URL:      $REPO_URL"
echo "    Root:          $NUC_ROOT"
echo "    NGINX:         $NGINX_CONTAINER_NAME"
echo "    Env file:      $CANONICAL_ENV_FILE"
echo "    Target https:  $TARGET_HTTP_DIR"
echo "    Target snips:  $TARGET_SNIPPETS_DIR"

########################################
# Helper: render templates via envsubst
########################################
render_file() {
  local src="$1"
  local dst="$2"

  if ! command -v envsubst >/dev/null 2>&1; then
    echo "ERROR: envsubst not found on PATH." >&2
    echo "Install gettext (envsubst) or add a renderer fallback." >&2
    return 1
  fi

  envsubst < "$src" > "$dst"
  return 0
}

########################################
# 1) Temp directory in current folder
########################################
DEPLOY_ROOT="$(pwd)"
TEMP_DIR="$DEPLOY_ROOT/$TAG"

if [[ -d "$TEMP_DIR" ]]; then
  echo "==> Removing existing temp dir: $TEMP_DIR"
  rm -rf "$TEMP_DIR"
fi
mkdir -p "$TEMP_DIR"

########################################
# 2) Clone repo at tag into TEMP_DIR
########################################
echo "==> Cloning repo into: $TEMP_DIR"
git clone "$REPO_URL" "$TEMP_DIR"
git -C "$TEMP_DIR" fetch --all --tags
git -C "$TEMP_DIR" checkout "$TAG"

########################################
# 3) Validate expected files/folders
########################################
COMPOSE_FILE="$TEMP_DIR/server/deploy/docker-compose.yml"
if [[ ! -f "$COMPOSE_FILE" || ! -s "$COMPOSE_FILE" ]]; then
  echo "ERROR: docker-compose.yml not found or empty at:" >&2
  echo "  $COMPOSE_FILE" >&2
  rm -rf "$TEMP_DIR"
  exit 1
fi

SOURCE_HTTP_TPL_DIR="$TEMP_DIR/server/deploy/nginx/https"
SOURCE_SNIPPETS_TPL_DIR="$TEMP_DIR/server/deploy/nginx/snippets"

if [[ ! -d "$SOURCE_HTTP_TPL_DIR" ]]; then
  echo "ERROR: nginx template folder not found:" >&2
  echo "  $SOURCE_HTTP_TPL_DIR" >&2
  rm -rf "$TEMP_DIR"
  exit 1
fi

if [[ ! -d "$SOURCE_SNIPPETS_TPL_DIR" ]]; then
  echo "ERROR: nginx snippets template folder not found:" >&2
  echo "  $SOURCE_SNIPPETS_TPL_DIR" >&2
  rm -rf "$TEMP_DIR"
  exit 1
fi

########################################
# 4) Copy canonical .env into the tag
########################################
TEMP_ENV_FILE="$TEMP_DIR/server/deploy/.env"
echo "==> Copying canonical .env into cloned tag"
cp "$CANONICAL_ENV_FILE" "$TEMP_ENV_FILE"

########################################
# 5) Render nginx templates
########################################
RENDER_DIR="$TEMP_DIR/.rendered-nginx"
RENDER_HTTP_DIR="$RENDER_DIR/https"
RENDER_SNIPPETS_DIR="$RENDER_DIR/snippets"

mkdir -p "$RENDER_HTTP_DIR" "$RENDER_SNIPPETS_DIR"
mkdir -p "$TARGET_HTTP_DIR" "$TARGET_SNIPPETS_DIR"

echo "==> Rendering http-scope configs (templates -> rendered):"
shopt -s nullglob
for file in "$SOURCE_HTTP_TPL_DIR"/*.conf; do
  base=$(basename "$file")
  out="$RENDER_HTTP_DIR/$base"
  echo "    render: $base"
  render_file "$file" "$out"
done

shopt -u nullglob

echo "==> Installing rendered http-scope configs (only overwriting tag files):"
for file in "$RENDER_HTTP_DIR"/*.conf; do
  [[ -e "$file" ]] || continue
  base=$(basename "$file")
  echo "    -> $TARGET_HTTP_DIR/$base"
  cp "$file" "$TARGET_HTTP_DIR/$base"
done

echo "==> Installing snippets (no rendering; overwrite only tag files):"
shopt -s nullglob
for file in "$SOURCE_SNIPPETS_TPL_DIR"/*.conf; do
  base=$(basename "$file")
  echo "    -> $TARGET_SNIPPETS_DIR/$base"
  cp "$file" "$TARGET_SNIPPETS_DIR/$base"
done

########################################
# 6) Ensure docker network exists
########################################
if ! docker network inspect nuc-network >/dev/null 2>&1; then
  echo "WARNING: Docker network 'nuc-network' not found." >&2
  echo "If this is your first run, bring up the env once so the network exists." >&2
fi

########################################
# 7) Build + run stack from that tag
########################################
echo "==> Starting stack from tag $TAG"
docker compose -f "$COMPOSE_FILE" up -d --build

########################################
# 8) Reload nginx container
########################################
echo "==> Reloading nginx container: $NGINX_CONTAINER_NAME"
docker exec "$NGINX_CONTAINER_NAME" nginx -t
docker exec "$NGINX_CONTAINER_NAME" nginx -s reload

########################################
# 9) Cleanup
########################################
echo "==> Cleaning up temp dir: $TEMP_DIR"
rm -rf "$TEMP_DIR"

echo "==> Deployment of tag '$TAG' completed."
