namespace SanVicente.Models;

public class Email
{
    public int Id { get; set; }
    public DateTime SendDate { get; set; }
    public string Subject { get; set; }
    public string Content { get; set; }
    public string EmailStatus { get; set; }
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; }
}