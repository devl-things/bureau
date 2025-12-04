## 🧱 Solution Structure Overview

The repository follows a layered (onion/clean) architecture. Projects are organized into solution folders that reflect responsibility boundaries and the allowed dependency direction.

---

### **Core**
Contains all fundamental building blocks and domain logic. Core projects do not depend on any other layer.

- **BuildingBlocks** – Shared primitives and utilities used across the entire solution  
  (e.g., `Result`, `PagedResult`, `PagingParameters`, logging helpers).
- **Domain** – Domain entities, value objects, domain services, and domain rules.  
  Pure business logic with no infrastructure dependencies.
- **Application** – Application services, use-case orchestration, and interfaces (ports)  
  that define how the domain is accessed. Depends only on Domain and Core building blocks.

---

### **Infrastructure**
Contains implementations of application interfaces and integrations with external systems.

- Database access (EF Core, SQL, caching, repositories)
- External service clients
- File storage, messaging, background processing
- Concrete runtime dependencies of the Application layer

Infrastructure depends on **Application**, **Domain**, and **Core**, but not vice-versa.

---

### **Presentation**
Contains delivery mechanisms and entry points of the system.

- REST API (`.Api` projects)
- Web/UI frontends
- Hosted/background worker applications

Presentation depends on **Application** (to execute use cases) and may reference Infrastructure at the composition root to register concrete implementations.

---

### **Tests**
Contains automated tests for different layers.

- **Unit Tests** – test Domain and Application behavior without infrastructure.
- **Integration Tests** – test Infrastructure components (database, external services).
- **End-to-End Tests** – test complete system flows through the Presentation layer.

---
