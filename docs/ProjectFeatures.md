# Backend Assessment Project


## Περιεχόμενα

1. [Επισκόπηση Project](#1-επισκόπηση-project)
2. [Αρχιτεκτονική](#2-αρχιτεκτονική)
   - [Γιατί χρειάζεται ο διαχωρισμός των layers](#γιατί-χρειάζεται-ο-διαχωρισμός-των-layers-domain-application-infrastructure)
3. [Backend Features](#3-backend-features)
   - [3.1 CRUD Operations για Entities (Employees, Departments, Projects)](#31-crud-operations-για-entities-employees-departments-projects)
   - [3.2 Pagination](#32-pagination)
   - [3.3 Soft Delete](#33-soft-delete)
   - [3.4 Validation (FluentValidation)](#34-validation-fluentvalidation)
   - [3.5 Global Exception Handling](#35-global-exception-handling)
   - [3.6 Data Persistence](#36-data-persistence)
   - [3.7 Repository Pattern](#37-repository-pattern)
   - [3.8 Swagger/OpenAPI Documentation](#38-swaggeropenapi-documentation)
   - [3.9 Logging (Serilog)](#39-logging-serilog)
   - [3.10 Dependency Injection](#310-dependency-injection)
   - [3.11 Database Migrations](#311-database-migrations)
   - [3.12 Data Seeding](#312-data-seeding)
   - [3.13 CORS Configuration](#313-cors-configuration)
   - [3.14 Health Check Endpoint](#314-health-check-endpoint)
   - [3.15 Docker & Containerization](#315-docker--containerization)
4. [API Endpoints Summary](#4-api-endpoints-summary)
5. [Testing](#5-testing)

---

## 1. Επισκόπηση Project

### Tech Stack

**Backend:**
- **.NET 8** - ASP.NET Core Web API
- **Entity Framework Core** - ORM με SQL Server
- **FluentValidation** - Input validation
- **Swashbuckle (Swagger/OpenAPI)** - API documentation
- **Serilog** - Structured logging
- **xUnit** - Unit testing framework
- **Docker** - Containerization

### Project Structure

```
backend/
├── BackendProject.API/          # API Layer (Controllers, Middleware, Filters)
├── BackendProject.Application/  # Application Layer (Services, DTOs, Validators, Mappers)
├── BackendProject.Domain/       # Domain Layer (Entities, Interfaces, Enums)
└── BackendProject.Infrastructure/ # Infrastructure Layer (DbContext, Repositories, Migrations)
```

---

## 2. Αρχιτεκτονική

Το project ακολουθεί **Clean Architecture** με 4 layers:

| Layer | Project | Ευθύνη | Κύριοι Φάκελοι |
|-------|---------|--------|----------------|
| **API** | `BackendProject.API` | HTTP entry point, Controllers, Middleware, Filters | `Controllers/`, `Middleware/`, `Filters/`, `Program.cs` |
| **Application** | `BackendProject.Application` | Business logic, DTOs, Validators, Mappers, Services | `Services/`, `DTOs/`, `Validators/`, `Mappers/`, `Common/` |
| **Domain** | `BackendProject.Domain` | Entities, Domain interfaces, Enums | `Entities/`, `Interfaces/`, `Enums/`, `Common/` |
| **Infrastructure** | `BackendProject.Infrastructure` | Data access, EF Core, Repositories, Migrations | `Data/`, `Repositories/`, `Migrations/` |

### Γιατί χρειάζεται ο διαχωρισμός των layers (Domain, Application, Infrastructure)

Ο διαχωρισμός σε ξεχωριστά layers εξυπηρετεί συγκεκριμένους σκοπούς:

**1. Domain Layer - Ο πυρήνας χωρίς εξαρτήσεις**

- **Τι περιέχει:** Entities, interfaces (`IRepository<T>`, `ISaveChanges`), Enums
- **Γιατί ξεχωριστά:**
  - **Zero dependencies:** Το Domain δεν εξαρτάται από κανένα άλλο project (όχι EF, όχι HTTP, όχι SQL)
  - **Reusability:** Οι ίδιες entities και interfaces μπορούν να χρησιμοποιηθούν σε API, background jobs, console apps, ή ακόμα και σε διαφορετικό frontend
  - **Stability:** Ο business domain αλλάζει σπάνια· τα "how" (database, HTTP) αλλάζουν συχνότερα
  - **Testability:** Μπορείς να test-άρεις domain logic χωρίς database ή web server

**2. Application Layer - Η business logic που εξαρτάται μόνο από Domain**

- **Τι περιέχει:** Services, DTOs, Validators, Mappers
- **Γιατί ξεχωριστά:**
  - **Depends only on Domain:** Το Application γνωρίζει μόνο interfaces (`IRepository<T>`, `ISaveChanges`), όχι πώς υλοποιούνται
  - **Database-agnostic:** Δεν ξέρει αν τα δεδομένα έρχονται από SQL Server, PostgreSQL, MongoDB ή in-memory· απλά καλεί `repository.Query()` και `saveChanges.SaveChangesAsync()`
  - **Swappable implementations:** Αν αλλάξεις από EF Core σε Dapper, αλλάζεις μόνο Infrastructure· το Application μένει αμετάβλητο
  - **Easy unit testing:** Mock-άρεις τα `IRepository<T>` και `ISaveChanges` χωρίς πραγματική βάση

**3. Infrastructure Layer - Οι συγκεκριμένες υλοποιήσεις**

- **Τι περιέχει:** AppDbContext, Repository implementations, EF configurations, Migrations
- **Γιατί ξεχωριστά:**
  - **Contains "how":** Όλη η λογική "πώς μιλάμε στη βάση" ζει εδώ (EF Core, connection strings, SQL)
  - **Implements Domain interfaces:** Το `Repository<T>` υλοποιεί `IRepository<T>`, το `AppDbContext` υλοποιεί `ISaveChanges`
  - **Replaceable:** Μπορείς να αντικαταστήσεις ολόκληρο το Infrastructure project (π.χ. από EF σε Dapper) χωρίς να αγγίξεις Application ή Domain
  - **Framework details isolated:** EF-specific code (configurations, migrations) μένει μακριά από business logic

**4. API Layer - Το HTTP entry point**

- **Τι περιέχει:** Controllers, Middleware, Filters
- **Γιατί ξεχωριστά:**
  - **Thin layer:** Μεταφράζει HTTP requests σε service calls και επιστρέφει responses
  - **Replaceable transport:** Το ίδιο Application μπορεί να τροφοδοτηθεί από REST API, gRPC, ή message queue consumers
  - **Presentation concerns:** CORS, Swagger, exception formatting ανήκουν εδώ, όχι στο business logic

**5. Dependency Rule - Οι εξαρτήσεις δείχνουν προς τα μέσα**

```
API ──────────► Application ──────────► Domain
  │                     │
  │                     │ (depends on interfaces only)
  │                     │
  └─────────────────────┼─────────────────────────► Infrastructure
                        │                                    │
                        │                                    │ implements
                        │                                    ▼
                        └──────────────────────────────► Domain interfaces
```

- **Domain:** Κανένα reference σε άλλα projects
- **Application:** Reference μόνο στο Domain (χρησιμοποιεί `IRepository<T>`, `ISaveChanges`)
- **Infrastructure:** Reference στο Domain (υλοποιεί τις interfaces)
- **API:** Reference στο Application και Infrastructure (για DI registration)

**Πρακτικό παράδειγμα - Τι κερδίζουμε:**

| Σενάριο | Χωρίς διαχωρισμό | Με Clean Architecture |
|---------|-------------------|------------------------|
| Αλλαγή database (SQL → PostgreSQL) | Αλλαγές σε όλο το codebase | Αλλαγές μόνο στο Infrastructure |
| Unit tests χωρίς database | Δύσκολο (πρέπει να mock-άρεις ολόκληρο DbContext) | Εύκολο (mock `IRepository<T>` και `ISaveChanges`) |
| Νέο API (π.χ. gRPC δίπλα στο REST) | Duplicate business logic | Ίδιο Application, νέο API project |
| Αλλαγή ORM (EF → Dapper) | Refactor σε πολλά αρχεία | Νέο Infrastructure project, Application αμετάβλητο |

**Design Patterns:**
- Repository Pattern (`IRepository<T>`, `Repository<T>`)
- Dependency Injection (Constructor injection)
- DTO Pattern (Request/Response DTOs)

---

## 3. Backend Features

### 3.1 CRUD Operations για Entities (Employees, Departments, Projects)

**Feature:** Πλήρες CRUD operations για διαχείριση των τριών main entities (Employees, Departments, Projects) με search, pagination, και relationship management.

**Κοινή Αρχιτεκτονική:**

Όλα τα entities ακολουθούν το ίδιο pattern με τα εξής components:

| Component | Τι είναι | Ευθύνη | Πού βρίσκεται |
|-----------|----------|--------|---------------|
| **Controller** | HTTP entry point | Δέχεται HTTP requests, καλεί Services, επιστρέφει HTTP responses | `BackendProject.API/Controllers/{Entity}Controller.cs` |
| **Service** | Business logic layer | Περιέχει business logic, καλεί Repositories και ISaveChanges, map-άρει entities ↔ DTOs | `BackendProject.Application/Services/{Entity}Service.cs` |
| **Interface** | Service contract | Ορίζει τα methods που πρέπει να υλοποιήσει το Service | `BackendProject.Application/Interfaces/I{Entity}Service.cs` |
| **DTOs** | Data Transfer Objects | Request/Response objects για API communication (ΔΕΝ είναι το table schema - αυτό είναι το Entity) | `BackendProject.Application/DTOs/{Entity}Dtos.cs` |
| **Validators** | Validation rules | FluentValidation rules για input validation | `BackendProject.Application/Validators/{Entity}Validators.cs` |
| **Mapper** | Entity ↔ DTO conversion | Μετατρέπει entities σε DTOs και αντίστροφα | `BackendProject.Application/Mappers/{Entity}Mapper.cs` |
| **Entity** | Domain model / Table schema | Κλάση που αντιπροσωπεύει τον πίνακα στη βάση (αυτό είναι το table schema) | `BackendProject.Domain/Entities/{Entity}.cs` |
| **Configuration** | EF Core configuration | Fluent API configuration για relationships και constraints | `BackendProject.Infrastructure/Data/Configurations/{Entity}Configuration.cs` |

**Σημαντική Διαφορά - DTOs vs Entities:**

- **Entity:** Αντιπροσωπεύει το **table schema** στη βάση. Map-άρεται απευθείας σε database table με EF Core. Περιέχει navigation properties, relationships, και όλα τα fields της βάσης.
- **DTOs:** ΔΕΝ είναι το table schema. Είναι **separate objects** που χρησιμοποιούνται για:
  - Μεταφορά δεδομένων μέσω HTTP (API requests/responses)
  - Διαχωρισμό του internal entity structure από το public API contract
  - Έλεγχο τι data εκτίθεται στους clients
  - Versioning του API χωρίς να αλλάξει το entity
  - Μείωση coupling μεταξύ layers

**Παράδειγμα:** Ένα `Employee` entity μπορεί να έχει `IsDeleted`, `DeletedAt` (internal fields), αλλά το `EmployeeResponse` DTO μπορεί να μην τα περιλαμβάνει γιατί δεν θέλουμε να τα εκθέσουμε στο API.

**Πού βρίσκονται τα Components για κάθε Entity:**

**Employees:**
- **Controller:** [`backend/BackendProject.API/Controllers/EmployeesController.cs`](backend/BackendProject.API/Controllers/EmployeesController.cs)
- **Service:** [`backend/BackendProject.Application/Services/EmployeeService.cs`](backend/BackendProject.Application/Services/EmployeeService.cs)
- **Interface:** [`backend/BackendProject.Application/Interfaces/IEmployeeService.cs`](backend/BackendProject.Application/Interfaces/IEmployeeService.cs)
- **DTOs:** [`backend/BackendProject.Application/DTOs/EmployeeDtos.cs`](backend/BackendProject.Application/DTOs/EmployeeDtos.cs)
- **Validators:** [`backend/BackendProject.Application/Validators/EmployeeValidators.cs`](backend/BackendProject.Application/Validators/EmployeeValidators.cs)
- **Mapper:** [`backend/BackendProject.Application/Mappers/EmployeeMapper.cs`](backend/BackendProject.Application/Mappers/EmployeeMapper.cs)
- **Entity:** [`backend/BackendProject.Domain/Entities/Employee.cs`](backend/BackendProject.Domain/Entities/Employee.cs)
- **Configuration:** [`backend/BackendProject.Infrastructure/Data/Configurations/EmployeeConfiguration.cs`](backend/BackendProject.Infrastructure/Data/Configurations/EmployeeConfiguration.cs)

**Departments:**
- **Controller:** [`backend/BackendProject.API/Controllers/DepartmentsController.cs`](backend/BackendProject.API/Controllers/DepartmentsController.cs)
- **Service:** [`backend/BackendProject.Application/Services/DepartmentService.cs`](backend/BackendProject.Application/Services/DepartmentService.cs)
- **Interface:** [`backend/BackendProject.Application/Interfaces/IDepartmentService.cs`](backend/BackendProject.Application/Interfaces/IDepartmentService.cs)
- **DTOs:** [`backend/BackendProject.Application/DTOs/DepartmentDtos.cs`](backend/BackendProject.Application/DTOs/DepartmentDtos.cs)
- **Validators:** [`backend/BackendProject.Application/Validators/DepartmentValidators.cs`](backend/BackendProject.Application/Validators/DepartmentValidators.cs)
- **Mapper:** [`backend/BackendProject.Application/Mappers/DepartmentMapper.cs`](backend/BackendProject.Application/Mappers/DepartmentMapper.cs)
- **Entity:** [`backend/BackendProject.Domain/Entities/Department.cs`](backend/BackendProject.Domain/Entities/Department.cs)
- **Configuration:** [`backend/BackendProject.Infrastructure/Data/Configurations/DepartmentConfiguration.cs`](backend/BackendProject.Infrastructure/Data/Configurations/DepartmentConfiguration.cs)

**Projects:**
- **Controller:** [`backend/BackendProject.API/Controllers/ProjectsController.cs`](backend/BackendProject.API/Controllers/ProjectsController.cs)
- **Service:** [`backend/BackendProject.Application/Services/ProjectService.cs`](backend/BackendProject.Application/Services/ProjectService.cs)
- **Interface:** [`backend/BackendProject.Application/Interfaces/IProjectService.cs`](backend/BackendProject.Application/Interfaces/IProjectService.cs)
- **DTOs:** [`backend/BackendProject.Application/DTOs/ProjectDtos.cs`](backend/BackendProject.Application/DTOs/ProjectDtos.cs)
- **Validators:** [`backend/BackendProject.Application/Validators/ProjectValidators.cs`](backend/BackendProject.Application/Validators/ProjectValidators.cs)
- **Mapper:** [`backend/BackendProject.Application/Mappers/ProjectMapper.cs`](backend/BackendProject.Application/Mappers/ProjectMapper.cs)
- **Entity:** [`backend/BackendProject.Domain/Entities/Project.cs`](backend/BackendProject.Domain/Entities/Project.cs)
- **Configuration:** [`backend/BackendProject.Infrastructure/Data/Configurations/ProjectConfiguration.cs`](backend/BackendProject.Infrastructure/Data/Configurations/ProjectConfiguration.cs)

**Endpoints ανά Entity:**

**Employees Endpoints:**
- `GET /api/employees` - Λήψη όλων των employees (paginated)
- `GET /api/employees/{id}` - Λήψη employee by ID (με projects)
- `GET /api/employees/search?q={term}` - Αναζήτηση employees (by first name, last name, or email)
- `POST /api/employees` - Δημιουργία νέου employee
- `PUT /api/employees/{id}` - Ενημέρωση employee
- `DELETE /api/employees/{id}` - Soft delete employee
- `POST /api/employees/{id}/projects/{projectId}` - Ανάθεση employee σε project
- `DELETE /api/employees/{id}/projects/{projectId}` - Αφαίρεση employee από project

**Departments Endpoints:**
- `GET /api/departments` - Λήψη όλων των departments (paginated)
- `GET /api/departments/{id}` - Λήψη department by ID
- `GET /api/departments/search?q={term}` - Αναζήτηση departments (by name or description)
- `GET /api/departments/{id}/employees` - Λήψη employees σε department
- `POST /api/departments` - Δημιουργία νέου department
- `PUT /api/departments/{id}` - Ενημέρωση department
- `DELETE /api/departments/{id}` - Soft delete department

**Projects Endpoints:**
- `GET /api/projects` - Λήψη όλων των projects (paginated)
- `GET /api/projects/{id}` - Λήψη project by ID
- `GET /api/projects/search?q={term}` - Αναζήτηση projects (by name or description)
- `POST /api/projects` - Δημιουργία νέου project
- `PUT /api/projects/{id}` - Ενημέρωση project
- `DELETE /api/projects/{id}` - Soft delete project

**Employee-Project Assignments:**

**Πού βρίσκεται:**
- **Controller Methods:** [`backend/BackendProject.API/Controllers/EmployeesController.cs`](backend/BackendProject.API/Controllers/EmployeesController.cs) (lines 113-137)
- **Service Methods:** [`backend/BackendProject.Application/Services/EmployeeService.cs`](backend/BackendProject.Application/Services/EmployeeService.cs) (AssignToProjectAsync, RemoveFromProjectAsync)
- **Join Entity:** [`backend/BackendProject.Domain/Entities/EmployeeProject.cs`](backend/BackendProject.Domain/Entities/EmployeeProject.cs)
- **Configuration:** [`backend/BackendProject.Infrastructure/Data/Configurations/EmployeeProjectConfiguration.cs`](backend/BackendProject.Infrastructure/Data/Configurations/EmployeeProjectConfiguration.cs)

**Endpoints:**
- `POST /api/employees/{id}/projects/{projectId}` - Ανάθεση employee σε project
- `DELETE /api/employees/{id}/projects/{projectId}` - Αφαίρεση employee από project

**Key Features ανά Entity:**

**Employees:**
- Email uniqueness validation
- Department relationship (One-to-Many)
- Project assignments (Many-to-Many)
- Employee status (Active/Inactive enum)
- Search by first name, last name, or email
- Detailed response includes assigned projects

**Departments:**
- Name uniqueness validation
- Optional description field
- Employee listing by department
- Search by name or description

**Projects:**
- Start date και optional end date
- Date validation (end date must be after start date)
- Many-to-many relationship με employees
- Search by name or description

**Employee-Project Assignments:**
- Idempotent assignment (no error αν already assigned)
- Atomic operations via EF SaveChangesAsync
- Validation of employee και project existence
- Detailed error messages για missing entities

---

### 3.2 Pagination

**Feature:** Server-side pagination για όλα τα list endpoints με configurable page size.

**Τι είναι:** Η διαίρεση μεγάλων result sets σε μικρότερες "σελίδες" δεδομένων. Αντί να επιστρέφονται όλες οι εγγραφές, επιστρέφονται μόνο οι εγγραφές της συγκεκριμένης σελίδας.

**Γιατί χρησιμοποιείται:**
- **Performance:** Φόρτωση 10,000 εγγραφών είναι αργή. Φόρτωση 10 εγγραφών ανά σελίδα είναι γρήγορη.
- **Network Efficiency:** Μειώνει τη μεταφορά δεδομένων
- **User Experience:** Οι χρήστες μπορούν να πλοηγηθούν σελίδα-σελίδα
- **Scalability:** Λειτουργεί αποτελεσματικά ακόμα και με εκατομμύρια εγγραφές

**Πού βρίσκεται:**
- **Common:** [`backend/BackendProject.Application/Common/PaginationParams.cs`](backend/BackendProject.Application/Common/PaginationParams.cs) (περιέχει `PaginationParams` και `PaginatedResult<T>`)
- **Extension:** [`backend/BackendProject.Application/Common/QueryableExtensions.cs`](backend/BackendProject.Application/Common/QueryableExtensions.cs)

**Πώς λειτουργεί:**
1. Client στέλνει request με `pageNumber` (π.χ. 1) και `pageSize` (π.χ. 10)
2. Server υπολογίζει total count των matching records
3. Server χρησιμοποιεί SQL `SKIP` και `TAKE` για να πάρει μόνο τη requested σελίδα
4. Server επιστρέφει: τα items της σελίδας + metadata (total count, total pages, has next/previous)

**Implementation Details:**
- **PaginationParams:** Input class με `PageNumber` και `PageSize` (max 100 για να αποφευχθεί abuse)
- **PaginatedResult<T>:** Output class που περιέχει:
  - `Items` - Τα actual data για την τρέχουσα σελίδα
  - `TotalCount` - Συνολικός αριθμός records που match το query
  - `TotalPages` - Υπολογίζεται ως `Ceiling(TotalCount / PageSize)`
  - `HasPrevious` - True αν υπάρχει previous σελίδα
  - `HasNext` - True αν υπάρχει next σελίδα
- **ToPaginatedResultAsync:** Extension method που εφαρμόζει pagination σε οποιοδήποτε IQueryable

**Usage Example:**
```csharp
// Στο Controller
var pagination = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
var result = await _employeeService.GetAllAsync(pagination, cancellationToken);
// Returns: { Items: [10 employees], TotalCount: 150, TotalPages: 15, HasNext: true, HasPrevious: false }

// Στο Service
var query = _employees.Query().Include(e => e.Department);
return await query.ToPaginatedResultAsync(pagination, EmployeeMapper.ToResponse, cancellationToken);
```

---

### 3.3 Soft Delete

**Feature:** Logical deletion των entities αντί για physical removal από τη βάση.

**Τι είναι:** Το Soft Delete είναι μια στρατηγική διαγραφής όπου οι εγγραφές σημειώνονται ως "διαγραμμένες" αλλά παραμένουν στη βάση. Αντί για physical removal (hard delete), το σύστημα θέτει ένα flag (`IsDeleted = true`) και ένα timestamp (`DeletedAt`). Η εγγραφή στη συνέχεια αποκρύπτεται αυτόματα από όλα τα queries.

**Γιατί χρησιμοποιείται:**
- **Data Recovery:** Οι διαγραμμένες εγγραφές μπορούν να "αναστραφούν" αν χρειαστεί
- **Audit Trail:** Μπορείς να δεις πότε και τι διαγράφηκε
- **Referential Integrity:** Οι related records δεν γίνονται orphaned (foreign keys ακόμα λειτουργούν)
- **Compliance:** Ορισμένοι κανονισμοί απαιτούν διατήρηση διαγραμμένων δεδομένων για μια περίοδο
- **Analytics:** Μπορείς να αναλύσεις patterns διαγραφής με την πάροδο του χρόνου

**Πού βρίσκεται:**
- **Base Entity:** [`backend/BackendProject.Domain/Common/BaseEntity.cs`](backend/BackendProject.Domain/Common/BaseEntity.cs)
- **DbContext:** [`backend/BackendProject.Infrastructure/Data/AppDbContext.cs`](backend/BackendProject.Infrastructure/Data/AppDbContext.cs)
- **Repository:** [`backend/BackendProject.Infrastructure/Repositories/Repository.cs`](backend/BackendProject.Infrastructure/Repositories/Repository.cs)

**Πώς λειτουργεί:**
1. **BaseEntity Properties:**
   - `IsDeleted` (bool) - Flag που δείχνει αν το entity είναι διαγραμμένο (default: false)
   - `DeletedAt` (DateTime?) - Timestamp όταν έγινε η διαγραφή (null αν δεν είναι διαγραμμένο)

2. **Global Query Filter:**
   - Εφαρμόζεται αυτόματα στο `AppDbContext.OnModelCreating`
   - Filter: `HasQueryFilter(e => !e.IsDeleted)`
   - Effect: Όλα τα queries αποκρύπτουν αυτόματα τα soft-deleted entities
   - Example: `context.Employees.ToList()` επιστρέφει μόνο employees όπου `IsDeleted == false`

3. **Soft Delete Process:**
   - Όταν καλείται `DELETE /api/employees/{id}`:
     - Service καλεί `Repository.SoftDeleteAsync(id)`
     - Repository θέτει `IsDeleted = true` και `DeletedAt = DateTime.UtcNow`
     - Entity state αλλάζει σε `Modified` (όχι `Deleted`)
     - Στο επόμενο query, το entity αποκρύπτεται αυτόματα

4. **Physical Delete Interception:**
   - Αν ο κώδικας προσπαθήσει να κάνει physical delete (`context.Remove(entity)`):
     - Το `SaveChanges` override το intercept
     - Μετατρέπει `EntityState.Deleted` σε `EntityState.Modified`
     - Θέτει `IsDeleted = true` και `DeletedAt = DateTime.UtcNow`
     - Το physical delete αποτρέπεται

**Implementation Details:**
- Όλα τα entities (Employee, Department, Project) κληρονομούν από `BaseEntity`
- Query filter εφαρμόζεται σε όλα τα BaseEntity types αυτόματα
- `Repository.SoftDeleteAsync` παρέχει explicit soft delete method
- `SaveChanges` override εξασφαλίζει ότι δεν γίνονται physical deletes

**Benefits:**
- Διαγραμμένοι employees δεν εμφανίζονται στις λίστες αυτόματα
- Μπορείς να ανακτήσεις διαγραμμένα δεδομένα αν χρειαστεί
- Διατηρεί relationships (π.χ. employee's projects ακόμα reference τον employee)
- Παρέχει audit trail των διαγραφών

---

### 3.4 Validation (FluentValidation)

**Feature:** Comprehensive input validation χρησιμοποιώντας FluentValidation με async checks και database validation.

**Τι είναι:** Η Validation είναι η διαδικασία ελέγχου ότι τα incoming data πληρούν ορισμένους κανόνες πριν την επεξεργασία. Το FluentValidation είναι μια βιβλιοθήκη που παρέχει ένα fluent, readable τρόπο για να ορίσεις validation rules με support για sync και async validation.

**Γιατί χρησιμοποιείται:**
- **Data Quality:** Εξασφαλίζει ότι μόνο valid data μπαίνει στο σύστημα
- **Security:** Αποτρέπει invalid ή malicious input
- **User Experience:** Παρέχει clear error messages για το τι είναι λάθος
- **Business Rules:** Εφαρμόζει domain rules (π.χ. email πρέπει να είναι unique)
- **Separation of Concerns:** Η validation logic είναι χωρισμένη από το business logic
- **Early Validation:** Validation συμβαίνει πριν το request φτάσει στο business logic

**Πού βρίσκεται:**
- **Filter:** [`backend/BackendProject.API/Filters/ValidationFilter.cs`](backend/BackendProject.API/Filters/ValidationFilter.cs)
- **Validators:**
  - [`backend/BackendProject.Application/Validators/EmployeeValidators.cs`](backend/BackendProject.Application/Validators/EmployeeValidators.cs)
  - [`backend/BackendProject.Application/Validators/DepartmentValidators.cs`](backend/BackendProject.Application/Validators/DepartmentValidators.cs)
  - [`backend/BackendProject.Application/Validators/ProjectValidators.cs`](backend/BackendProject.Application/Validators/ProjectValidators.cs)
- **Registration:** [`backend/BackendProject.Application/DependencyInjection.cs`](backend/BackendProject.Application/DependencyInjection.cs)

**Πώς λειτουργεί:**

**ValidationFilter Process:**
1. **Request φτάνει** στο controller action
2. **ValidationFilter** intercept πριν εκτελεστεί το action (via `IAsyncActionFilter`)
3. **Filter εξάγει route ID** (για Update operations) και το set-άρει στο DTO
4. **Filter βρίσκει validator** για κάθε request DTO από DI container (χρησιμοποιώντας `IValidator<T>`)
5. **Validator τρέχει** όλους τους validation rules (sync και async)
6. **Αν invalid:** Πετάει `ValidationException` με λίστα errors
7. **GlobalExceptionMiddleware** πιάνει exception και επιστρέφει 400 Bad Request με errors
8. **Αν valid:** Request συνεχίζει στο controller action

**Validator Implementation Pattern:**

Κάθε validator κληρονομεί από `AbstractValidator<T>` και ορίζει rules με fluent syntax. Validators inject συγκεκριμένα repositories για async database checks:

```csharp
public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeValidator(IRepository<Employee> employees, IRepository<Department> departments)
    {
        // Sync validation rules
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");
        
        // Async validation rules (database checks)
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MustAsync(async (email, cancellation) =>
            {
                var exists = await employees.AnyAsync(
                    e => e.Email.ToLower() == email.ToLower(),
                    cancellation);
                return !exists;
            })
            .WithMessage("An employee with this email address already exists.");
    }
}
```

**Validation Rules Examples:**

**Employee Validation (`CreateEmployeeValidator` & `UpdateEmployeeValidator`):**
- `FirstName`: Required, max 100 characters
- `LastName`: Required, max 100 characters
- `Email`: Required, valid email format, max 256 characters, must be unique (async database check)
- `DepartmentId`: Required, must reference existing department (async database check)
- `HireDate`: Required, cannot be in future
- `Status`: Required, must be valid enum value (`IsInEnum()`)
- `Notes`: Optional, max 1000 characters αν provided

**Department Validation:**
- `Name`: Required, max 200 characters, must be unique (async database check)
- `Description`: Optional, max 1000 characters αν provided

**Project Validation:**
- `Name`: Required, max 200 characters
- `StartDate`: Required
- `EndDate`: Optional, αλλά αν provided πρέπει να είναι μετά το `StartDate` (custom rule με `GreaterThan()`)
- `Description`: Optional, max 1000 characters αν provided

**Key FluentValidation Methods Used:**
- `NotEmpty()` - Field cannot be empty
- `EmailAddress()` - Validates email format
- `MaximumLength(n)` - String length validation
- `IsInEnum()` - Enum value validation
- `LessThanOrEqualTo()` - Date comparison
- `GreaterThan()` - Date comparison
- `MustAsync()` - Custom async validation (database checks)
- `WithMessage()` - Custom error message

**Error Response Format:**
Όταν η validation αποτύχει, το API επιστρέφει:
```json
{
  "message": "Validation failed",
  "errors": [
    {
      "property": "Email",
      "message": "An employee with this email address already exists."
    },
    {
      "property": "FirstName",
      "message": "First name is required"
    }
  ]
}
```

**ValidationFilter Implementation Details:**
- Χρησιμοποιεί `IAsyncActionFilter` για async execution
- Extract-άρει route parameters (ID) και τα set-άρει στο DTO για Update operations
- Βρίσκει validators από DI container με reflection (`IValidator<T>`)
- Execute-άρει validation με `ValidateAsync()`
- Πετάει `ValidationException` με `ValidationFailure` objects

**Benefits:**
- Validation συμβαίνει αυτόματα για όλα τα requests (via filter)
- Clear, specific error messages με custom messages
- Υποστηρίζει sync και async validation
- Μπορεί να ελέγξει database (π.χ. uniqueness, foreign keys) κατά τη διάρκεια validation
- Αποτρέπει invalid data από το να φτάσει στο business logic
- Separation of concerns: validation logic είναι χωρισμένη από controllers και services
- Reusable validators: ίδιοι validators μπορούν να χρησιμοποιηθούν σε multiple contexts
- Testable: validators μπορούν να test-αρούν in isolation

---

### 3.5 Global Exception Handling

**Feature:** Centralized exception handling με appropriate HTTP status codes και error messages.

**Τι είναι:** Το Global Exception Handling είναι ένας centralized μηχανισμός που πιάνει όλες τις unhandled exceptions στην εφαρμογή και τις μετατρέπει σε appropriate HTTP responses. Αντί να αφήνει exceptions να bubble up και να crash την εφαρμογή, τις πιάνει, τις logάρει, και επιστρέφει user-friendly error messages.

**Γιατί χρησιμοποιείται:**
- **Consistency:** Όλα τα errors ακολουθούν το ίδιο format
- **Security:** Αποτρέπει exposure internal error details στους clients
- **User Experience:** Παρέχει clear, actionable error messages
- **Debugging:** Logάρει detailed error information για developers
- **Reliability:** Η εφαρμογή δεν crash-άρει σε unexpected errors

**Πού βρίσκεται:**
- **Middleware:** [`backend/BackendProject.API/Middleware/GlobalExceptionMiddleware.cs`](backend/BackendProject.API/Middleware/GlobalExceptionMiddleware.cs)
- **Registration:** [`backend/BackendProject.API/Program.cs`](backend/BackendProject.API/Program.cs) (line 112)

**Πώς λειτουργεί:**
1. **Middleware τυλίγει** ολόκληρο το request pipeline
2. **Request ρέει** μέσω middleware → controllers → services → repositories
3. **Αν exception συμβεί** οπουδήποτε στο pipeline:
   - Exception πιάνεται από middleware
   - Exception type αναγνωρίζεται
   - Appropriate HTTP status code καθορίζεται
   - Error response format-άρει
   - Exception log-άρεται (με full details για developers)
   - Error response στέλνεται στον client

**Exception Handling Rules:**

| Exception Type | HTTP Status | Use Case | Example |
|---------------|------------|----------|---------|
| **ValidationException** | 400 Bad Request | Input validation failed | Email format invalid, required field missing |
| **KeyNotFoundException** | 404 Not Found | Resource doesn't exist | Employee με ID δεν βρέθηκε |
| **ArgumentException** | 400 Bad Request | Invalid argument passed | Negative page number |
| **DbUpdateException (SQL 2627)** | 409 Conflict | Unique constraint violation | Email already exists (caught at DB level) |
| **DbUpdateException (SQL 547)** | 400 Bad Request | Foreign key violation | Προσπάθεια διαγραφής department με employees |
| **DbUpdateException (Other)** | 500 Internal Server Error | Other database errors | Connection timeout, constraint violation |
| **Default (Any other)** | 500 Internal Server Error | Unexpected errors | Null reference, divide by zero |

**Error Response Format:**
```json
// Validation Error (400)
{
  "message": "Validation failed",
  "errors": [
    { "property": "Email", "message": "Email is required" },
    { "property": "FirstName", "message": "First name cannot exceed 100 characters" }
  ]
}

// Not Found Error (404)
{
  "message": "Employee with ID 123e4567-e89b-12d3-a456-426614174000 not found"
}

// Conflict Error (409)
{
  "message": "A record with this value already exists"
}

// Server Error (500)
{
  "message": "An internal server error occurred"
}
```

**Logging:**
- Όλες οι exceptions log-άρονται με full details (stack trace, inner exceptions)
- Logs περιλαμβάνουν: Exception type, message, request path, HTTP method
- Logs βοηθούν developers να debug issues χωρίς να εκθέτουν details στους clients
- Χρησιμοποιεί Serilog για structured logging

**Benefits:**
- Consistent error format σε ολόκληρο το API
- Καμία unhandled exception δεν crash-άρει την εφαρμογή
- Clear error messages βοηθούν clients να καταλάβουν τι πήγε στραβά
- Detailed logs βοηθούν developers να debug issues
- Security: Internal error details δεν εκτίθενται στους clients

---

### 3.6 Data Persistence

**Feature:** Atomic data persistence via EF Core `SaveChangesAsync` με `ISaveChanges` interface για clean architecture layering.

**Τι είναι:** Η persistence των αλλαγών στη βάση γίνεται μέσω του `ISaveChanges.SaveChangesAsync()`. Το Entity Framework Core εξασφαλίζει ότι κάθε κλήση `SaveChangesAsync` είναι **atomic** - είτε όλες οι αλλαγές στο change tracker επιτυγχάνουν μαζί, είτε καμία δεν αποθηκεύεται.

**Γιατί χρησιμοποιείται:**
- **Atomicity:** Κάθε `SaveChangesAsync` call είναι atomic - all-or-nothing
- **Data Consistency:** Αποτρέπει partial updates (αν exception συμβεί πριν το SaveChangesAsync, καμία αλλαγή δεν αποθηκεύεται)
- **Clean Architecture:** Το `ISaveChanges` interface διατηρεί το Application layer ανεξάρτητο από Infrastructure
- **EF-Only:** Το project χρησιμοποιεί μόνο Entity Framework - δεν απαιτείται explicit transaction management για single-context operations

**Πού βρίσκεται:**
- **Interface:** [`backend/BackendProject.Domain/Interfaces/ISaveChanges.cs`](backend/BackendProject.Domain/Interfaces/ISaveChanges.cs)
- **Implementation:** [`backend/BackendProject.Infrastructure/Data/AppDbContext.cs`](backend/BackendProject.Infrastructure/Data/AppDbContext.cs) (implements `ISaveChanges`)
- **Registration:** [`backend/BackendProject.Infrastructure/DependencyInjection.cs`](backend/BackendProject.Infrastructure/DependencyInjection.cs)

**Πώς λειτουργεί:**

**1. Architecture:**
```
Service Layer
    ↓ (injects)
IRepository<T> (Employees, Departments, Projects) + ISaveChanges
    ↓ (implemented by)
Repository<T> + AppDbContext (shares same DbContext instance via DI)
```

**2. Service Pattern:**
- Services inject `IRepository<T>` για κάθε entity type που χρειάζονται
- Services inject `ISaveChanges` για persistence
- Μετά από mutations (Add, Update, SoftDelete), το service καλεί `await _saveChanges.SaveChangesAsync()`
- Το `SaveChangesAsync` είναι atomic - όλες οι αλλαγές στο DbContext commit-άρονται μαζί

**3. Example (Assign Employee to Project):**
```csharp
public async Task AssignToProjectAsync(Guid employeeId, Guid projectId)
{
    var employeeExists = await _employees.ExistsAsync(employeeId);
    if (!employeeExists)
        throw new KeyNotFoundException("Employee not found");
    
    var projectExists = await _projects.ExistsAsync(projectId);
    if (!projectExists)
        throw new KeyNotFoundException("Project not found");
    
    var employee = await _employees.Query()
        .Include(e => e.EmployeeProjects)
        .FirstAsync(e => e.Id == employeeId);
    
    if (employee.EmployeeProjects.Any(ep => ep.ProjectId == projectId))
        return;  // Idempotent
    
    employee.EmployeeProjects.Add(new EmployeeProject
    {
        EmployeeId = employeeId,
        ProjectId = projectId
    });
    
    await _saveChanges.SaveChangesAsync();  // Atomic - όλες οι αλλαγές commit-άρονται μαζί
}
```

**4. Error Handling:**
- Αν exception συμβεί **πριν** το `SaveChangesAsync`: καμία αλλαγή δεν φτάνει στη βάση (αλλαγές μένουν στο change tracker και απορρίπτονται όταν το request τελειώνει)
- Αν exception συμβεί **κατά** το `SaveChangesAsync`: EF κάνει rollback αυτόματα - καμία partial data
- Το DbContext είναι scoped per request - όταν το request τελειώνει, το context dispose-άρεται

**Key Benefits:**
- **Simplified Architecture:** Δεν απαιτείται Unit of Work ή explicit transaction management
- **EF-Native:** Αξιοποιεί το built-in atomic behavior του EF SaveChangesAsync
- **Clean Layering:** Application layer εξαρτάται μόνο από Domain (ISaveChanges interface)
- **Testability:** ISaveChanges μπορεί να mock-αριστεί εύκολα σε tests

---

### 3.7 Repository Pattern

**Feature:** Abstraction layer για data access logic με generic implementation και soft delete support.

**Τι είναι:** Ένα Repository είναι ένα abstraction layer που encapsulate-άρει την πρόσβαση σε δεδομένα για έναν τύπο entity. Αντί να γράφεις database queries απευθείας στο business logic, χρησιμοποιείς repository methods για να αλληλεπιδράσεις με δεδομένα.

**Γιατί χρησιμοποιείται:**
- **Separation of Concerns:** Διαχωρίζει business logic από data access technology
- **Testability:** Μπορείς να mock repositories για testing
- **Consistency:** Παρέχει consistent interface για data operations
- **Flexibility:** Μπορείς να αλλάξεις από EF Core σε Dapper χωρίς να αλλάξεις business logic
- **Reusability:** Generic repository μπορεί να χρησιμοποιηθεί για οποιοδήποτε entity type

**Πού βρίσκεται:**
- **Interface:** [`backend/BackendProject.Domain/Interfaces/IRepository.cs`](backend/BackendProject.Domain/Interfaces/IRepository.cs)
- **Implementation:** [`backend/BackendProject.Infrastructure/Repositories/Repository.cs`](backend/BackendProject.Infrastructure/Repositories/Repository.cs)

**Repository Implementation Details:**

**Generic Repository Class:**
```csharp
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;  // DbContext (shared via DI scoping)
    protected readonly DbSet<T> _dbSet;         // EF Core DbSet για entity type T

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();  // Get DbSet για generic type T
    }
}
```

**Key Methods:**

**1. `AddAsync(T entity)` - Προσθήκη νέου entity:**
```csharp
public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
{
    await _dbSet.AddAsync(entity, cancellationToken);
    return entity;  // Entity προστέθηκε στο change tracker, αλλά δεν έχει save-άρει ακόμα
}
```
- Προσθέτει entity στο EF Core change tracker
- Entity δεν αποθηκεύεται στη βάση αμέσως - το service καλεί `ISaveChanges.SaveChangesAsync()` μετά

**2. `Update(T entity)` - Mark entity ως modified:**
```csharp
public virtual void Update(T entity)
{
    _dbSet.Update(entity);  // Mark entity ως modified στο change tracker
}
```
- Mark-άρει entity ως modified στο EF Core change tracker
- Όταν καλέσεις `SaveChangesAsync()`, EF Core θα κάνει UPDATE query

**3. `SoftDeleteAsync(Guid id)` - Soft delete entity:**
```csharp
public virtual async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
{
    var entity = await _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    if (entity != null)
    {
        entity.IsDeleted = true;      // Set soft delete flag
        entity.DeletedAt = DateTime.UtcNow;  // Set deletion timestamp
        Update(entity);               // Mark ως modified
    }
}
```
- Βρίσκει entity by ID
- Θέτει `IsDeleted = true` και `DeletedAt = DateTime.UtcNow`
- Mark-άρει entity ως modified (θα κάνει UPDATE όταν save-άρεις)

**4. `Query()` - Επιστρέφει IQueryable για complex queries:**
```csharp
public virtual IQueryable<T> Query()
{
    return _dbSet.AsQueryable();  // Returns queryable για building LINQ queries
}
```
- Επιστρέφει `IQueryable<T>` που μπορείς να extend με LINQ
- Μπορείς να χρησιμοποιήσεις `.Include()`, `.Where()`, `.OrderBy()`, etc.
- Queries είναι deferred (executed όταν enumerate-άρεις)

**5. `ExistsAsync(Guid id)` - Έλεγχος αν entity υπάρχει:**
```csharp
public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
{
    return await _dbSet.AnyAsync(e => e.Id == id, cancellationToken);
}
```
- Ελέγχει αν entity με συγκεκριμένο ID υπάρχει
- Respects soft delete (γιατί `_dbSet` έχει global query filter)

**6. `AnyAsync(predicate)` - Έλεγχος αν οποιοδήποτε entity match-άρει condition:**
```csharp
public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
{
    return await _dbSet.AnyAsync(predicate, cancellationToken);
}
```
- Ελέγχει αν οποιοδήποτε entity match-άρει το predicate
- Χρήσιμο για uniqueness validation (π.χ. email uniqueness)

**Connection με Services και ISaveChanges:**

**Shared DbContext via DI:**
- `Repository<T>` δέχεται `AppDbContext` στον constructor (injected via DI)
- Όλα τα repositories και το `ISaveChanges` (AppDbContext) είναι **scoped** - μοιράζονται το ίδιο instance ανά HTTP request
- Όλες οι αλλαγές μένουν στο change tracker μέχρι το service να καλέσει `ISaveChanges.SaveChangesAsync()`

**Example Usage:**
```csharp
// Σε ένα service
public class EmployeeService
{
    private readonly IRepository<Employee> _employees;
    private readonly IRepository<Department> _departments;
    private readonly ISaveChanges _saveChanges;

    public EmployeeService(
        IRepository<Employee> employees,
        IRepository<Department> departments,
        ISaveChanges saveChanges)
    {
        _employees = employees;
        _departments = departments;
        _saveChanges = saveChanges;
    }

    public async Task<EmployeeResponse> GetByIdAsync(Guid id)
    {
        var employee = await _employees.Query()
            .Include(e => e.Department)
            .Include(e => e.EmployeeProjects)
                .ThenInclude(ep => ep.Project)
            .FirstOrDefaultAsync(e => e.Id == id);
        
        if (employee == null)
            throw new KeyNotFoundException("Employee not found");
        
        return EmployeeMapper.ToResponse(employee);
    }

    public async Task<EmployeeResponse> CreateAsync(CreateEmployeeRequest request)
    {
        var employee = EmployeeMapper.ToEntity(request);
        await _employees.AddAsync(employee);
        await _saveChanges.SaveChangesAsync();
        return EmployeeMapper.ToResponse(employee);
    }
}
```

**Benefits:**
- **Generic Implementation:** Ένα repository class για όλα τα entity types
- **Consistent Interface:** Ίδια methods για όλα τα entities
- **Soft Delete Support:** Built-in soft delete functionality
- **Query Flexibility:** `Query()` method επιτρέπει complex LINQ queries
- **Persistence via ISaveChanges:** Services καλούν SaveChangesAsync μετά από mutations για atomic persistence
- **Testability:** Interface-based design επιτρέπει easy mocking

---

### 3.8 Swagger/OpenAPI Documentation

**Feature:** Interactive API documentation με Swagger annotations, XML comments, examples, και comprehensive endpoint documentation.

**Τι είναι:** Το Swagger (Swashbuckle) είναι ένα εργαλείο που αυτόματα δημιουργεί interactive API documentation από τον κώδικα, annotations, και XML comments. Δημιουργεί OpenAPI specification που μπορεί να χρησιμοποιηθεί από clients, testing tools, και documentation generators.

**Γιατί χρησιμοποιείται:**
- **API Documentation:** Παρέχει comprehensive API documentation χωρίς manual writing
- **Interactive Testing:** Επιτρέπει testing API endpoints απευθείας στον browser (Try it out)
- **Schema Documentation:** Δείχνει request/response schemas, data types, και constraints
- **Status Code Documentation:** Document-άρει όλους τους possible status codes και error responses
- **Client Generation:** OpenAPI spec μπορεί να χρησιμοποιηθεί για automatic client code generation
- **API Discovery:** Βοηθά developers να καταλάβουν το API structure και usage

**Πού βρίσκεται:**
- **Configuration:** [`backend/BackendProject.API/Program.cs`](backend/BackendProject.API/Program.cs) (lines 44-59)
- **Annotations:** Όλοι οι controllers χρησιμοποιούν `[SwaggerOperation]` και `[SwaggerResponse]` attributes
- **XML Comments:** XML documentation comments σε controllers και DTOs
- **Project Configuration:** [`backend/BackendProject.API/BackendProject.API.csproj`](backend/BackendProject.API/BackendProject.API.csproj) (line 7: `GenerateDocumentationFile`)

**Swagger Configuration:**

```csharp
builder.Services.AddSwaggerGen(options =>
{
    // API Info
    options.SwaggerDoc("v1", new()
    {
        Title = "Backend Developer Technical Assessment API",
        Version = "v1",
        Description = "API for managing Employees, Departments, and Projects"
    });
    
    // Enable Swagger annotations (SwaggerOperation, SwaggerResponse, etc.)
    options.EnableAnnotations();
    
    // Include XML comments from generated XML file
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});
```

**Swagger Annotations:**

**1. `[SwaggerOperation]` Attribute:**
Χρησιμοποιείται για να document-άρει operation summaries και descriptions:

```csharp
[HttpGet]
[SwaggerOperation(
    Summary = "Get all employees", 
    Description = "Returns a paginated list of all employees"
)]
public async Task<ActionResult<PaginatedResult<EmployeeResponse>>> GetAll(...)
```

**Τι κάνει:**
- `Summary`: Σύντομη περιγραφή που εμφανίζεται στη λίστα endpoints
- `Description`: Λεπτομερής περιγραφή που εμφανίζεται όταν expand-άρεις το endpoint

**2. `[SwaggerResponse]` Attribute:**
Χρησιμοποιείται για να document-άρει response types και status codes:

```csharp
[SwaggerResponse(200, "Returns the list of employees", typeof(PaginatedResult<EmployeeResponse>))]
[SwaggerResponse(400, "Invalid request")]
[SwaggerResponse(404, "Employee not found")]
```

**Τι κάνει:**
- **Status Code:** HTTP status code (200, 201, 400, 404, 409, 500)
- **Description:** Περιγραφή του τι επιστρέφει αυτό το status code
- **Type:** Response type (optional) - δείχνει το schema του response

**3. XML Comments (`/// <summary>`):**
XML documentation comments που εμφανίζονται στο Swagger UI:

```csharp
/// <summary>
/// Get all employees with pagination
/// </summary>
[HttpGet]
public async Task<ActionResult<PaginatedResult<EmployeeResponse>>> GetAll(...)
```

**Πώς λειτουργεί:**
- XML comments γράφονται πάνω από methods, classes, properties
- `GenerateDocumentationFile` στο `.csproj` δημιουργεί XML file κατά το build
- `IncludeXmlComments()` στο Swagger config διαβάζει το XML file
- Swagger UI εμφανίζει τα comments ως documentation

**Example Controller με Annotations:**

```csharp
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EmployeesController : ControllerBase
{
    /// <summary>
    /// Get all employees with pagination
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all employees", 
        Description = "Returns a paginated list of all employees"
    )]
    [SwaggerResponse(200, "Returns the list of employees", typeof(PaginatedResult<EmployeeResponse>))]
    public async Task<ActionResult<PaginatedResult<EmployeeResponse>>> GetAll(...)
    
    /// <summary>
    /// Create a new employee
    /// </summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create employee", 
        Description = "Creates a new employee"
    )]
    [SwaggerResponse(201, "Employee created successfully", typeof(EmployeeResponse))]
    [SwaggerResponse(400, "Invalid request")]
    [SwaggerResponse(409, "Conflict - email already exists")]
    public async Task<ActionResult<EmployeeResponse>> Create(...)
}
```

**Swagger UI Features:**

**1. Interactive API Explorer:**
- Try it out button για κάθε endpoint
- Request body editor με JSON schema validation
- Response viewer με formatted JSON
- Status code display

**2. Schema Documentation:**
- Request/Response schemas με data types
- Required vs optional fields
- Enum values
- Nested objects και arrays

**3. Status Code Documentation:**
- Όλοι οι possible status codes για κάθε endpoint
- Descriptions για κάθε status code
- Example responses

**4. Authentication (αν υπάρχει):**
- API key configuration
- OAuth flows
- Bearer token support

**Access:**
- **Development:** `http://localhost:5000/swagger` (μόνο σε Development environment)
- **Docker:** `http://localhost:5000/swagger` (αν `ASPNETCORE_ENVIRONMENT=Development`)
- **Production:** Swagger UI είναι disabled για security (μόνο Development mode)

**OpenAPI Specification:**
- JSON spec διαθέσιμο στο: `/swagger/v1/swagger.json`
- Μπορεί να χρησιμοποιηθεί για:
  - Client code generation (OpenAPI Generator, NSwag)
  - API testing tools (Postman import)
  - Documentation generators
  - API gateways

**Benefits:**
- **Self-Documenting API:** Documentation δημιουργείται αυτόματα από τον κώδικα
- **Interactive Testing:** Test endpoints χωρίς external tools
- **Schema Validation:** Request validation με JSON schema
- **Type Safety:** Response types document-άρονται με C# types
- **Versioning Support:** Multiple API versions με separate Swagger docs
- **Standards Compliant:** OpenAPI 3.0 specification

---

### 3.9 Logging (Serilog)

**Feature:** Structured logging με file και console output.

**Τι είναι:** Το Logging είναι η καταγραφή events και information κατά τη διάρκεια της εκτέλεσης της εφαρμογής. Το Serilog είναι μια structured logging library που παρέχει flexible logging configuration.

**Γιατί χρησιμοποιείται:**
- **Debugging:** Βοηθά να εντοπίσεις προβλήματα
- **Monitoring:** Παρακολούθηση application health
- **Audit Trail:** Καταγραφή σημαντικών events
- **Performance:** Παρακολούθηση slow operations

**Πού βρίσκεται:**
- **Configuration:** [`backend/BackendProject.API/Program.cs`](backend/BackendProject.API/Program.cs) (lines 11-24)
- **Usage:** `GlobalExceptionMiddleware` και σε όλη την εφαρμογή

**Features:**
- Console output με formatted timestamps
- File logging (JSON format) στο `logs/app-.log`
- Daily rolling log files
- 30-day retention
- Machine name και thread ID enrichment
- Log levels: Warning για Microsoft/System, Information για application

**Configuration:**
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(...)
    .WriteTo.File(...)
    .CreateLogger();
```


### 3.10 Dependency Injection

**Feature:** Constructor injection για όλα τα services, repositories, και dependencies.

**Τι είναι:** Το Dependency Injection είναι ένα design pattern όπου objects λαμβάνουν τις dependencies τους από external source αντί να τις δημιουργούν internally.

**Γιατί χρησιμοποιείται:**
- **Testability:** Κάνει τον κώδικα testable (μπορείς να inject mocks)
- **Reduces Coupling:** Μειώνει coupling μεταξύ classes
- **Manages Lifetimes:** Διαχειρίζεται object lifetimes (singleton, scoped, transient)
- **Centralizes Creation:** Κεντρικοποιεί object creation

**Πού βρίσκεται:**
- **Application DI:** [`backend/BackendProject.Application/DependencyInjection.cs`](backend/BackendProject.Application/DependencyInjection.cs)
- **Infrastructure DI:** [`backend/BackendProject.Infrastructure/DependencyInjection.cs`](backend/BackendProject.Infrastructure/DependencyInjection.cs)
- **API Registration:** [`backend/BackendProject.API/Program.cs`](backend/BackendProject.API/Program.cs)

**Lifetime Types:**
- **Scoped** - Ένα instance ανά HTTP request (Services, Repositories, DbContext, ISaveChanges)
- **Transient** - Νέο instance κάθε φορά (Validators)
- **Singleton** - Ένα instance για ολόκληρη την εφαρμογή (Serilog -> Log.Logger = new LoggerConfiguration().CreateLogger();)

**Registrations:**
- Services (scoped)
- Repositories (scoped, generic `IRepository<>`)
- ISaveChanges (scoped, resolves to AppDbContext)
- Validators (από assembly)
- DbContext (scoped)
- Filters και middleware

**Example:**
```csharp
// Registration (Infrastructure)
services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
services.AddScoped<ISaveChanges>(sp => sp.GetRequiredService<AppDbContext>());

// Usage (injected via constructor)
public EmployeesController(IEmployeeService employeeService)
{
    _employeeService = employeeService; // Injected automatically
}
```

---

### 3.11 Database Migrations

**Feature:** Automatic migration application on startup και manual migration support.

**Τι είναι:** Οι Migrations είναι scripts που εφαρμόζουν αλλαγές στο database schema. Το EF Core δημιουργεί migrations από τις entity configurations.

**Γιατί χρησιμοποιείται:**
- **Version Control:** Database schema changes είναι version controlled
- **Consistency:** Εξασφαλίζει ότι όλοι οι developers έχουν το ίδιο schema
- **Deployment:** Επιτρέπει easy deployment σε production
- **Rollback:** Μπορείς να rollback migrations αν χρειαστεί

**Πού βρίσκεται:**
- **Migrations:** [`backend/BackendProject.Infrastructure/Migrations/`](backend/BackendProject.Infrastructure/Migrations/)
- **Initial Migration:** `20260121133003_InitialCreate.cs`
- **Auto-apply:** [`backend/BackendProject.API/Program.cs`](backend/BackendProject.API/Program.cs) (lines 74-99)

**Features:**
- Automatic migration on application startup (αν υπάρχουν pending)
- Manual migration support μέσω `dotnet ef database update`
- Migration history tracking

**How it works:**
1. Application startup ελέγχει για pending migrations
2. Αν υπάρχουν, εφαρμόζονται αυτόματα
3. Log-άρεται η διαδικασία
4. Αν αποτύχει, application συνεχίζει (για development)

---

### 3.12 Data Seeding

**Feature:** Initial data seeding.

**Τι είναι:** Το Data Seeding είναι η διαδικασία προσθήκης initial data στη βάση όταν δημιουργείται.

**Γιατί χρησιμοποιείται:**
- **Development:** Παρέχει test data για development
- **Testing:** Βοηθά με testing scenarios
- **Demo:** Παρέχει sample data για demonstrations

**Πού βρίσκεται:**
- **Seeder:** [`backend/BackendProject.Infrastructure/Data/DataSeeder.cs`](backend/BackendProject.Infrastructure/Data/DataSeeder.cs)

**Features:**
- Seed initial departments
- Seed employees
- Seed projects
- Seed employee-project assignments
- Special seed data:
  - Employee: **Panagiotis Stavrakellis**
  - Company: **Company**
  - Project: **Backend Developer Technical Assessment**

---

### 3.13 CORS Configuration

**Feature:** Cross-Origin Resource Sharing configuration για frontend integration.

**Τι είναι:** Το CORS είναι ένα security feature που επιτρέπει σε web pages να κάνουν requests σε domains διαφορετικά από αυτό που σέρβιρε τη σελίδα.

**Γιατί χρησιμοποιείται:**
- **Frontend Integration:** Επιτρέπει στο React frontend να κάνει requests στο API
- **Development:** Απαραίτητο για local development με separate frontend/backend

**Πού βρίσκεται:**
- **Configuration:** [`backend/BackendProject.API/Program.cs`](backend/BackendProject.API/Program.cs) (lines 62-70)

**Features:**
- Allow any origin (development)
- Allow any method
- Allow any header
- Configured για frontend integration

**Configuration:**
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

---

### 3.14 Health Check Endpoint

**Feature:** Health check endpoint για Docker και monitoring.

**Τι είναι:** Ένα Health Check endpoint είναι ένα endpoint που επιστρέφει την κατάσταση health της εφαρμογής.

**Γιατί χρησιμοποιείται:**
- **Docker Health Checks:** Docker μπορεί να ελέγξει αν το container είναι healthy
- **Monitoring:** Load balancers και monitoring tools μπορούν να ελέγξουν health
- **Deployment:** Βοηθά με deployment strategies

**Πού βρίσκεται:**
- **Endpoint:** [`backend/BackendProject.API/Program.cs`](backend/BackendProject.API/Program.cs) (line 119)

**Endpoint:**
- `GET /health` - Επιστρέφει health status και timestamp

**Response:**
```json
{
  "status": "healthy",
  "timestamp": "2026-01-27T10:30:00Z"
}
```

**Usage:**
- Docker health checks
- Monitoring και load balancers

---

### 3.15 Docker & Containerization

**Feature:** Complete Docker support με multi-stage builds, Docker Compose για development, και health checks.

**Τι είναι:** Το Docker είναι containerization platform που package-άρει την εφαρμογή και τις dependencies της σε containers. Containers είναι isolated, portable, και consistent across different environments.

**Γιατί χρησιμοποιείται:**
- **Consistency:** Same environment σε development, staging, και production
- **Isolation:** Application και dependencies είναι isolated από host system
- **Portability:** Run anywhere (local, cloud, CI/CD)
- **Easy Deployment:** One command deployment (`docker-compose up`)
- **Dependency Management:** Database, API, και services run together
- **Development Speed:** Quick setup για new developers (no local SQL Server installation needed)

**Πού βρίσκεται:**
- **Dockerfile:** [`Dockerfile`](Dockerfile) (root directory)
- **Docker Compose:** [`docker-compose.yml`](docker-compose.yml) (root directory)

**Dockerfile Structure:**

**Multi-Stage Build:**
Το Dockerfile χρησιμοποιεί multi-stage build για optimized image size:

**Stage 1: Build Stage**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for dependency restoration (layer caching optimization)
COPY backend/BackendProject.Domain/BackendProject.Domain.csproj backend/BackendProject.Domain/
COPY backend/BackendProject.Application/BackendProject.Application.csproj backend/BackendProject.Application/
COPY backend/BackendProject.Infrastructure/BackendProject.Infrastructure.csproj backend/BackendProject.Infrastructure/
COPY backend/BackendProject.API/BackendProject.API.csproj backend/BackendProject.API/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY backend/BackendProject.Domain/ ./backend/BackendProject.Domain/
COPY backend/BackendProject.Application/ ./backend/BackendProject.Application/
COPY backend/BackendProject.Infrastructure/ ./backend/BackendProject.Infrastructure/
COPY backend/BackendProject.API/ ./backend/BackendProject.API/

# Build and publish
RUN dotnet publish -c Release -o /app/publish --no-restore
```

**Τι κάνει:**
- Χρησιμοποιεί `.NET SDK` image για build
- Copy project files πρώτα (layer caching - dependencies restore only αν project files change)
- Restore dependencies
- Copy source code
- Build και publish application

**Stage 2: Runtime Stage**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "BackendProject.API.dll"]
```

**Τι κάνει:**
- Χρησιμοποιεί `.NET Runtime` image (μικρότερο από SDK - no build tools)
- Copy published output από build stage
- Expose port 8080
- Set environment variables
- Configure health check (Docker checks `/health` endpoint)
- Run application

**Benefits of Multi-Stage Build:**
- **Smaller Image:** Runtime image είναι μικρότερο (no SDK, no source code)
- **Security:** Production image δεν περιέχει build tools ή source code
- **Performance:** Faster image pulls (smaller size)

**Docker Compose Configuration:**

**Services:**

**1. API Service:**
```yaml
api:
  build:
    context: .
    dockerfile: Dockerfile
  container_name: backend-api
  ports:
    - "5000:8080"  # Host:Container port mapping
  environment:
    - ASPNETCORE_ENVIRONMENT=Development
    - ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=BackendProjectDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True
  depends_on:
    sqlserver:
      condition: service_healthy  # Wait for SQL Server to be healthy
  networks:
    - backend-network
  restart: unless-stopped
```

**Τι κάνει:**
- Build image από Dockerfile
- Map port 5000 (host) → 8080 (container)
- Set environment variables (connection string uses `sqlserver` service name)
- Wait for SQL Server to be healthy before starting
- Connect to `backend-network`
- Auto-restart αν container stops

**2. SQL Server Service:**
```yaml
sqlserver:
  image: mcr.microsoft.com/mssql/server:2022-latest
  container_name: backend-sqlserver
  ports:
    - "1433:1433"  # Expose SQL Server port
  environment:
    - ACCEPT_EULA=Y
    - MSSQL_SA_PASSWORD=YourStrong!Passw0rd
    - MSSQL_PID=Developer
  volumes:
    - sqlserver-data:/var/opt/mssql  # Persistent data storage
  networks:
    - backend-network
  healthcheck:
    test: ["CMD-SHELL", "timeout 3 bash -c 'cat < /dev/null > /dev/tcp/localhost/${MSSQL_TCP_PORT:-1433}' || exit 1"]
    interval: 10s
    timeout: 5s
    retries: 10
    start_period: 30s
  restart: unless-stopped
```

**Τι κάνει:**
- Use official SQL Server 2022 image
- Expose port 1433
- Set SQL Server configuration (password, edition)
- Persistent volume για database data (survives container restarts)
- Health check: verify SQL Server is accepting connections
- Connect to `backend-network`

**Networks:**
```yaml
networks:
  backend-network:
    driver: bridge
```

**Τι κάνει:**
- Creates isolated network για API και SQL Server
- Services can communicate using service names (`sqlserver`, `api`)

**Volumes:**
```yaml
volumes:
  sqlserver-data:
```

**Τι κάνει:**
- Named volume για SQL Server data persistence
- Data survives container deletion/restart

**Running with Docker:**

**Start Services:**
```bash
docker-compose up -d
```

**Τι κάνει:**
- Build API image (αν needed)
- Pull SQL Server image (αν needed)
- Start both containers
- Run in detached mode (`-d`)

**Stop Services:**
```bash
docker-compose down
```

**View Logs:**
```bash
docker-compose logs -f api
docker-compose logs -f sqlserver
```

**Access:**
- **API:** `http://localhost:5000`
- **Swagger:** `http://localhost:5000/swagger`
- **Health Check:** `http://localhost:5000/health`
- **SQL Server:** `localhost:1433`

**Docker Health Checks:**

**API Health Check:**
- Defined στο Dockerfile: `HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3`
- Checks `/health` endpoint every 30 seconds
- Container marked as unhealthy αν health check fails 3 times

**SQL Server Health Check:**
- Defined στο docker-compose.yml
- Checks if SQL Server accepts connections
- API waits for SQL Server to be healthy before starting

**Benefits:**
- **Quick Setup:** One command για complete environment (`docker-compose up`)
- **Consistent Environment:** Same setup για όλους τους developers
- **No Local Dependencies:** No need για local SQL Server installation
- **Isolated:** Containers are isolated από host system
- **Easy Cleanup:** `docker-compose down` removes everything
- **Production Ready:** Same Dockerfile can be used για production
- **Health Monitoring:** Docker monitors container health automatically

**Production Considerations:**
- Use environment-specific connection strings
- Use secrets management (Docker secrets, environment variables)
- Configure proper logging
- Set up monitoring και alerting
- Use production-grade SQL Server configuration
- Configure backup strategies για database volumes

---

## 4. API Endpoints Summary

### Employees Endpoints

| Method | Endpoint | Description | Response Codes |
|--------|----------|-------------|----------------|
| GET | `/api/employees` | Get all employees (paginated) | 200 |
| GET | `/api/employees/{id}` | Get employee by ID | 200, 404 |
| GET | `/api/employees/search?q={term}` | Search employees | 200, 400 |
| POST | `/api/employees` | Create employee | 201, 400, 409 |
| PUT | `/api/employees/{id}` | Update employee | 200, 400, 404 |
| DELETE | `/api/employees/{id}` | Soft delete employee | 204, 404 |
| POST | `/api/employees/{id}/projects/{projectId}` | Assign to project | 200, 404 |
| DELETE | `/api/employees/{id}/projects/{projectId}` | Remove from project | 200, 404 |

### Departments Endpoints

| Method | Endpoint | Description | Response Codes |
|--------|----------|-------------|----------------|
| GET | `/api/departments` | Get all departments (paginated) | 200 |
| GET | `/api/departments/{id}` | Get department by ID | 200, 404 |
| GET | `/api/departments/search?q={term}` | Search departments | 200, 400 |
| GET | `/api/departments/{id}/employees` | Get employees in department | 200, 404 |
| POST | `/api/departments` | Create department | 201, 400, 409 |
| PUT | `/api/departments/{id}` | Update department | 200, 400, 404 |
| DELETE | `/api/departments/{id}` | Soft delete department | 204, 404 |

### Projects Endpoints

| Method | Endpoint | Description | Response Codes |
|--------|----------|-------------|----------------|
| GET | `/api/projects` | Get all projects (paginated) | 200 |
| GET | `/api/projects/{id}` | Get project by ID | 200, 404 |
| GET | `/api/projects/search?q={term}` | Search projects | 200, 400 |
| POST | `/api/projects` | Create project | 201, 400, 409 |
| PUT | `/api/projects/{id}` | Update project | 200, 400, 404 |
| DELETE | `/api/projects/{id}` | Soft delete project | 204, 404 |

### System Endpoints

| Method | Endpoint | Description | Response Codes |
|--------|----------|-------------|----------------|
| GET | `/health` | Health check | 200 |
| GET | `/swagger` | Swagger UI (dev only) | 200 |

---

## 5. Testing

**Feature:** Comprehensive unit testing για business logic, validation rules, και service operations με xUnit, Moq, και in-memory database.

**Τι είναι:** Unit testing είναι η πρακτική γραφής tests που verify-άρουν ότι individual units (methods, classes) λειτουργούν σωστά in isolation. Tests τρέχουν γρήγορα, είναι repeatable, και βοηθούν να catch-άρεις bugs πριν το deployment.

**Γιατί χρησιμοποιείται:**
- **Quality Assurance:** Εξασφαλίζει ότι ο κώδικας λειτουργεί σωστά
- **Regression Prevention:** Catch-άρει bugs όταν γίνονται changes
- **Documentation:** Tests serve ως living documentation για το πώς πρέπει να χρησιμοποιείται ο κώδικας
- **Refactoring Safety:** Μπορείς να refactor με confidence ότι τα tests θα catch-άρουν breaking changes
- **Design Feedback:** Writing tests βοηθά να identify design issues (tight coupling, hard to test code)

**Πού βρίσκεται:**
- **Test Project:** [`tests/BackendProject.Tests/`](tests/BackendProject.Tests/)
- **Project File:** [`tests/BackendProject.Tests/BackendProject.Tests.csproj`](tests/BackendProject.Tests/BackendProject.Tests.csproj)

**Test Framework & Tools:**
- **xUnit:** Testing framework (alternative to MSTest, NUnit)
- **Moq:** Mocking library για creating test doubles
- **FluentAssertions:** Fluent syntax για assertions (readable test code)
- **Entity Framework In-Memory Database:** In-memory database για testing data access (no real database needed)

**Test Structure:**

**1. Service Tests:**
Test business logic σε services με in-memory database:

- [`Services/EmployeeServiceTests.cs`](tests/BackendProject.Tests/Services/EmployeeServiceTests.cs)
- [`Services/DepartmentServiceTests.cs`](tests/BackendProject.Tests/Services/DepartmentServiceTests.cs)
- [`Services/ProjectServiceTests.cs`](tests/BackendProject.Tests/Services/ProjectServiceTests.cs)

**Service Test Pattern:**
```csharp
public class EmployeeServiceTests : ServiceTestBase
{
    private readonly EmployeeService _service;

    public EmployeeServiceTests()
    {
        _service = new EmployeeService(Employees, Departments, Projects, SaveChanges);
        SeedTestData();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedEmployees()
    {
        // Arrange
        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };
        
        // Act
        var result = await _service.GetAllAsync(pagination, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().BeGreaterThan(0);
    }
}
```

**2. Validator Tests:**
Test validation rules in isolation:

- [`Validators/EmployeeValidatorsTests.cs`](tests/BackendProject.Tests/Validators/EmployeeValidatorsTests.cs)
- [`Validators/EmployeeValidatorTests.cs`](tests/BackendProject.Tests/Validators/EmployeeValidatorTests.cs)
- [`Validators/DepartmentValidatorsTests.cs`](tests/BackendProject.Tests/Validators/DepartmentValidatorsTests.cs)
- [`Validators/DepartmentValidatorTests.cs`](tests/BackendProject.Tests/Validators/DepartmentValidatorTests.cs)
- [`Validators/ProjectValidatorsTests.cs`](tests/BackendProject.Tests/Validators/ProjectValidatorsTests.cs)
- [`Validators/ProjectValidatorTests.cs`](tests/BackendProject.Tests/Validators/ProjectValidatorTests.cs)

**Validator Test Pattern:**
```csharp
public class EmployeeValidatorTests : ValidatorTestBase
{
    private readonly CreateEmployeeValidator _validator;

    public EmployeeValidatorTests()
    {
        _validator = new CreateEmployeeValidator(Employees, Departments);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ValidateAsync_InvalidEmail_ReturnsError(string email)
    {
        // Arrange
        var request = new CreateEmployeeRequest { Email = email };
        
        // Act
        var result = await _validator.ValidateAsync(request);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }
}
```

**Test Infrastructure:**

**1. ServiceTestBase:**
Base class για service tests με in-memory database setup:

- [`TestFixture/ServiceTestBase.cs`](tests/BackendProject.Tests/TestFixture/ServiceTestBase.cs)

**Τι παρέχει:**
- In-memory `AppDbContext` (isolated database per test)
- `IRepository<Employee>`, `IRepository<Department>`, `IRepository<Project>` instances
- `ISaveChanges` (resolves to Context) για persistence
- Automatic cleanup (dispose) μετά από κάθε test
- Unique database name per test (no test interference)

**Implementation:**
```csharp
public abstract class ServiceTestBase : IDisposable
{
    protected readonly AppDbContext Context;
    protected readonly IRepository<Employee> Employees;
    protected readonly IRepository<Department> Departments;
    protected readonly IRepository<Project> Projects;
    protected readonly ISaveChanges SaveChanges;

    protected ServiceTestBase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new AppDbContext(options);
        Employees = new Repository<Employee>(Context);
        Departments = new Repository<Department>(Context);
        Projects = new Repository<Project>(Context);
        SaveChanges = Context;
    }
}
```

**2. ValidatorTestBase:**
Base class για validator tests:

- [`TestFixture/ValidatorTestBase.cs`](tests/BackendProject.Tests/TestFixture/ValidatorTestBase.cs)

**Τι παρέχει:**
- In-memory database για async validation checks (uniqueness, foreign keys)
- `IRepository<Employee>`, `IRepository<Department>`, `IRepository<Project>` για validators που χρειάζονται database access
- Test data seeding methods

**Test Coverage:**

**Service Tests Cover:**
- ✅ **CRUD Operations:** Create, Read, Update, Delete
- ✅ **Pagination:** Page number, page size, total count, has next/previous
- ✅ **Search Functionality:** Search by name, email, description
- ✅ **Employee-Project Assignments:** Assign, remove, idempotent operations
- ✅ **Error Handling:** Not found scenarios, validation errors
- ✅ **Business Logic:** Status updates, date validations, relationships

**Validator Tests Cover:**
- ✅ **Required Fields:** Empty/null validation
- ✅ **String Length:** Max length validation
- ✅ **Email Format:** Valid email address validation
- ✅ **Uniqueness:** Email, name uniqueness (async database checks)
- ✅ **Foreign Keys:** Department, project existence validation
- ✅ **Date Validation:** Hire date not in future, end date after start date
- ✅ **Enum Validation:** Status enum values

**Running Tests:**

**Command Line:**
```bash
dotnet test
```

**Visual Studio:**
- Test Explorer window
- Run All Tests
- Debug tests

**Test Execution:**
- Tests run in parallel (xUnit default)
- Each test gets isolated in-memory database
- Tests are fast (no real database, no network calls)
- Tests are repeatable (deterministic)

**Best Practices Used:**
- **Arrange-Act-Assert Pattern:** Clear test structure
- **Test Isolation:** Each test is independent (no shared state)
- **Descriptive Test Names:** Test names describe what is being tested
- **Theory Tests:** Parameterized tests για multiple scenarios
- **FluentAssertions:** Readable assertions (`result.Should().NotBeNull()`)
- **In-Memory Database:** Fast, isolated, no external dependencies
- **Test Data Seeding:** Helper methods για creating test data

**Benefits:**
- **Fast Feedback:** Tests run quickly (in-memory database)
- **Isolated:** Each test is independent (no side effects)
- **Comprehensive:** Covers business logic και validation rules
- **Maintainable:** Clear test structure, reusable base classes
- **CI/CD Ready:** Tests can run in automated pipelines
- **Documentation:** Tests serve ως examples για API usage

---