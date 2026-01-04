# Nomenclature Specification: Locations, Areas, Slots

This document defines a **consistent, compact, and human-readable naming convention** for physical locations, functional areas, and storage slots. The goal is to make identifiers:

* Short and scannable
* Easy to label physically
* Predictable and sortable
* Suitable for databases, QR codes, labels, and spoken references

---

## 1. Locations (Buildings / Apartments)

**Format:**

```
LLL
```

**Rules:**

* Exactly **3 uppercase letters**
* Must be **globally unique** within the system
* Stable over time (never reused for a different location)
* No numbers, no special characters

**Recommended semantics:**

* First letter → building / site group
* Remaining letters → specific unit or apartment

**Examples:**

| Code | Meaning                    |
| ---- | -------------------------- |
| HQA  | Headquarters – Apartment A |
| HQB  | Headquarters – Apartment B |
| CEN  | Central storage location   |

---

## 2. Areas

Areas are functional zones inside a location. They include rooms **and** non-room scopes like the whole apartment.

**Format:**

```
AAA | AA#
```

**Rules:**

* Exactly **3 characters**
* Combination of **uppercase letters and numbers**
* Unique **within a location** (not necessarily global)
* Numbers are used **only when multiple areas of the same type exist**

**Standard area codes (recommended):**

| Area                | Code(s)       | Notes                 |
| ------------------- | ------------- | --------------------- |
| Apartment (general) | APP           | Whole apartment scope |
| Kitchen             | KTC           |                       |
| Bathroom            | BT1, BT2, BTR | Numbered or generic   |
| Bedroom             | BD1, BD2, BDR | Numbered or generic   |
| Living room         | LVR           |                       |
| Dining room         | DNR           |                       |
| Hallway             | HLW           |                       |
| Balcony             | BLC           |                       |
| Office              | OF1, OFF      | Numbered or generic   |
| Storage room        | STO           |                       |
| Garage              | GAR           |                       |

**Guidelines:**

* Prefer **numbered variants** (`BD1`, `BT2`) when multiple areas exist
* Use **generic codes** (`BDR`, `BTR`, `OFF`) when uniqueness is implicit
* Do not mix numbering styles for the same area type in one location

**Custom areas** are allowed as long as they follow the 3-character rule.

---

## 3. Storage Slots (Shelves / Drawers / Vertical Storage)

A **slot** is a precise addressable storage position inside an AREA.

**Slot format:**

```
XXX-YYY
```

Both axes follow **the same rules**.

### Meaning

* **XXX** – horizontal address (left → right) from the **area entrance perspective**
* **YYY** – vertical address (top → bottom)

### Standard numeric form (default)

**Rules (applies to both `XXX` and `YYY`):**

* Three digits, zero-padded (`010–999`)
* Planned layout uses **steps of 10**: `010`, `020`, `030`, ...
* Insertions use any free value between (e.g. `015` between `010` and `020`)
* If something is added **before** the first planned position, use a lower value (e.g. `005` before `010`)
* Values are **addresses, not counters**
* Once assigned, a value is **never renumbered**

### Exception form (alphanumeric)

Used when the grid model doesn’t fit (e.g. counters, islands, irregular storage, hooks).

**Format (applies to both `XXX` and `YYY`):**

```
LNN
```

Where:

* `L` = uppercase letter `A–Z`
* `NN` = two digits, **step of 1** (`01–99`)

**Rules:**

* Exception codes are **unique within the AREA**
* They **do not imply** left→right or top→bottom ordering
* They are **never renumbered**

### Examples

| Slot      | Meaning                                                |
| --------- | ------------------------------------------------------ |
| `010-010` | First closet from the left, top shelf                  |
| `010-020` | First closet, second shelf from top                    |
| `005-010` | Closet added left of the first one, top shelf          |
| `015-020` | Closet inserted between first and second, second shelf |
| `A01-010` | Counter/island storage (exception X), top shelf        |
| `A01-B03` | Counter/island storage with exception Y position       |

---

## 4. Full Composite Identifier (Recommended)

Although each level can exist independently, the **full identifier** is strongly recommended:

```
LOCATION-AREA-SLOT
```

Where:

* `LOCATION` = 3-letter location code
* `AREA` = 3-character area code (`APP`, `KTC`, `BD1`, ...)
* `SLOT` = `XXX-YYY`

**Examples:**

| Identifier      | Meaning                                           |
| --------------- | ------------------------------------------------- |
| HQA-KTC-010-010 | Location HQA, kitchen area, first slot, top shelf |
| HQA-KTC-050-040 | Location HQA, kitchen area, slot 050-040          |
| HQA-APP-A01-010 | Location HQA, apartment scope, exception slot     |

---

## 5. Design Principles

* **Human-first**: readable without documentation
* **Label-friendly**: fits on small stickers
* **Sortable**: lexical sorting matches physical order
* **Extensible**: allows future expansion without breaking existing codes

---

## 6. Explicit Non-Goals

* No semantic meaning in numbers (only position)
* No dynamic renaming based on contents
* No automatic reuse of retired codes

---

## 7. Validation Summary

| Level        | Pattern                        |                      |              |
| ------------ | ------------------------------ | -------------------- | ------------ |
| Location     | `[A-Z]{3}`                     |                      |              |
| Area         | `[A-Z0-9]{3}`                  |                      |              |
| Slot segment | `\d{3}` or `[A-Z]\d{2}`        |                      |              |
| Slot         | `(?:\d{3}                      | [A-Z]\d{2})-(?:\d{3} | [A-Z]\d{2})` |
| Full         | `[A-Z]{3}-[A-Z0-9]{3}-(?:\d{3} | [A-Z]\d{2})-(?:\d{3} | [A-Z]\d{2})` |

---

