using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null, bool googleNotConfigured = false)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        ViewData["GoogleLoginEnabled"] = IsGoogleLoginEnabled();
        if (googleNotConfigured)
            ModelState.AddModelError(string.Empty, "El acceso con Google aún no está configurado.");

        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["GoogleLoginEnabled"] = IsGoogleLoginEnabled();

        var configuredUsername = Environment.GetEnvironmentVariable("ADMIN_USERNAME");
        var configuredPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
        if (string.IsNullOrWhiteSpace(configuredUsername) || string.IsNullOrWhiteSpace(configuredPassword))
        {
            ModelState.AddModelError(string.Empty, "El acceso de administrador no está configurado en el servidor.");
            return View(model);
        }

        if (!ModelState.IsValid)
            return View(model);

        var usernameMatches = string.Equals(model.Username, configuredUsername, StringComparison.OrdinalIgnoreCase);
        var passwordMatches = CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(model.Password)),
            SHA256.HashData(Encoding.UTF8.GetBytes(configuredPassword)));

        if (!usernameMatches || !passwordMatches)
        {
            ModelState.AddModelError(string.Empty, "El usuario o la contraseña son incorrectos.");
            return View(model);
        }

        await SignInAdminAsync(model.Username);
        return LocalRedirectOrDashboard(returnUrl);
    }

    [HttpGet]
    public IActionResult GoogleLogin(string? returnUrl = null)
    {
        if (!IsGoogleLoginEnabled())
            return RedirectToAction(nameof(Login), new { returnUrl, googleNotConfigured = true });

        var destination = Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Action("Index", "Home")!;
        return Challenge(new AuthenticationProperties { RedirectUri = destination }, GoogleDefaults.AuthenticationScheme);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private async Task SignInAdminAsync(string username)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
    }

    private IActionResult LocalRedirectOrDashboard(string? returnUrl) =>
        Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction("Index", "Home");

    private static bool IsGoogleLoginEnabled() =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID"))
        && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET"))
        && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ADMIN_GOOGLE_EMAIL"));
}
