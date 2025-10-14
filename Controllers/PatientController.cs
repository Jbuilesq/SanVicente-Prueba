using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SanVicente.Infraestructure;
using SanVicente.Models;

namespace SanVicente.Controllers;

public class PatientController : Controller
{
    public readonly DbAppContext _context;
    
    
    public PatientController(DbAppContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
         var patients = await _context.Patients.ToListAsync();
        
        return View(patients);
    }
    // Validate if DNI Exist
    public bool DniExist(string dni)
    {
        return _context.Patients.Any(p => p.DNI == dni);
    }
    
    //Cleans and capitalize names and last names
    private string CleanAndCapitalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;
        string cleaned = Regex.Replace(input, @"\s+", " ").Trim();
        TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
        return ti.ToTitleCase(cleaned.ToLower());
    }
    
    
    [HttpPost]
    public async Task<IActionResult> Create([Bind("Name,LastName,DNI,Age,Phone,Email")] Patient patient)
    {
        if (!string.IsNullOrEmpty(patient.Name))
        {
            patient.Name = CleanAndCapitalize(patient.Name);
        }
       
        if (!string.IsNullOrEmpty(patient.LastName))
        {
            patient.LastName = CleanAndCapitalize(patient.LastName);
        }
        
        if (DniExist(patient.DNI))
        {
            ModelState.AddModelError("DNI", "Ya existe un paciente con este DNI.");
            return View(nameof(Index), await _context.Patients.ToListAsync()); 
        }
        
        if (!ModelState.IsValid)
        {
            return View(nameof(Index), await _context.Patients.ToListAsync());
        }
        try
        {
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {

            Console.WriteLine($"Error al guardar el paciente: {ex.Message}"); 
            ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado al guardar los datos. Inténtelo de nuevo.");
            return View(nameof(Index), await _context.Patients.ToListAsync());
        }
    }
    
    
    [HttpPost]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,LastName,DNI,Age,Phone,Email")] Patient patient)
    {
        if (id != patient.Id) BadRequest();
        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(patient);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                Console.WriteLine("Error al editar el paciente");
                ModelState.AddModelError("string.Empty", "Ocurrió un error al editar paciente ");
            }

            return RedirectToAction(nameof(Index));
        }

        return View(patient);
    }
    
    public async Task<IActionResult> Delete(int id)
    {
        var patient = await _context.Patients.FindAsync(id);
        
        if (patient == null)
        {
            return RedirectToAction(nameof(Index));
        }
        try
        {
            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar el paciente {id}: {ex.Message}");
        }
        
        return RedirectToAction(nameof(Index));
    }
    
    
    
    
}