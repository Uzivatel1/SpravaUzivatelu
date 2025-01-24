using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SpravaUzivatelu.Models;

namespace SpravaUzivatelu.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;

        // Dependency Injection: UserManager a SignInManager jsou injektovány do kontroleru
        public AccountController
        (
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager
        )
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        // Přesměrování na požadovanou stránku po úspěšném přihlášení
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            // Kontroluje, zda je returnUrl platná lokální adresa
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl); // Přesměrování na původní stránku
            }
            // Pokud není URL platná, přesměruje na výchozí stránku se seznamem uživatelů
            return RedirectToAction(nameof(UzivateleController.Index), "Uzivatele");
        }

        // GET akce pro zobrazení přihlašovacího formuláře
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl; // Ukládá návratovou URL do ViewData
            return View(); // Vrátí pohled s formulářem
        }

        // POST akce pro zpracování přihlášení
        [HttpPost]
        [ValidateAntiForgeryToken] // Chrání proti CSRF útokům
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl; // Ukládá návratovou URL do ViewData

            if (ModelState.IsValid) // Kontrola validity modelu
            {
                // Pokus o přihlášení uživatele s poskytnutými přihlašovacími údaji
                Microsoft.AspNetCore.Identity.SignInResult result =
                    await signInManager.PasswordSignInAsync(model.Login, model.Password, model.RememberMe, false);

                if (result.Succeeded) // Pokud je přihlášení úspěšné
                    return RedirectToLocal(returnUrl); // Přesměrování na původní stránku

                // Přidání chyby do ModelState, pokud přihlášení selhalo
                ModelState.AddModelError("Login error", "Neplatné přihlašovací údaje.");
                return View(model); // Zobrazení formuláře znovu
            }

            // Pokud model není validní, zobrazí formulář znovu
            return View(model);
        }

        // GET akce pro zobrazení registračního formuláře
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl; // Ukládá návratovou URL do ViewData
            return View(); // Vrátí pohled s registračním formulářem
        }

        // POST akce pro zpracování registrace nového uživatele
        [HttpPost]
        [ValidateAntiForgeryToken] // Chrání proti CSRF útokům
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl; // Ukládá návratovou URL do ViewData

            if (ModelState.IsValid) // Kontrola validity modelu
            {
                // Vytvoření nového uživatele s uživatelským jménem a heslem
                IdentityUser user = new IdentityUser { UserName = model.Login };
                IdentityResult result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded) // Pokud je registrace úspěšná
                {
                    await signInManager.SignInAsync(user, isPersistent: false); // Automatické přihlášení uživatele
                    return RedirectToLocal(returnUrl); // Přesměrování na původní stránku
                }

                // Přidání chyb z procesu registrace do ModelState
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
            }

            // Pokud model není validní, zobrazí formulář znovu
            return View(model);
        }

        // Akce pro odhlášení uživatele
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync(); // Odhlášení uživatele
            return RedirectToLocal(null); // Přesměrování na výchozí stránku
        }
    }
}
