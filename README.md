# Backend Developer Technical Assessment

A .NET 8 Web API for managing Employees, Departments, and Projects with clean architecture principles.

## Tech Stack

- **.NET 8** - ASP.NET Core Web API
- **Entity Framework Core** - ORM with SQL Server
- **xUnit** - Unit testing framework
- **Docker** - Containerization support

## Features

- CRUD operations for Employees, Departments, and Projects ([Controllers](backend/BackendProject.API/Controllers/))
- Pagination support ([PaginationParams](backend/BackendProject.Application/Common/PaginationParams.cs))
- Soft delete functionality ([BaseEntity](backend/BackendProject.Domain/Common/BaseEntity.cs))
- FluentValidation ([Validators](backend/BackendProject.Application/Validators/))
- Global exception handling middleware ([GlobalExceptionMiddleware](backend/BackendProject.API/Middleware/GlobalExceptionMiddleware.cs))
- Swagger documentation ([Program.cs](backend/BackendProject.API/Program.cs))
- Unit tests for business logic ([Tests](tests/BackendProject.Tests/))

**Frontend**: See [frontend/README.md](frontend/README.md) for the optional React frontend application (Supports CRUD operations).

## Getting Started

### Using Docker Compose

```bash
docker-compose up -d
```

The API will be available at `http://localhost:5000` and Swagger UI at `http://localhost:5000/swagger`.

### Database

The connection string configuration works as follows:

- **When running with Docker Compose**: The environment variable in `docker-compose.yml` overrides `appsettings.json`, using `Server=sqlserver,1433` (the Docker service name).

- **When running without Docker**: The application reads from `appsettings.json` (or `appsettings.Development.json` in dev mode), using `Server=localhost,1433`.

###
The API will be available at `http://localhost:5000` (or the port configured in `launchSettings.json`) and Swagger UI at `http://localhost:5000/swagger`.

### Running without docker: 

   ```bash
   cd src/BackendProject.API
   dotnet run
   ```

### Running without docker: 

   ```bash
   cd src/BackendProject.API
   dotnet run
   ```   

## API Endpoints

- **Employees**: `/api/employees` - Manage employee records
- **Departments**: `/api/departments` - Manage department records
- **Projects**: `/api/projects` - Manage project records

All endpoints support GET (with pagination), POST, PUT, and DELETE operations.

## Testing

Run unit tests with:

```bash
dotnet test
```

---

## Running Migrations

### Using .NET CLI
```bash
cd src/BackendProject.Infrastructure

dotnet ef database update --startup-project ../BackendProject.API
```

### Using Docker
```bash
# Migration command executes inside API container
docker exec -it backend-api dotnet ef database update
```

---

## Project Structure

- `BackendProject.Domain` - Domain entities and interfaces
- `BackendProject.Application` - Business logic and DTOs
- `BackendProject.Infrastructure` - Data access and EF Core
- `BackendProject.API` - Web API controllers and middleware
- `BackendProject.Tests` - Unit tests
