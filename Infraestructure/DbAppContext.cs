using Microsoft.EntityFrameworkCore;
using SanVicente.Models;

namespace SanVicente.Infraestructure;

public class DbAppContext : DbContext
{
    public DbAppContext(DbContextOptions<DbAppContext> options) 
        : base(options)
    {
    }

    // For unique fields (DNI)
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>()
            .HasIndex(p => p.DNI)
            .IsUnique();
        
        modelBuilder.Entity<Doctor>()
            .HasIndex(m => m.DNI)
            .IsUnique();
        
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Patient>  Patients { get; set; }
    public DbSet<Appointment>  Appointments { get; set; }
    public DbSet<Doctor>  Doctors { get; set; }
    public DbSet<Email>  Emails { get; set; }
}