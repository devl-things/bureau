## 🧭 Namespace Conventions

This solution uses a simplified and consistent namespace strategy that avoids leaking project structure into public APIs.

### **No `.Abstractions` in Namespaces**

Even though abstractions (interfaces, contracts, and service definitions) live in `*.Abstractions` projects, we do **not** include `.Abstractions` in namespaces.

Example:

* ✔ `namespace Niles.Chores.Services`
* ✘ `namespace Niles.Chores.Abstractions.Services`

**Rationale:**

* Consumers should not know or care which assembly a type comes from.
* Mirrors Microsoft’s pattern (`Microsoft.Extensions.Logging` and `Microsoft.Extensions.Logging.Abstractions` share the same namespace).
* Keeps the public API surface clean, stable, and easy to refactor internally.

---

### **Namespace = Feature Root**

Namespaces reflect the **feature** (e.g., Chores, Users, Billing), not the folder or project name.

Examples:

* `Niles.Chores.Services`
* `Niles.Chores.Commands`
* `Niles.Chores.Queries`
* `Niles.Chores.Repositories`

This keeps the API intuitive and avoids namespace noise.

---

### **Models Use the Feature Root Namespace**

Domain or application models sit in a `Models` folder for organizational clarity,
but their **namespace remains the feature root**, not `*.Models`.

Example:

* File: `Niles.Chores/Models/Chore.cs`
* Namespace: `Niles.Chores`

This is an intentional exception that keeps model types short, clean, and pleasant to use.

---

### **Primitives Follow the Same Pattern**

Shared primitives (e.g., `Result`, `Result<T>`, `ResultError`, `PagedResult<T>`, `PagingParameters`)
live physically inside a `Primitives` folder but use the root namespace of the core library:

Example:

* Folder: `Core/Primitives`
* Namespace: `Niles.Core`

Folder layout never forces namespace changes — clarity and consistency take priority.

---

If you'd like, I can also provide a Code Style section pairing these rules with folder conventions.
