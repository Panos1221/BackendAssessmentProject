# Backend Developer Technical Assessment

A .NET 8 Web API for managing Employees, Departments, and Projects.

## Tech Stack

- **.NET 8** - ASP.NET Core Web API
- **Entity Framework Core** - ORM with SQL Server
- **xUnit** - Unit testing framework
- **Docker** - Containerization support

## Features

- CRUD operations for Employees, Departments, and Projects
- Pagination support
- Soft delete functionality
- FluentValidation
- Global exception handling middleware
- Swagger documentation
- Unit tests

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
   cd backend/BackendProject.API
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
cd backend/BackendProject.Infrastructure

dotnet ef database update --startup-project ../BackendProject.API
```

### Using Docker
```bash
# Migration command executes inside API container
docker exec -it backend-api dotnet ef database update
```

### Useful Query:

Μας επιστρέφει τα ονόματα των employees και τα projects στα οποία έχουν ανατεθεί.

```sql
SELECT
    e.FirstName,
    e.LastName,
    p.Name AS ProjectName,
    p.Description AS ProjectDescription,
    p.StartDate,
    p.EndDate
FROM Employees e
INNER JOIN EmployeeProjects ep
    ON e.Id = ep.EmployeeId
INNER JOIN Projects p
    ON ep.ProjectId = p.Id
WHERE e.IsDeleted = 0
  AND p.IsDeleted = 0;
```

---

## Project Structure

- `BackendProject.Domain` - Domain entities and interfaces
- `BackendProject.Application` - Business logic and DTOs
- `BackendProject.Infrastructure` - Data access and EF Core
- `BackendProject.API` - Web API controllers and middleware
- `BackendProject.Tests` - Unit tests