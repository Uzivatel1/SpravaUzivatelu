using Microsoft.EntityFrameworkCore;
using SpravaUzivatelu.Models;

namespace SpravaUzivatelu.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context, JsonDataService jsonDataService)
        {
            // Vyhledání existujících uživatelů
            if (context.Uzivatele.Any())
            {
                return; // Databáze byla naplněna daty
            }

            // Resetování Id uživatelů po resetu databáze
            context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('Uzivatele', RESEED, 1)");

            var uzivatele = new List<Uzivatel>
            {
                new Uzivatel { Jmeno = "Irena", Prijmeni = "Novotná" },
                new Uzivatel { Jmeno = "Libor", Prijmeni = "Veselý" },
                new Uzivatel { Jmeno = "Jitka", Prijmeni = "Svobodná" },
                new Uzivatel { Jmeno = "David", Prijmeni = "Opletal" },
                new Uzivatel { Jmeno = "Michaela", Prijmeni = "Vyskočilová" },
                new Uzivatel { Jmeno = "Radim", Prijmeni = "Procházka" },
                new Uzivatel { Jmeno = "Eva", Prijmeni = "Králová" },
                new Uzivatel { Jmeno = "Jan", Prijmeni = "Dvořák" },
                new Uzivatel { Jmeno = "Lenka", Prijmeni = "Malá" },
                new Uzivatel { Jmeno = "Tomáš", Prijmeni = "Švec" },
                new Uzivatel { Jmeno = "Petra", Prijmeni = "Horáková" },
                new Uzivatel { Jmeno = "Marek", Prijmeni = "Kučera" },
                new Uzivatel { Jmeno = "Alena", Prijmeni = "Nováková" },
                new Uzivatel { Jmeno = "Petr", Prijmeni = "Jelínek" },
                new Uzivatel { Jmeno = "Hana", Prijmeni = "Benešová" },
                new Uzivatel { Jmeno = "Jakub", Prijmeni = "Mach" },
                new Uzivatel { Jmeno = "Lucie", Prijmeni = "Holubová" },
                new Uzivatel { Jmeno = "Karel", Prijmeni = "Růžička" },
                new Uzivatel { Jmeno = "Tereza", Prijmeni = "Vlčková" },
                new Uzivatel { Jmeno = "Jiří", Prijmeni = "Černý" },
                new Uzivatel { Jmeno = "Klára", Prijmeni = "Kolářová" },
                new Uzivatel { Jmeno = "Martin", Prijmeni = "Navrátil" },
                new Uzivatel { Jmeno = "Veronika", Prijmeni = "Zemanová" },
                new Uzivatel { Jmeno = "Zdeněk", Prijmeni = "Fiala" }
            };

            // Uložení dat do databáze
            context.AddRange(uzivatele);
            await context.SaveChangesAsync();

            // Uložení dat do JSON souboru
            await jsonDataService.SaveUzivateleAsync(uzivatele);
        }
    }
}
