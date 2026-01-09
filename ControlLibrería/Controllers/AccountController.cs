using ControlLibrería.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ControlLibrería.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("login")]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [Route("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string employeeCode, string password, bool rememberMe = false, string? returnUrl = null)
        {
            var user = await _userManager.FindByNameAsync(employeeCode);
            if (user != null && !user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Esta cuenta ha sido desactivada.");
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(employeeCode, password, rememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }
            
            ModelState.AddModelError(string.Empty, "Código de empleado o contraseña incorrectos.");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [Route("logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [Route("access-denied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
