using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SanVicente.Infraestructure;
using SanVicente.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SanVicente.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly DbAppContext _context;

        // Constructor for DI
        public AppointmentController(DbAppContext context)
        {
            _context = context;
        }

        // GET: /Appointment/Index
        public async Task<IActionResult> Index(int? patientId, int? doctorId)
        {
            // Base query with eager loading for navigation properties
            var query = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .AsQueryable();

            // Filter logic: List appointments by patient or doctor (if IDs are provided)
            if (patientId.HasValue && patientId.Value > 0)
            {
                query = query.Where(a => a.PatientId == patientId.Value);
                ViewData["CurrentFilter"] = $"Citas del Paciente ID: {patientId.Value}";
            }
            else if (doctorId.HasValue && doctorId.Value > 0)
            {
                query = query.Where(a => a.DoctorId == doctorId.Value);
                ViewData["CurrentFilter"] = $"Citas del Doctor ID: {doctorId.Value}";
            }
            else
            {
                ViewData["CurrentFilter"] = "Todas las Citas";
            }

            var appointments = await query.ToListAsync();

            // Load required data for dropdowns (always needed)
            ViewBag.patientList = await _context.Patients.ToListAsync();
            ViewBag.doctorList = await _context.Doctors.ToListAsync();

            return View(appointments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF protection
        public async Task<IActionResult> Create([Bind("Date,AppointmentStatus,PatientId,DoctorId")] Appointment appointment)
        {
            // Load required data for dropdowns (in case of validation error)
            ViewBag.patientList = await _context.Patients.ToListAsync();
            ViewBag.doctorList = await _context.Doctors.ToListAsync();

            if (string.IsNullOrEmpty(appointment.AppointmentStatus))
            {
                appointment.AppointmentStatus = "Pendiente";
            }

            if (ModelState.IsValid)
            {
                // --------------------- CONFLICT VALIDATION ---------------------

                // 1. Doctor conflict check: Doctor cannot have two appointments at the same time
                bool doctorConflict = await _context.Appointments
                    .AnyAsync(a => a.DoctorId == appointment.DoctorId && a.Date == appointment.Date && 
                                   a.AppointmentStatus != "Cancelada"); // Exclude cancelled appointments

                if (doctorConflict)
                {
                    ModelState.AddModelError(string.Empty, "The selected doctor already has an appointment scheduled at that time.");
                }

                // 2. Patient conflict check: Patient cannot have two appointments at the same time
                bool patientConflict = await _context.Appointments
                    .AnyAsync(a => a.PatientId == appointment.PatientId && a.Date == appointment.Date &&
                                   a.AppointmentStatus != "Cancelada"); // Exclude cancelled appointments

                if (patientConflict)
                {
                    ModelState.AddModelError(string.Empty, "The selected patient already has another appointment scheduled at that time.");
                }

                // ----------------------------------------------------------------
            }
            
            if (!ModelState.IsValid)
            {
                // Re-fetch appointments, including navigation properties for table rendering
                var appointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .ToListAsync();

                // Return to Index view with current list and validation errors
                return View(nameof(Index), appointments);
            }

            try
            {
                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync(); // Persist to database

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving appointment: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while saving data. Please try again.");

                // Re-fetch appointments for rendering
                var appointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .ToListAsync();
                
                return View(nameof(Index), appointments);
            }
        }
        
        // POST: /Appointment/MarkAsCancelled/5
        [HttpPost]
        public async Task<IActionResult> MarkAsCancelled(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            // Change status to Cancelada
            appointment.AppointmentStatus = "Cancelada";
            _context.Update(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        
        // POST: /Appointment/MarkAsCompleted/5
        [HttpPost]
        public async Task<IActionResult> MarkAsCompleted(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            // Change status to Completada (Attended)
            appointment.AppointmentStatus = "Completada";
            _context.Update(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Appointment/GetByFilter
        [HttpGet]
        public IActionResult GetByFilter(int? patientId, int? doctorId)
        {
            // Redirect to Index action with filter parameters
            return RedirectToAction(nameof(Index), new { patientId = patientId, doctorId = doctorId });
        }
        
        // Placeholder for Edit POST method (updated from your previous code)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,AppointmentStatus,PatientId,DoctorId")] Appointment appointment)
        {
            if (id != appointment.Id)
            {
                return NotFound();
            }

            // Load required data for dropdowns in case of error
            ViewBag.patientList = await _context.Patients.ToListAsync();
            ViewBag.doctorList = await _context.Doctors.ToListAsync();
            
            if (ModelState.IsValid)
            {
                 // Re-check conflicts, excluding the current appointment being edited
                
                // 1. Doctor conflict check
                bool doctorConflict = await _context.Appointments
                    .AnyAsync(a => a.DoctorId == appointment.DoctorId && a.Date == appointment.Date && 
                                   a.Id != appointment.Id && a.AppointmentStatus != "Cancelada"); 

                if (doctorConflict)
                {
                    ModelState.AddModelError(string.Empty, "The selected doctor already has an appointment scheduled at that time.");
                }

                // 2. Patient conflict check
                bool patientConflict = await _context.Appointments
                    .AnyAsync(a => a.PatientId == appointment.PatientId && a.Date == appointment.Date &&
                                   a.Id != appointment.Id && a.AppointmentStatus != "Cancelada"); 

                if (patientConflict)
                {
                    ModelState.AddModelError(string.Empty, "The selected patient already has another appointment scheduled at that time.");
                }
            }
            
            if (!ModelState.IsValid)
            {
                 // Re-fetch appointments for rendering
                var appointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .ToListAsync();
                
                // Return to Index view
                return View(nameof(Index), appointments);
            }

            try
            {
                _context.Update(appointment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Appointments.Any(e => e.Id == appointment.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            
            return RedirectToAction(nameof(Index));
        }
        
        // GET: /Appointment/Delete/5 (Should ideally be POST for security)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }
    }
}