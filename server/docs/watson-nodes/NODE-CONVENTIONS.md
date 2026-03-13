# Watson.Nodes – Naming & Data Conventions

This document defines canonical conventions used by the **Watson.Nodes** service.
All producers and consumers **must** follow these rules to ensure consistent storage, indexing, and interoperability.

---

## 1. Locale

### Purpose
Locales are used to localize attribute values such as labels and descriptions.

### Format
- Locale values MUST be a **BCP 47 language tag**.
- Examples:
  - `en`
  - `en-US`
  - `hr`
  - `hr-HR`

### Semantics
- `null` locale means **invariant / non-localized** value.
- The same attribute key MAY exist multiple times on a node for different locales.

### Service behavior (recommended)
When a locale is specified for read/search projection:
- Prefer attributes with the requested locale.
- MAY fall back to invariant values (`Locale = null`).
- Optional: MAY fall back to a configured default locale (e.g., `en`) if requested locale is missing.

The exact fallback behavior is defined by the service layer and should remain stable once published.

---

## 2. Scope

### Purpose
Scope partitions nodes into logical namespaces (e.g., apps, tenants, domains) while keeping NodeIds globally unique.

### Reserved value
- `global` – globally shared nodes (default)

### Allowed format
Scopes MUST:
- be lowercase
- contain only: `a-z`, `0-9`, `_`, `-`, `:`
- be 1–64 characters long

### Examples
- `global`
- `shoppinglist`
- `chores`
- `tenant:acme`
- `project:kitchen-renovation`

### Semantics
- Scope is treated as an **opaque partition key**.
- No hierarchy semantics are assumed by the system.

---

## 3. Attribute keys

### Purpose
Attribute keys identify metadata attached to nodes.

### Allowed format
Attribute keys MUST:
- be lowercase
- use `snake_case`
- contain only: `a-z`, `0-9`, `_`, `:`
- be 1–64 characters long

### Namespacing
Keys MAY be namespaced using `:` for organizational grouping:
- `label`
- `description`
- `fat_percent`
- `catalog:ean`
- `nutrition:kcal`

Namespaces have no built-in semantic meaning; they are a naming convention only.

---

## 4. Attribute values

Each attribute has:
- `Key`
- optional `Locale`
- `Type`
- exactly one value field populated (based on `Type`)

### Supported types
| Type   | Value field        | Meaning |
|--------|---------------------|---------|
| String | `ValueString`       | Text |
| Number | `ValueNumber`       | Numeric (decimal) |
| Bool   | `ValueBool`         | Boolean |
| Json   | `ValueJson`         | JSON serialized as text |
| Ref    | `RefNodeId`         | Reference to another node |
| Date   | `ValueDate`         | Date/time |

---

## 5. Ref attributes

### What `RefNodeId` means
`RefNodeId` is a typed pointer to another node. **The meaning comes from the attribute key**, not from the reference itself.

Example:
- `Key=brand`, `Type=Ref`, `RefNodeId=<id>` means “this node’s brand is that node”.

### What `RefNodeId` does NOT mean
It does NOT define:
- relationship type beyond the key name
- inverse relationship
- relationship metadata (weights, roles, quantities)
- many-to-many relationships

When you need richer semantics or relationship metadata, use a dedicated relationship/edge model (future).

---

## 6. Attribute projection (search)

Search endpoints return **projected nodes**, meaning:
- only a subset of attributes may be included in the response
- projection is determined by:
  1. client-requested attribute keys (if provided)
  2. otherwise, defaults defined per `NodeKind` (service policy)

Consumers MUST NOT assume that search results include all attributes.

---

## 7. Cursor semantics

Cursor paging is **exclusive**:
- `cursor` represents the last seen position
- results start strictly after the cursor

Example:
- `cursor = 345` ⇒ results where `Sequence > 345`

Rules:
- clients MUST use `nextCursor` returned by the server for the next request
- server MUST NOT re-emit records at the cursor position
- if a page returns no items, `nextCursor` MUST equal the input cursor

---

## 8. Canonical key

### Purpose
Canonical keys provide stable, human-meaningful identifiers for some node kinds.

### Rules
- Required for: `Item`, `Tag` (per current policy)
- Other kinds may have different rules (policy-defined)

### Format (recommended)
- lowercase
- URL-safe
- no whitespace

### Uniqueness
Enforce uniqueness per `(Kind, Scope, CanonicalKey)` (policy/DB constraint).

---

## 9. Node identity & versioning

- `NodeId` is globally unique (UUIDv7).
- `Version` increments on every logical mutation (attributes/status/canonical key/etc.).
- `Version` supports optimistic concurrency, cache invalidation, and change reasoning.

---

## 10. Validation recommendations

Service-side validation (recommended):
- `Scope`: `^[a-z0-9:_-]{1,64}$`
- `AttributeKey`: `^[a-z0-9:_]{1,64}$`
- `Locale`: accept BCP 47 (e.g., validate with `CultureInfo.GetCultureInfo(locale)` if you want strict validation)

Validation rules should be stable once published to avoid breaking consumers.
