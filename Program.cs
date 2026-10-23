using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;

// 1) Cargar el archivo .env (ASP.NET Core no lo lee solo). En Railway no existe: se usan variables del panel.
var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envFile))
{
    foreach (var linea in File.ReadAllLines(envFile))
    {
        var l = linea.Trim();
        if (l.Length == 0 || l.StartsWith('#') || !l.Contains('=')) continue;
        var partes = l.Split('=', 2);
        Environment.SetEnvironmentVariable(partes[0].Trim(), partes[1].Trim().Trim('"'));
    }
}

// Usar punto decimal siempre. Sin esto, en un Windows en español un precio "9.5" puede leerse como 95.
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

var dataProtectionPath = Environment.GetEnvironmentVariable("DATA_PROTECTION_PATH");
var keyDirectory = string.IsNullOrWhiteSpace(dataProtectionPath)
    ? Path.Combine(builder.Environment.ContentRootPath, "keys")
    : dataProtectionPath;
Directory.CreateDirectory(keyDirectory);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keyDirectory));

builder.Services.AddControllersWithViews(options =>
{
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
        field => $"El campo {field} debe ser un número.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (value, field) => $"El valor '{value}' no es válido para {field}.");
    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
        value => $"El valor '{value}' no es válido.");
});

var googleClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
var googleClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");
var adminGoogleEmail = Environment.GetEnvironmentVariable("ADMIN_GOOGLE_EMAIL");
var googleLoginEnabled = !string.IsNullOrWhiteSpace(googleClientId)
    && !string.IsNullOrWhiteSpace(googleClientSecret)
    && !string.IsNullOrWhiteSpace(adminGoogleEmail);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
    });

if (googleLoginEnabled)
{
    builder.Services.AddAuthentication()
        .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
        {
            options.ClientId = googleClientId!;
            options.ClientSecret = googleClientSecret!;
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.Scope.Add("email");
            options.Events.OnTicketReceived = context =>
            {
                var email = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (!string.Equals(email, adminGoogleEmail, StringComparison.OrdinalIgnoreCase))
                    context.Fail("La cuenta de Google no está autorizada para este sistema.");

                return Task.CompletedTask;
            };
        });
}

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// 2) Base de datos. El proveedor se cambia SOLO en esta línea.
//    Ahora: SQLite (archivo local). Ver README, sección "Conexión a Turso".
var dbPath = Environment.GetEnvironmentVariable("DB_PATH") ?? "ventas.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// 3) Railway entrega el puerto en la variable PORT
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
