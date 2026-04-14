using Microsoft.AspNetCore.Authentication.Cookies;
using Rotativa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// --- CONFIGURACIÓN DE LLAVES (AZURE O LOCAL) ---
// Intentamos leer de Azure (Mayúsculas) o del JSON (Jerarquía)
var supabaseUrl = builder.Configuration["SUPABASE_URL"] ?? builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["SUPABASE_KEY"] ?? builder.Configuration["Supabase:Key"];
var cloudinaryUrl = builder.Configuration["CLOUDINARY_URL"]; // Para Cloudinary

// Solo pasamos la URL y la Key, las opciones por defecto ya funcionan bien
builder.Services.AddScoped(_ => new Supabase.Client(supabaseUrl, supabaseKey));
// -----------------------------------------------

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
    config.AccessDeniedPath = "/Account/Login";
    config.ExpireTimeSpan = TimeSpan.FromHours(8);
    config.Cookie.HttpOnly = true;
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

IWebHostEnvironment env = app.Environment;

// Detectamos si es Windows o Linux para elegir la carpeta/ejecutable
string folderName = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)
                    ? "Rotativa"
                    : "Rotativa"; // Si pusiste el binario de Linux dentro de una subcarpeta llamada 'Linux', cámbialo aquí a "Rotativa/Linux"

RotativaConfiguration.Setup(env.WebRootPath, folderName);

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Mantenimientos}/{action=Index}/{id?}");

app.Run();