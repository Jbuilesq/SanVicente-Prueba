// SanVicente.Models/Appointment.cs

using System.ComponentModel.DataAnnotations.Schema; // Added for clarity

namespace SanVicente.Models;

public class Appointment
{
    public int Id { get; set; }
    public DateTime Date { get; set; } 
    public string AppointmentStatus { get; set; }
    
    // Foreign Keys
    public int PatientId { get; set; }
    public int DoctorId { get; set; }

    // Navigation Properties (required for join/Include)
    // [ForeignKey("PatientId")]
    public virtual Patient? Patient { get; set; } 
    
    // [ForeignKey("DoctorId")]
    public virtual Doctor? Doctor { get; set; }
}