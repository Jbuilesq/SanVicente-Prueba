using Microsoft.EntityFrameworkCore;
using SanVicente.Infraestructure;
using SanVicente.Models.EmailSett;
using SanVicente.Services;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------------
// Coneccion a la base de datos

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<DbAppContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// --------------------------------------------------------------
// Coneccion con el servicio correos
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<EmailService>();

//---------------------------------------------------------------

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
