namespace SanVicente.Models;

public class Doctor : Person
{
   
    public string Specialty { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}