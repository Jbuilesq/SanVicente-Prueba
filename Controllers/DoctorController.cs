using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SanVicente.Infraestructure;
using SanVicente.Models;

namespace SanVicente.Controllers;

public class DoctorController : Controller
{
    public readonly DbAppContext _context;
    
    
    public DoctorController(DbAppContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
         var doctors = await _context.Doctors.ToListAsync();
        
        return View(doctors);
    }
    // Validate if DNI Exist
    public bool DniExist(string dni)
    {
        return _context.Doctors.Any(p => p.DNI == dni);
    }
    
    //Cleans and capitalize names and last names
    private string CleanAndCapitalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;
        string cleaned = Regex.Replace(input, @"\s+", " ").Trim();
        TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
        return ti.ToTitleCase(cleaned.ToLower());
    }


    [HttpGet]
    public IActionResult Index(string specialtyFilter)
    {
        IQueryable<Doctor> doctorsQuery = _context.Doctors;
        if (!string.IsNullOrEmpty(specialtyFilter))
        {
            doctorsQuery = doctorsQuery.Where(d => d.Specialty.ToString() == specialtyFilter);
        }
        IEnumerable<Doctor> doctorsToDisplay = doctorsQuery.ToList();
        return View(doctorsToDisplay);
    }

    [HttpPost]
    public async Task<IActionResult> Create([Bind("Name,LastName,DNI,Age,Phone,Email,Specialty")] Doctor doctor)
    {
        if (!string.IsNullOrEmpty(doctor.Name))
        {
            doctor.Name = CleanAndCapitalize(doctor.Name);
        }
       
        if (!string.IsNullOrEmpty(doctor.LastName))
        {
            doctor.LastName = CleanAndCapitalize(doctor.LastName);
        }
        
        if (DniExist(doctor.DNI))
        {
            ModelState.AddModelError("DNI", "Ya existe un doctor con este DNI.");
            return View(nameof(Index), await _context.Doctors.ToListAsync()); 
        }
        
        if (!ModelState.IsValid)
        {
            return View(nameof(Index), await _context.Doctors.ToListAsync());
        }
        try
        {
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {

            Console.WriteLine($"Error al guardar el doctor: {ex.Message}"); 
            ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado al guardar los datos. Inténtelo de nuevo.");
            return View(nameof(Index), await _context.Doctors.ToListAsync());
        }
    }
    
    
    [HttpPost]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,LastName,DNI,Age,Phone,Email,Specialty")] Doctor doctor)
    {
        if (id != doctor.Id) BadRequest();
        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(doctor);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                Console.WriteLine("Error al editar el doctor");
                ModelState.AddModelError("string.Empty", "Ocurrió un error al editar el doctor ");
            }

            return RedirectToAction(nameof(Index));
        }

        return View(doctor);
    }
    
    public async Task<IActionResult> Delete(int id)
    {
        var doctor = await _context.Doctors.FindAsync(id);
        
        if (doctor == null)
        {
            return RedirectToAction(nameof(Index));
        }
        try
        {
            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar el doctor {id}: {ex.Message}");
        }
        
        return RedirectToAction(nameof(Index));
    }
}