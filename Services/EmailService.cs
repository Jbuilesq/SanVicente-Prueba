// SanVicente.Services/EmailService.cs

using SanVicente.Models;
using SanVicente.Infraestructure;
using System;
using System.Threading.Tasks;

namespace SanVicente.Services
{
    public class EmailService : IEmailService
    {
        private readonly DbAppContext _context;
        
        // Inject DbContext to save the email log/history
        public EmailService(DbAppContext context)
        {
            _context = context;
        }

        public async Task<Email> SendAppointmentConfirmationAsync(Appointment appointment)
        {
            // IMPORTANT: In a real application, the Appointment object should be 
            // loaded with Patient/Doctor data before calling this service 
            // (e.g., using .Include() in the Controller).

            string patientName = appointment.Patient?.Name ?? "Patient";
            string doctorName = appointment.Doctor?.Name ?? "Doctor";
            
            // 1. Prepare Email Content
            string subject = $"Appointment Confirmation for {patientName}";
            string content = $@"
                Dear {patientName},
                Your appointment has been successfully scheduled.
                
                Details:
                - Date & Time: {appointment.Date:dd/MM/yyyy HH:mm}
                - Doctor: {doctorName} ({appointment.Doctor?.Specialty})
                - Status: {appointment.AppointmentStatus}
                
                Thank you.
            ";

            // 2. Mock Email Sending Logic
            string status;
            try
            {
                // *** PLACEHOLDER FOR ACTUAL EMAIL SENDING LOGIC (SMTP) ***
                // await SendEmailViaSmtpAsync(appointment.Patient.Email, subject, content); 
                await Task.Delay(50); // Simulate network delay
                status = "Sent"; // Assume success for the mock
            }
            catch (Exception ex)
            {
                // Log failure details
                status = $"Failed: {ex.Message}";
            }

            // 3. Create Email History Log entry
            var emailLog = new Email
            {
                SendDate = DateTime.Now,
                Subject = subject,
                Content = content,
                EmailStatus = status,
                AppointmentId = appointment.Id
            };

            // 4. Save Email Log to Database
            _context.Emails.Add(emailLog);
            await _context.SaveChangesAsync();
            
            return emailLog;
        }
    }
}