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
        private readonly JsonDataService _jsonDataService;
        private readonly ApplicationDbContext _context;

        public UzivateleController(JsonDataService jsonDataService, ApplicationDbContext context)
        {
            _jsonDataService = jsonDataService;
            _context = context;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(
            string sortOrder,
            string currentFilterPrijmeni,
            string currentFilterJmeno,
            string searchPrijmeni,
            string searchJmeno,
            int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["IdSortParm"] = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewData["PrijmeniSortParm"] = sortOrder == "Prijmeni" ? "prijmeni_desc" : "Prijmeni";
            ViewData["JmenoSortParm"] = sortOrder == "Jmeno" ? "jmeno_desc" : "Jmeno";

            if (searchPrijmeni != null || searchJmeno != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchPrijmeni = currentFilterPrijmeni;
                searchJmeno = currentFilterJmeno;
            }

            ViewData["CurrentFilterPrijmeni"] = searchPrijmeni;
            ViewData["CurrentFilterJmeno"] = searchJmeno;

            var uzivatele = from s in _context.Uzivatele
                            select s;

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

            uzivatele = sortOrder switch
            {
                "id_desc" => uzivatele.OrderByDescending(s => s.Id),
                "Prijmeni" => uzivatele.OrderBy(s => s.Prijmeni),
                "prijmeni_desc" => uzivatele.OrderByDescending(s => s.Prijmeni),
                "Jmeno" => uzivatele.OrderBy(s => s.Jmeno),
                "jmeno_desc" => uzivatele.OrderByDescending(s => s.Jmeno),
                _ => uzivatele.OrderBy(s => s.Id),
            };

            int pageSize = 6;
            return View(await PaginatedList<Uzivatel>.CreateAsync(uzivatele.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Uzivatele/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uzivatel = (await _jsonDataService.GetUzivateleAsync())
                .FirstOrDefault(m => m.Id == id);
            if (uzivatel == null)
            {
                return NotFound();
            }

            return View(uzivatel);
        }

        // GET: Uzivatele/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Uzivatele/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Jmeno,Prijmeni")] Uzivatel uzivatel)
        {
            if (ModelState.IsValid)
            {
                await _jsonDataService.AddUzivatelAsync(uzivatel); // Uloží jak do databáze, tak do JSON souboru
                return RedirectToAction(nameof(Index));
            }
            return View(uzivatel);
        }

        // GET: Uzivatele/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uzivatel = (await _jsonDataService.GetUzivateleAsync())
                    .FirstOrDefault(m => m.Id == id);
            if (uzivatel == null)
            {
                return NotFound();
            }
            return View(uzivatel);
        }

        // POST: Uzivatele/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Jmeno,Prijmeni")] Uzivatel uzivatel)
        {
            if (id != uzivatel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _jsonDataService.EditUzivatelAsync(uzivatel); // Uloží jak do databáze, tak do JSON souboru
                return RedirectToAction(nameof(Index));
            }
            return View(uzivatel);
        }

        // GET: Uzivatele/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uzivatel = (await _jsonDataService.GetUzivateleAsync())
                    .FirstOrDefault(m => m.Id == id);
            if (uzivatel == null)
            {
                return NotFound();
            }

            return View(uzivatel);
        }

        // POST: Uzivatele/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _jsonDataService.DeleteUzivatelAsync(id); // Smaže jak z databáze, tak z JSON souboru
            return RedirectToAction(nameof(Index));
        }
    }
}
