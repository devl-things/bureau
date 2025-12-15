# Deployment

This repository uses a **tag-based deployment** workflow driven by a single
deployment script and a canonical environment file.

The deployment process is intentionally **environment-agnostic**:
all environment-specific values live in `.env`, which is excluded from git.

---

## Repository layout

```
server/deploy/
  deploy.sh
  .env                  # excluded from git
  docker-compose.yml
  nginx/
    https/              # NGINX server templates (rendered)
    snippets/           # NGINX snippets (copied as-is)
```

---

## Canonical environment file (`.env`)

The `.env` file serves **two different purposes**:

1) **Deployment configuration**
2) **Application runtime configuration**

It is intentionally **not committed to git**.

---

### 1) Deployment configuration (`DEPLOY__*`)

Only variables prefixed with `DEPLOY__` are loaded into the deployment script.

Required:

```env
DEPLOY__ROOT=/path/to/deployment/root
DEPLOY__NGINX_CONTAINER_NAME=nginx
```

Optional:

```env
DEPLOY__REPO_URL=https://example.com/repo.git
DEPLOY__NGINX_HTTP_DIR=/custom/nginx/https
DEPLOY__NGINX_SNIPPETS_DIR=/custom/nginx/snippets
```

The deploy script deliberately sources **only `DEPLOY__*` variables**
to avoid shell-side path conversion issues (notably on Windows / Git Bash).

---

### 2) Application runtime configuration

All non-`DEPLOY__*` variables are **not sourced by the deploy script**.

They are passed to the application **via Docker Compose** using `env_file: .env`.

Example:

```env
Api__BaseUrl=
Api__ChoresUrl=/api/chores
Api__PrioritizedChoresSegment=/prioritized-chores
```

---

## NGINX configuration model

| Location | Purpose | Processing |
|--------|--------|------------|
| `nginx/https/*.conf` | App-specific server blocks | Rendered with `envsubst` |
| `nginx/snippets/*.conf` | Shared nginx logic | Copied verbatim |

Notes:

- `nginx/https/*.conf` may include `${DEPLOY__...}` placeholders (rendered at deploy time).
- `nginx/snippets/*.conf` must **not** be rendered, because snippets often contain NGINX
  runtime variables like `$host`, `$remote_addr`, etc.

---

## Docker Compose requirements

Application services that require runtime configuration should include:

```yaml
env_file:
  - .env
```

The deploy script copies `.env` into the cloned tag directory so this works automatically.

---

## How to run a deployment

You can run the script from **any directory**. A temporary folder named after the tag
will be created in the current working directory.

```bash
/path/to/repo/server/deploy/deploy.sh v0.1.0
```

---

## What `deploy.sh` does

For a given git tag:

1. Loads only `DEPLOY__*` variables from `.env`
2. Creates a temporary directory `./<TAG>/`
3. Clones the repository and checks out `<TAG>`
4. Copies `.env` into the cloned tag
5. Renders NGINX configs from `nginx/https/` using `envsubst`
6. Copies NGINX snippets without rendering
7. Starts or updates the stack using Docker Compose
8. Validates and reloads the NGINX container
9. Cleans up the temporary directory

The script **only overwrites files present in the tag** and never deletes unrelated
NGINX configuration.

---

## Troubleshooting

### NGINX reload fails

Run:

```bash
docker exec <nginx-container> nginx -t
```

Then fix the reported file/line. After fixing, reload:

```bash
docker exec <nginx-container> nginx -s reload
```

### Application config values look wrong

- Confirm the service has `env_file: .env` in `docker-compose.yml`
- Confirm `.env` exists inside the checked-out tag directory after deploy
- Remember: the deploy script does **not** source non-`DEPLOY__*` variables

---

## Windows / Git Bash note

Sourcing variables whose values start with `/` in Git Bash can trigger
automatic path conversion.

To avoid this:
- the deploy script sources **only `DEPLOY__*` variables**
- application variables are handled exclusively by Docker Compose

---

## Adding another application

1. Add `DEPLOY__NGINX__<APP>__*` variables to `.env`
2. Add a new server template under `nginx/https/`
3. Ensure the application service uses `env_file: .env`
4. Deploy a new tag
