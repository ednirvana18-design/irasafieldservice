using Microsoft.AspNetCore.Authentication.Cookies;
using Rotativa.AspNetCore;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

// --- CONFIGURACIÓN DE PUERTO PARA RENDER ---
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// --- CONFIGURACIÓN DE LLAVES ---
var supabaseUrl = builder.Configuration["SUPABASE_URL"] ?? builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["SUPABASE_KEY"] ?? builder.Configuration["Supabase:Key"];

builder.Services.AddScoped(_ => new Supabase.Client(supabaseUrl, supabaseKey));

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "CookieAuth";
    options.DefaultSignInScheme = "CookieAuth";
    options.DefaultChallengeScheme = "CookieAuth";
})
.AddCookie("CookieAuth", config =>
{
    config.Cookie.Name = "ServiceTower.Cookie";
    config.LoginPath = "/Account/Login";
    config.ExpireTimeSpan = TimeSpan.FromHours(8);
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// COMENTA ESTA LÍNEA SI RENDER TE DA ERROR DE "TOO MANY REDIRECTS"
// Render ya maneja el SSL (HTTPS) por fuera.
// app.UseHttpsRedirection(); 

app.UseStaticFiles();

IWebHostEnvironment env = app.Environment;

// --- AJUSTE DE ROTATIVA PARA LINUX (RENDER) ---
string folderName = "Rotativa"; // Carpeta para Windows (Local)

if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    // En Docker/Linux, forzamos a que use el binario del sistema
    // o el que está en la carpeta específica de Linux
    folderName = "Rotativa/Linux";
}

// Configuración global
RotativaConfiguration.Setup(env.WebRootPath, folderName);

// OPCIONAL: Si el error de "Permission Denied" sigue en el controlador, 
// asegúrate de que el controlador NO sobreescriba esta ruta con una de Windows.

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Mantenimientos}/{action=Index}/{id?}");

app.MapGet("/api/ping", () => Results.Content("OK", "text/plain"))
   .AllowAnonymous();

app.Run();
