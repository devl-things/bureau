# bureau
This repository is a monorepo that contains multiple applications and shared components, for example:
- there are no examples at this point

All projects share a **single version** and follow a unified branching and release strategy.

## Repository Structure

At the root level:

- `docs/`  
  Shared documentation for the entire repository.  
  - `VERSIONING.md` – versioning rules and release process  
  - `BRANCHING.md` – Git branching and workflow  
  - `ARCHITECTURE.md` – high-level architecture overview  

- `server/`  
  Server-side solution for backend / web projects.  
  - `Bureau.sln` – main solution file for server projects  
  - `src/` – application source code (all server-side projects and features)  
  - `tests/` – test projects mirroring the structure of `src`  
  - `docs/` – documentation specific to server projects (implementation details, ADRs, etc.)  
  - `deploy/` – deployment-related configuration and scripts for the server solution  

- `mobile/`  
  Native mobile applications (planned / future). Currently empty.

## Git Branching & Release Strategy
Go to [BRANCHING.md](docs/BRANCHING.md)
## Versioning Rules
Go to [VERSIONING.md](docs/VERSIONING.md)
## Architecture Overview
Go to [ARCHITECTURE.md](docs/ARCHITECTURE.md)
## [WIP] Local Development Setup
   - Exact commands to run app locally 
   - Needed environment variables / config files  
   - Any special launch profiles / docker-compose usage  
## [WIP] Configuration
    - What config files exist (`appsettings.json`, `.env`, etc.)  
   - What must not be committed (secrets)  
   - How to override config for local dev  
## Licensing
This repository is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.