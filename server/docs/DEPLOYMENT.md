# Deployment

This repository uses a **tag-based deployment** workflow driven by:

- `server/deploy/deploy.sh` (deployment script)
- `server/deploy/.env` (canonical environment file; **not committed**)
- `server/deploy/docker-compose.yml` (stack definition, versioned by tag)
- `server/deploy/nginx/` (NGINX configuration templates, versioned by tag)

The deployment is **environment-agnostic**: environment-specific values are injected from `server/deploy/.env` at deploy time.

---

## Repository layout

```
server/deploy/
  deploy.sh
  .env                  # excluded from git
  docker-compose.yml
  nginx/
    https/              # http-scope includes; typically contain `server {}` blocks
    snippets/           # reusable includes
```

---

## Prerequisites

### Required
- `git`
- Docker + Docker Compose v2 (`docker compose`)
- Network access to clone from the configured repo remote

### Optional (template rendering)
The deployment script renders NGINX templates using `envsubst`.

- If you run the script in an environment that already has `envsubst` (e.g. Git Bash with gettext), nothing else is needed.
- If `envsubst` is missing, the script will fail fast with a clear error.

Verify `envsubst`:

```bash
command -v envsubst
envsubst --version
```

---

## Canonical environment file

Create `server/deploy/.env` (excluded from git). It contains:

### 1) Deployment variables

Required:

```env
DEPLOY__ROOT=/path/to/root
DEPLOY__NGINX_CONTAINER_NAME=nginx
```

Optional:

```env
DEPLOY__REPO_URL=<git repo url>
DEPLOY__NGINX_HTTP_DIR=<override target http-scope dir>
DEPLOY__NGINX_SNIPPETS_DIR=<override target snippets dir>
```

### 2) NGINX template variables

Files under `server/deploy/nginx/https/*.conf` and `server/deploy/nginx/snippets/*.conf` may contain placeholders like:

- `${DEPLOY__NGINX__APPKEY__SERVER_NAME}`
- `${DEPLOY__NGINX__APPKEY__WEB_UPSTREAM}`
- `${DEPLOY__NGINX__APPKEY__API_UPSTREAM}`

All placeholders are rendered using values from `server/deploy/.env` at deploy time.

---

## Running a deployment

Run from any folder. The script creates a temporary folder named after the tag under your current working directory.

```bash
/path/to/repo/server/deploy/deploy.sh v0.1.0
```

---

## What `deploy.sh` does

Given a tag:

1. Loads `server/deploy/.env` and exports all variables
2. Clones the repo at the specified tag into `./<TAG>/`
3. Copies canonical `.env` into the cloned tag (`./<TAG>/server/deploy/.env`)
4. Renders NGINX templates from the cloned tag using `envsubst`
5. Installs rendered configs into the target host directories:
   - `${DEPLOY__NUC_ROOT}/nginx/conf/https/`
   - `${DEPLOY__NUC_ROOT}/nginx/conf/snippets/`

   The script **only overwrites files present in the tag** and does not delete unrelated configs.

6. Starts/updates the stack using the tag’s `docker-compose.yml`
7. Validates and reloads the NGINX gateway container via `docker exec`
8. Cleans up the temporary folder

---

## Operational notes

### Container name vs compose service name
- `docker exec <CONTAINER_NAME> ...` uses the container name (e.g. `DEPLOY__NGINX_CONTAINER_NAME`)
- `docker compose restart <SERVICE_NAME>` uses the compose service name

This deployment script uses `docker exec`.

### Restart vs reload
- Template/config changes typically require only:
  - `nginx -t`
  - `nginx -s reload`
- Container volume/port changes require a container restart (outside this script unless you add it explicitly).
