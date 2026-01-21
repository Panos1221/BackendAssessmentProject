using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BackendProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeProjects",
                columns: table => new
                {
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeProjects", x => new { x.EmployeeId, x.ProjectId });
                    table.ForeignKey(
                        name: "FK_EmployeeProjects_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeProjects_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "DeletedAt", "Description", "IsDeleted", "Name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), null, "Backend Developing department", false, "Backend Developing" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), null, "Core engineering and infrastructure team", false, "Engineering" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), null, "Sales and customer relations department", false, "Sales" }
                });

            migrationBuilder.InsertData(
                table: "Projects",
                columns: new[] { "Id", "DeletedAt", "Description", "EndDate", "IsDeleted", "Name", "StartDate" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), null, "Technical assessment project for evaluating backend development skills", new DateTime(2024, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), false, "Backend Developer Technical Assessment", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), null, "Cross-platform mobile application development project", null, false, "Mobile Application Platform", new DateTime(2024, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), null, "Legacy system data migration to new cloud infrastructure", new DateTime(2024, 9, 30, 0, 0, 0, 0, DateTimeKind.Utc), false, "Data Migration Initiative", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "DeletedAt", "DepartmentId", "Email", "FirstName", "HireDate", "IsDeleted", "LastName", "Notes", "Status" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0001-000000000001"), null, new Guid("22222222-2222-2222-2222-222222222222"), "maria.georgiou@company.com", "Maria", new DateTime(2022, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Georgiou", "QA Engineer", 0 },
                    { new Guid("00000000-0000-0000-0002-000000000002"), null, new Guid("11111111-1111-1111-1111-111111111111"), "eleni.dimitriou@company.com", "Eleni", new DateTime(2020, 11, 5, 0, 0, 0, 0, DateTimeKind.Utc), false, "Dimitriou", "Frontend Developer", 0 },
                    { new Guid("00000000-0000-0000-0003-000000000003"), null, new Guid("33333333-3333-3333-3333-333333333333"), "dimitrios.antonopoulos@company.com", "Dimitrios", new DateTime(2018, 5, 15, 0, 0, 0, 0, DateTimeKind.Utc), false, "Antonopoulos", "Former Project Manager", 1 },
                    { new Guid("00000000-0000-0000-0004-000000000004"), null, new Guid("33333333-3333-3333-3333-333333333333"), "konstantina.vasileiou@company.com", "Konstantina", new DateTime(2023, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), false, "Vasileiou", "Sales Representative", 0 },
                    { new Guid("00000000-0000-0000-0005-000000000005"), null, new Guid("22222222-2222-2222-2222-222222222222"), "athanasios.nikolaidis@company.com", "Athanasios", new DateTime(2021, 8, 20, 0, 0, 0, 0, DateTimeKind.Utc), false, "Nikolaidis", "Database Administrator", 0 },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), null, new Guid("11111111-1111-1111-1111-111111111111"), "panagiotis.stavrakellis@company.com", "Panagiotis", new DateTime(2020, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), false, "Stavrakellis", "Backend Developer", 0 },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), null, new Guid("11111111-1111-1111-1111-111111111111"), "nikolaos.papadopoulos@company.com", "Nikolaos", new DateTime(2021, 3, 10, 0, 0, 0, 0, DateTimeKind.Utc), false, "Papadopoulos", "Frontend Developer", 0 },
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), null, new Guid("22222222-2222-2222-2222-222222222222"), "georgios.konstantinidis@company.com", "Georgios", new DateTime(2019, 7, 22, 0, 0, 0, 0, DateTimeKind.Utc), false, "Konstantinidis", "DevOps Engineer", 0 }
                });

            migrationBuilder.InsertData(
                table: "EmployeeProjects",
                columns: new[] { "EmployeeId", "ProjectId" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0001-000000000001"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") },
                    { new Guid("00000000-0000-0000-0002-000000000002"), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb") },
                    { new Guid("00000000-0000-0000-0005-000000000005"), new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc") },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc") },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb") },
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb") },
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Name",
                table: "Departments",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeProjects_ProjectId",
                table: "EmployeeProjects",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Name",
                table: "Projects",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeProjects");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Departments");
        }
    }
}
