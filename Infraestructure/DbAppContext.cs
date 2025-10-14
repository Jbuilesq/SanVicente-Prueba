using Microsoft.EntityFrameworkCore;
using SanVicente.Models;

namespace SanVicente.Infraestructure;

public class DbAppContext : DbContext
{
    public DbAppContext(DbContextOptions<DbAppContext> options) : base(options)
    {
        
    }
    
    public DbSet<Patient>  Patients { get; set; }
    public DbSet<Appointment>  Appointments { get; set; }
    public DbSet<Doctor>  Doctors { get; set; }
    public DbSet<Email>  Emails { get; set; }
}