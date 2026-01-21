using BackendProject.Domain.Entities;
using BackendProject.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BackendProject.Infrastructure.Data;

/// <summary>
/// Seeds initial data into the database with Greek names as specified.
/// </summary>
public static class DataSeeder
{
    // Fixed GUIDs for seeding to ensure consistency
    private static readonly Guid BackendDepartment = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid EngineeringDeptId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid SalesDeptId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private static readonly Guid BackendAssessmentProjectId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid MobileAppProjectId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid DataMigrationProjectId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    private static readonly Guid PanagiotisId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    private static readonly Guid NikolaosId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
    private static readonly Guid GeorgiosId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
    private static readonly Guid MariaId = Guid.Parse("00000000-0000-0000-0001-000000000001");
    private static readonly Guid EleniId = Guid.Parse("00000000-0000-0000-0002-000000000002");
    private static readonly Guid DimitrisId = Guid.Parse("00000000-0000-0000-0003-000000000003");
    private static readonly Guid KonstantinaId = Guid.Parse("00000000-0000-0000-0004-000000000004");
    private static readonly Guid AthanasiosId = Guid.Parse("00000000-0000-0000-0005-000000000005");

    public static void Seed(ModelBuilder modelBuilder)
    {
        SeedDepartments(modelBuilder);
        SeedProjects(modelBuilder);
        SeedEmployees(modelBuilder);
        SeedEmployeeProjects(modelBuilder);
    }

    private static void SeedDepartments(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>().HasData(
            new Department
            {
                Id = BackendDepartment,
                Name = "Backend Developing",
                Description = "Backend Developing department"
            },
            new Department
            {
                Id = EngineeringDeptId,
                Name = "Engineering",
                Description = "Core engineering and infrastructure team"
            },
            new Department
            {
                Id = SalesDeptId,
                Name = "Sales",
                Description = "Sales and customer relations department"
            }
        );
    }

    private static void SeedProjects(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>().HasData(
            new Project
            {
                Id = BackendAssessmentProjectId,
                Name = "Backend Developer Technical Assessment",
                Description = "Technical assessment project for evaluating backend development skills",
                StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2024, 12, 31, 0, 0, 0, DateTimeKind.Utc)
            },
            new Project
            {
                Id = MobileAppProjectId,
                Name = "Mobile Application Platform",
                Description = "Cross-platform mobile application development project",
                StartDate = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = null
            },
            new Project
            {
                Id = DataMigrationProjectId,
                Name = "Data Migration Initiative",
                Description = "Legacy system data migration to new cloud infrastructure",
                StartDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2024, 9, 30, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }

    private static void SeedEmployees(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>().HasData(
            new Employee
            {
                Id = PanagiotisId,
                FirstName = "Panagiotis",
                LastName = "Stavrakellis",
                Email = "panagiotis.stavrakellis@company.com",
                Status = EmployeeStatus.Active,
                HireDate = new DateTime(2020, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                Notes = "Backend Developer",
                DepartmentId = BackendDepartment
            },
            // Additional Greek names
            new Employee
            {
                Id = NikolaosId,
                FirstName = "Nikolaos",
                LastName = "Papadopoulos",
                Email = "nikolaos.papadopoulos@company.com",
                Status = EmployeeStatus.Active,
                HireDate = new DateTime(2021, 3, 10, 0, 0, 0, DateTimeKind.Utc),
                Notes = "Frontend Developer",
                DepartmentId = BackendDepartment
            },
            new Employee
            {
                Id = GeorgiosId,
                FirstName = "Georgios",
                LastName = "Konstantinidis",
                Email = "georgios.konstantinidis@company.com",
                Status = EmployeeStatus.Active,
                HireDate = new DateTime(2019, 7, 22, 0, 0, 0, DateTimeKind.Utc),
                Notes = "DevOps Engineer",
                DepartmentId = EngineeringDeptId
            },
            new Employee
            {
                Id = MariaId,
                FirstName = "Maria",
                LastName = "Georgiou",
                Email = "maria.georgiou@company.com",
                Status = EmployeeStatus.Active,
                HireDate = new DateTime(2022, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                Notes = "QA Engineer",
                DepartmentId = EngineeringDeptId
            },
            new Employee
            {
                Id = EleniId,
                FirstName = "Eleni",
                LastName = "Dimitriou",
                Email = "eleni.dimitriou@company.com",
                Status = EmployeeStatus.Active,
                HireDate = new DateTime(2020, 11, 5, 0, 0, 0, DateTimeKind.Utc),
                Notes = "Frontend Developer",
                DepartmentId = BackendDepartment
            },
            new Employee
            {
                Id = DimitrisId,
                FirstName = "Dimitrios",
                LastName = "Antonopoulos",
                Email = "dimitrios.antonopoulos@company.com",
                Status = EmployeeStatus.Inactive,
                HireDate = new DateTime(2018, 5, 15, 0, 0, 0, DateTimeKind.Utc),
                Notes = "Former Project Manager",
                DepartmentId = SalesDeptId
            },
            new Employee
            {
                Id = KonstantinaId,
                FirstName = "Konstantina",
                LastName = "Vasileiou",
                Email = "konstantina.vasileiou@company.com",
                Status = EmployeeStatus.Active,
                HireDate = new DateTime(2023, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                Notes = "Sales Representative",
                DepartmentId = SalesDeptId
            },
            new Employee
            {
                Id = AthanasiosId,
                FirstName = "Athanasios",
                LastName = "Nikolaidis",
                Email = "athanasios.nikolaidis@company.com",
                Status = EmployeeStatus.Active,
                HireDate = new DateTime(2021, 8, 20, 0, 0, 0, DateTimeKind.Utc),
                Notes = "Database Administrator",
                DepartmentId = EngineeringDeptId
            }
        );
    }

    private static void SeedEmployeeProjects(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmployeeProject>().HasData(
            // Backend 
            new EmployeeProject { EmployeeId = PanagiotisId, ProjectId = BackendAssessmentProjectId },
            new EmployeeProject { EmployeeId = NikolaosId, ProjectId = BackendAssessmentProjectId },
            new EmployeeProject { EmployeeId = MariaId, ProjectId = BackendAssessmentProjectId },
            
            // Mobile 
            new EmployeeProject { EmployeeId = EleniId, ProjectId = MobileAppProjectId },
            new EmployeeProject { EmployeeId = NikolaosId, ProjectId = MobileAppProjectId },
            new EmployeeProject { EmployeeId = GeorgiosId, ProjectId = MobileAppProjectId },
            
            // Data Migration
            new EmployeeProject { EmployeeId = AthanasiosId, ProjectId = DataMigrationProjectId },
            new EmployeeProject { EmployeeId = GeorgiosId, ProjectId = DataMigrationProjectId },
            new EmployeeProject { EmployeeId = PanagiotisId, ProjectId = DataMigrationProjectId }
        );
    }
}
