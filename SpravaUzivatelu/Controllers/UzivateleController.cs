using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SpravaUzivatelu.Data;
using SpravaUzivatelu.Models;

namespace SpravaUzivatelu.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    public class UzivateleController : Controller
    {
        // Služby používané v kontroleru
        private readonly JsonDataService _jsonDataService; // Služba pro práci s daty uloženými v JSON
        private readonly ApplicationDbContext _context; // Kontext databáze

        // Konstruktor přijímá služby jako závislosti (Dependency Injection)
        public UzivateleController(JsonDataService jsonDataService, ApplicationDbContext context)
        {
            _jsonDataService = jsonDataService;
            _context = context;
        }

        // Akce pro zobrazení seznamu uživatelů s možností filtrování, třídění a stránkování
        [AllowAnonymous] // Tuto akci může volat i anonymní uživatel
        public async Task<IActionResult> Index(
            string sortOrder, // Parametr pro třídění
            string currentFilterPrijmeni, // Aktuální filtr podle příjmení
            string currentFilterJmeno, // Aktuální filtr podle jména
            string searchPrijmeni, // Hledané příjmení
            string searchJmeno, // Hledané jméno
            int? pageNumber) // Číslo aktuální stránky
        {
            // Uložení aktuálního třídění do ViewData
            ViewData["CurrentSort"] = sortOrder;
            ViewData["IdSortParm"] = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewData["PrijmeniSortParm"] = sortOrder == "Prijmeni" ? "prijmeni_desc" : "Prijmeni";
            ViewData["JmenoSortParm"] = sortOrder == "Jmeno" ? "jmeno_desc" : "Jmeno";

            // Pokud bylo zadáno nové vyhledávání, resetuje se číslo stránky
            if (searchPrijmeni != null || searchJmeno != null)
            {
                pageNumber = 1;
            }
            else
            {
                // Pokud není nové vyhledávání, použije se aktuální filtr
                searchPrijmeni = currentFilterPrijmeni;
                searchJmeno = currentFilterJmeno;
            }

            // Uložení aktuálních filtrů do ViewData
            ViewData["CurrentFilterPrijmeni"] = searchPrijmeni;
            ViewData["CurrentFilterJmeno"] = searchJmeno;

            // Dotaz na uživatele
            var uzivatele = from s in _context.Uzivatele
                            select s;

            // Filtrování podle zadaného příjmení a jména
            if (!string.IsNullOrEmpty(searchPrijmeni) && !string.IsNullOrEmpty(searchJmeno))
            {
                uzivatele = uzivatele.Where(s => s.Prijmeni.Contains(searchPrijmeni) && s.Jmeno.Contains(searchJmeno));
            }
            else if (!string.IsNullOrEmpty(searchPrijmeni))
            {
                uzivatele = uzivatele.Where(s => s.Prijmeni.Contains(searchPrijmeni));
            }
            else if (!string.IsNullOrEmpty(searchJmeno))
            {
                uzivatele = uzivatele.Where(s => s.Jmeno.Contains(searchJmeno));
            }

            // Třídění podle vybraného parametru
            uzivatele = sortOrder switch
            {
                "id_desc" => uzivatele.OrderByDescending(s => s.Id),
                "Prijmeni" => uzivatele.OrderBy(s => s.Prijmeni),
                "prijmeni_desc" => uzivatele.OrderByDescending(s => s.Prijmeni),
                "Jmeno" => uzivatele.OrderBy(s => s.Jmeno),
                "jmeno_desc" => uzivatele.OrderByDescending(s => s.Jmeno),
                _ => uzivatele.OrderBy(s => s.Id),
            };

            // Stránkování (počet položek na stránku)
            int pageSize = 6;
            return View(await PaginatedList<Uzivatel>.CreateAsync(uzivatele.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // Akce pro zobrazení detailů uživatele
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Vrátí chybu 404, pokud není ID zadáno
            }

            // Načte uživatele z JSON dat podle ID
            var uzivatel = (await _jsonDataService.GetUzivateleAsync())
                .FirstOrDefault(m => m.Id == id);
            if (uzivatel == null)
            {
                return NotFound(); // Vrátí chybu 404, pokud uživatel neexistuje
            }

            return View(uzivatel); // Vrátí pohled s detailem uživatele
        }

        // GET akce pro vytvoření nového uživatele
        public IActionResult Create()
        {
            return View(); // Zobrazí formulář pro vytvoření uživatele
        }

        // POST akce pro vytvoření nového uživatele
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Jmeno,Prijmeni")] Uzivatel uzivatel)
        {
            if (ModelState.IsValid) // Kontrola validity vstupních dat
            {
                await _jsonDataService.AddUzivatelAsync(uzivatel); // Uloží uživatele do databáze i JSON
                return RedirectToAction(nameof(Index)); // Přesměruje zpět na seznam
            }
            return View(uzivatel); // Znovu zobrazí formulář, pokud data nejsou validní
        }

        // GET akce pro editaci existujícího uživatele
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Načte uživatele z JSON dat podle ID
            var uzivatel = (await _jsonDataService.GetUzivateleAsync())
                    .FirstOrDefault(m => m.Id == id);
            if (uzivatel == null)
            {
                return NotFound();
            }
            return View(uzivatel); // Zobrazí formulář pro editaci
        }

        // POST akce pro editaci existujícího uživatele
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Jmeno,Prijmeni")] Uzivatel uzivatel)
        {
            if (id != uzivatel.Id)
            {
                return NotFound(); // Vrátí chybu 404, pokud ID nesouhlasí
            }

            if (ModelState.IsValid)
            {
                await _jsonDataService.EditUzivatelAsync(uzivatel); // Aktualizuje uživatele v databázi i JSON
                return RedirectToAction(nameof(Index)); // Přesměruje zpět na seznam
            }
            return View(uzivatel); // Znovu zobrazí formulář, pokud data nejsou validní
        }

        // GET akce pro potvrzení smazání uživatele
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Načte uživatele z JSON dat podle ID
            var uzivatel = (await _jsonDataService.GetUzivateleAsync())
                    .FirstOrDefault(m => m.Id == id);
            if (uzivatel == null)
            {
                return NotFound();
            }

            return View(uzivatel); // Zobrazí formulář pro potvrzení smazání
        }

        // POST akce pro smazání uživatele
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _jsonDataService.DeleteUzivatelAsync(id); // Smaže uživatele z databáze i JSON
            return RedirectToAction(nameof(Index)); // Přesměruje zpět na seznam
        }
    }
}
