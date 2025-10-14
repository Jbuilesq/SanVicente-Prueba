namespace SanVicente.Models;

public class Patient : Person
{
    
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}