
using SanVicente.Models;
using System.Threading.Tasks;

namespace SanVicente.Services
{
    public interface IEmailService
    {
        // Method to send a confirmation email for an appointment
        Task<Email> SendAppointmentConfirmationAsync(Appointment appointment);
    }
}