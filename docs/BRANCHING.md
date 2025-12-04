# Git Branching & Release Strategy

This document defines how branching, development, testing, and releases are handled in this repository.

The workflow follows this core principle:

**`feature/*` → PR → `test` → PR → `main`**

- All development work happens in `feature/*` branches.
- Completed features are merged into `test` **via Pull Request**.
- When `test` is stable, it is merged into `main` **via Pull Request** and tagged as a release.
- The `test` branch may also receive **prerelease tags** (e.g., `v1.2.0-rc1`, `v1.2.0-beta`, or small fix tags).
- Temporary integration testing is done in `sandbox/*` branches.

---

## Branch Types

### 1. `main` (Production)
- Represents the **production** state of the application.
- Only updated through a **Pull Request from `test`**.
- Protected: no direct pushes or force-pushes.
- Stable releases are tagged on `main` (e.g., `v1.0.0`).

**Allowed operations:**
- Create PRs *into* `main` from `test`.
- Add stable release tags.

---

### 2. `test` (Staging / UAT)
- Represents the **next version** being prepared for production.
- Used for integration testing, UAT, and QA.
- Only updated through **Pull Requests from `feature/*`** branches.
- Can receive **prerelease tags**:
  - Release candidates → `v1.4.0-rc1`
  - Beta or alpha tags → `v1.4.0-beta`
  - Internal QA versions → `v1.4.0-test.3`
  - Small fix prereleases → `v1.4.1-rc1`

These tags help track internal testing builds before promoting to `main`.

**Allowed operations:**
- Create PRs *into* `test` from `feature/*`.
- Add prerelease or test tags (non-production).
- Create temporary `sandbox/*` branches.

---

### 3. `feature/*` (Active Development)
All new development happens here.

**Naming convention:**
```
feature/<short-kebab-case-description>
```

Examples:
- feature/initial-setup
- feature/user-profile
- feature/reporting-dashboard

**Lifecycle:**
1. Branch from `test`.
2. Commit and develop normally.
3. Open a PR into `test`.
4. Delete the feature branch after merge.

---

### 4. `sandbox/*` (Temporary Integration & Experiment Branches)
Used to integrate multiple **WIP features** or test experimental concepts **without polluting** the `test` branch.

These branches:
- Are temporary
- Are never merged into `test` or `main`
- Exist only to run integration checks or resolve conflicts early

**Naming convention:**
```
sandbox/<purpose-or-feature-combo>
```

Examples:
- sandbox/auth-plus-profile
- sandbox/reporting-ui-integration

**Lifecycle:**
1. Branch from `test`.
2. Merge any number of `feature/*` branches into it.
3. Test, experiment, review interactions.
4. Apply any required fixes back to the original feature branches.
5. Delete the sandbox branch when done.

---

### 5. `bugfix/*` (Optional)
Used for non-critical bug fixes discovered on `test`.

**Naming:**
```
bugfix/<short-description>
```

Flow:
bugfix/* → PR → test.

---

### 6. `hotfix/*` (Optional)
Used for **urgent production fixes**.

**Naming:**
```
hotfix/<short-description>
```

Flow:
1. Branch from main.
2. Fix the issue.
3. PR into main.
4. PR into test (to keep branches aligned).
5. Delete the hotfix branch.

---

## Standard Workflows

### 1. Creating a New Feature

1. Create the branch:
```
git switch test
git pull
git switch -c feature/<name>
```

2. Develop changes.

3. Open a Pull Request:
**feature/<name> → test**

4. After merge:
```
git branch -d feature/<name>
git push origin --delete feature/<name>
```

---

### 2. Testing Multiple WIP Features Together (Sandbox)

Example: integrating feature/auth and feature/profile.

1. Create a sandbox branch:
```
git switch test
git pull
git switch -c sandbox/auth-plus-profile
```

2. Merge features:
```
git merge feature/auth
git merge feature/profile
```

3. Fix interaction issues inside the feature branches (not sandbox).

4. Delete sandbox branch when finished:
```
git branch -D sandbox/auth-plus-profile
git push origin --delete sandbox/auth-plus-profile
```

---

### 3. Promoting test to main (Release)

When test is stable:

1. Create a Pull Request:
**test → main**

2. After merge, tag the stable release:
```
git tag -a v1.0.0 -m "Release v1.0.0"
git push --tags
```

---

### 4. Tagging Prereleases on test

During UAT or internal QA cycles, you can add prerelease tags directly on the `test` branch:

Examples:
```
git tag -a v1.2.0-rc1 -m "Release candidate 1"
git tag -a v1.2.0-beta -m "Beta version for QA"
git tag -a v1.2.1-test.2 -m "Internal test build 2"
git push --tags
```

Prerelease tags **should never** be placed on `main`.

---

### 5. Hotfix Workflow (Optional)

1. Branch from main:
```
git switch main
git pull
git switch -c hotfix/<desc>
```

2. Fix the issue.

3. Open two PRs:
- hotfix/* → main
- hotfix/* → test

4. Delete the branch after merging.

---

## Naming Guidelines

- Use lowercase with hyphens (kebab-case).
- Keep names short but descriptive:
  - feature/user-profile
  - sandbox/auth-plus-profile
  - bugfix/pagination-error
- Avoid reserved names like:
  - merge
  - reset
  - revert

---

## Summary

**Do**
- Use feature/* for all development work.
- Use sandbox/* for temporary integration.
- Use PRs for merging into test and main.
- Tag **prereleases on test**.
- Tag **stable releases on main**.
- Delete merged branches.

**Don’t**
- Do not push directly to main or test.
- Do not merge sandbox/* into stable branches.
- Do not place prerelease tags on main.
- Do not develop directly on test.

---

This Git strategy ensures:
- Clean, controlled progression of features
- Safe integration testing
- Predictable stable and prerelease tagging
- Minimal long-lived branches
- Easy collaboration across multiple features
