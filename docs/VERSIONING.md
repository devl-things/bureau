# Versioning Rules

This repository follows **Semantic Versioning (SemVer)** with clear separation between
**production releases** and **prerelease (test) builds**.

The version format is:

```text
MAJOR.MINOR.PATCH[-PRERELEASE][+BUILD]
```

---

## Stable Release Versions (on `main`)

Stable versions are tagged **only on `main`**.

Examples:

- `v1.0.0`
- `v1.1.0`
- `v2.0.3`

**Rules:**

- Increment **MAJOR** when making incompatible changes.
- Increment **MINOR** when adding backward-compatible features.
- Increment **PATCH** when applying backward-compatible bug fixes.
- Do **not** use prerelease suffixes on stable releases.

---

## Prerelease Versions (on `test`)

Prerelease versions allow internal testing before promoting changes to `main`.

Prerelease tags are created **only on `test`**.

### Common types

#### 1. Release candidates (RC)

```text
v1.4.0-rc1
v1.4.0-rc2
```

Meaning: “Almost ready for production; expected to match the final release unless critical issues are found.”

#### 2. Beta builds

```text
v1.4.0-beta
v1.4.0-beta.2
```

Meaning: “Feature complete, but still under testing; not production-ready.”

#### 3. Alpha or experimental builds

```text
v1.4.0-alpha
v1.4.0-alpha.3
```

Meaning: “Early preview; may be unstable or incomplete.”

#### 4. Internal QA / test builds

```text
v1.4.1-test.1
v1.4.1-test.2
```

Meaning: “Internal test/UAT builds for a given upcoming patch or minor release.”

These tags help track internal testing builds before promoting to `main`.

---

## Build Metadata (Optional)

You may append metadata after `+` for CI or automated systems:

Examples:

```text
v1.4.0-rc1+build.345
v1.4.0-test.2+sha.92f1eab
```

Metadata **does not affect version precedence**; it is purely informational.

---

## Version Precedence (Which version is “newer”?)

Semantic Versioning defines strict ordering rules.

Example:

```text
1.0.0-alpha  <  1.0.0-beta  <  1.0.0-rc1  <  1.0.0
```

General rules:

- Prerelease versions come **before** the corresponding stable version.
- Numeric prereleases sort in natural order:
  - `rc1` < `rc2`
  - `beta.1` < `beta.2`
- Build metadata (`+build` or `+sha.x`) **does not change** ordering.

---

## How Versioning Relates to Branches

- **`feature/*` branches**: no tags by default; work in progress.
- **`test` branch**: receives **prerelease tags** for internal releases and UAT.
- **`main` branch**: receives **stable tags** for production releases.

Typical flow for a given version:

1. Develop features on `feature/*` branches.
2. Merge features into `test` via PR.
3. Tag prereleases on `test`:
   - `v1.4.0-alpha.1`
   - `v1.4.0-beta.1`
   - `v1.4.0-rc1`
4. Once stable, merge `test` into `main` via PR.
5. Tag a stable release on `main`:
   - `v1.4.0`

---

## Visual Diagram: Branch & Version Flow

The diagram below shows how versions travel from feature branches, through `test` with prerelease tags, and finally to `main` with stable tags.

```mermaid
flowchart LR
    subgraph FeatureBranches[feature/* branches]
        F1[feature/a]
        F2[feature/b]
        F3[feature/c]
    end

    subgraph TestBranch[test]
        T[test branch]
        TTag1((v1.4.0-alpha.1))
        TTag2((v1.4.0-beta.1))
        TTag3((v1.4.0-rc1))
    end

    subgraph MainBranch[main]
        M[main branch]
        MTag1((v1.4.0))
    end

    F1 -->|PR| T
    F2 -->|PR| T
    F3 -->|PR| T

    T --> TTag1
    T --> TTag2
    T --> TTag3

    T -->|PR (release)| M
    M --> MTag1
```

Interpretation:

- Multiple `feature/*` branches are merged into `test` via PRs.
- `test` receives several prerelease tags (`alpha`, `beta`, `rc`).
- Once ready, `test` is merged into `main` via a PR, and `main` receives a stable tag.

---

## Usage Summary

- Use **stable tags on `main`** for production releases (e.g. `v1.4.0`).
- Use **prerelease tags on `test`** for alpha, beta, RC, or internal test builds (e.g. `v1.4.0-rc1`).
- Follow SemVer when deciding whether to bump MAJOR, MINOR, or PATCH.
- Keep branching rules aligned with `BRANCHING.md`.
