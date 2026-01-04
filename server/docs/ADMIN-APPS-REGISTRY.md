# Admin Apps Registry

## Purpose

The **Admin Apps Registry** defines the list of available admin applications and enables **global navigation** across independent admin hosts while using a shared admin frame.

It is the mechanism that allows:

* consistent sidebar/header navigation
* cross-app navigation (e.g. Chores → Items)
* independent admin hosts to remain decoupled

The registry is **data-only**. It contains no business logic and no authentication logic.

---

## Conceptual overview

Each admin application ("app"):

* is hosted independently (own ASP.NET host)
* renders its UI inside the shared admin frame
* appears as an entry in the global admin navigation

The Admin Apps Registry is the **source of truth** for this navigation.

---

## Registry model

### `AppDescriptor`

Each entry in the registry represents one admin application.

**Fields:**

* `Key` (string, required)

  * Unique identifier for the app (e.g. `chores`, `items`)
* `Title` (string, required)

  * Display name in navigation
* `Url` (string, required)

  * Absolute URL to the admin entry point (e.g. `https://chores.domain/admin`)
* `Icon` (string, optional)

  * Icon identifier used by the frame UI

Example:

```json
{
  "Key": "chores",
  "Title": "Chores",
  "Url": "https://chores.domain/admin",
  "Icon": "checklist"
}
```

---

## Configuration format

The registry is loaded from application configuration.

Recommended configuration section:

```json
"Admin": {
  "CurrentApp": "chores",
  "Apps": [
    {
      "Key": "chores",
      "Title": "Chores",
      "Url": "https://chores.domain/admin",
      "Icon": "checklist"
    },
    {
      "Key": "items",
      "Title": "Items",
      "Url": "https://items.domain/admin",
      "Icon": "box"
    }
  ]
}
```

### `CurrentApp`

* Identifies the application currently being rendered
* Used by the frame to highlight the active navigation item
* Prevents duplication of navigation logic inside features

---

## Loading and access

Each admin host:

* binds the `Admin` configuration section
* exposes the registry to the view layer
* passes the registry to the shared admin frame

No runtime calls to other services are required.

---

## Responsibilities

### Admin Apps Registry **does**:

* define available admin applications
* define navigation structure
* enable cross-host navigation

### Admin Apps Registry **does not**:

* handle authentication or authorization
* define permissions (can be extended later)
* perform health checks or runtime discovery

---

## Relationship to the Admin Frame

The shared admin frame:

* receives a list of `AdminAppDescriptor`
* renders navigation UI
* highlights the active app using `CurrentApp`

The frame does not know:

* how apps are hosted
* whether apps use React or Razor
* how authentication works

---

## Evolution

This registry can be extended in a backward-compatible way to support:

* required permissions/scopes
* feature flags
* app health indicators
* version badges

Initial implementation should keep the model minimal.

---

## Design principles

1. **Configuration-driven**
2. **Host-owned**
3. **No runtime coupling between apps**
4. **Frame remains UI-only**

---

## Summary

The Admin Apps Registry is a simple, shared configuration model that enables a unified admin navigation experience across multiple independent ASP.NET hosts while keeping them loosely coupled.
