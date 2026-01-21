using BackendProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackendProject.Infrastructure.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.Property(e => e.Notes)
            .HasMaxLength(1000);
        
        builder.Property(e => e.Status)
            .IsRequired();
        
        builder.Property(e => e.HireDate)
            .IsRequired();

        // Index on DepartmentId for faster joins
        builder.HasIndex(e => e.DepartmentId);

        // Relationship: Employee belongs to one Department
        builder.HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
