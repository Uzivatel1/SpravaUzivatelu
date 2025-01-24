using Microsoft.EntityFrameworkCore;
using SpravaUzivatelu.Models;
using System.Text.Json;

namespace SpravaUzivatelu.Data
{
    public class JsonDataService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _filePath = Path.Combine(Directory.GetCurrentDirectory(), "uzivatele.json");

        // Konstruktor, který přijímá databázový kontext (Dependency Injection)
        public JsonDataService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Přidá uživatele do databáze a zároveň ho uloží do JSON souboru
        public async Task AddUzivatelAsync(Uzivatel uzivatel)
        {
            // Uložení uživatele do databáze
            _context.Add(uzivatel);
            await _context.SaveChangesAsync();

            // Načtení existujících uživatelů z JSON souboru
            var uzivatele = await GetUzivateleAsync();
            uzivatele.Add(uzivatel); // Přidání nového uživatele do seznamu
            await SaveUzivateleAsync(uzivatele); // Uložení seznamu zpět do JSON souboru
        }

        // Aktualizuje uživatele v databázi a JSON souboru
        public async Task EditUzivatelAsync(Uzivatel uzivatel)
        {
            // Aktualizace uživatele v databázi
            _context.Update(uzivatel);
            await _context.SaveChangesAsync();

            // Načtení existujících uživatelů z JSON souboru
            var uzivatele = await GetUzivateleAsync();
            var existingUzivatel = uzivatele.FirstOrDefault(u => u.Id == uzivatel.Id); // Najde uživatele podle ID
            if (existingUzivatel != null)
            {
                // Aktualizace vlastností uživatele
                existingUzivatel.Jmeno = uzivatel.Jmeno;
                existingUzivatel.Prijmeni = uzivatel.Prijmeni;
                await SaveUzivateleAsync(uzivatele); // Uložení změn do JSON souboru
            }
        }

        // Uloží seznam uživatelů do JSON souboru
        public async Task SaveUzivateleAsync(List<Uzivatel> uzivatele)
        {
            try
            {
                // Zajistí, že složka pro soubor existuje
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath));

                // Nastavení pro formátování JSON výstupu
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true, // Přidá mezery pro čitelnější formát
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // Povolení speciálních znaků
                };

                // Serializace seznamu uživatelů do JSON řetězce
                var jsonString = JsonSerializer.Serialize(uzivatele, options);
                await File.WriteAllTextAsync(_filePath, jsonString); // Uložení JSON do souboru
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Chyba při ukládání do JSON souboru: {ex.Message}");
                throw; // Opětovné vyvolání výjimky
            }
        }

        // Načte seznam uživatelů z JSON souboru
        public async Task<List<Uzivatel>> GetUzivateleAsync()
        {
            if (!File.Exists(_filePath)) // Pokud soubor neexistuje, vrátí prázdný seznam
            {
                return new List<Uzivatel>();
            }

            // Načtení obsahu JSON souboru
            var jsonString = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<Uzivatel>>(jsonString) ?? new List<Uzivatel>(); // Deserializace do seznamu uživatelů
        }

        // Smaže uživatele z databáze i JSON souboru
        public async Task DeleteUzivatelAsync(int id)
        {
            // Načtení existujících uživatelů z JSON souboru
            var uzivatele = await GetUzivateleAsync();
            var uzivatel = uzivatele.FirstOrDefault(u => u.Id == id); // Najde uživatele podle ID
            if (uzivatel != null)
            {
                uzivatele.Remove(uzivatel); // Odebere uživatele ze seznamu
                await SaveUzivateleAsync(uzivatele); // Uložení změn do JSON souboru
            }

            // Smazání uživatele z databáze
            var dbUzivatel = await _context.Uzivatele.FindAsync(id); // Najde uživatele v databázi
            if (dbUzivatel != null)
            {
                _context.Uzivatele.Remove(dbUzivatel); // Odstraní uživatele z databáze
                await _context.SaveChangesAsync(); // Uloží změny do databáze
            }
        }

        // Metoda pro inicializaci JSON souboru při spuštění aplikace
        public async Task InitializeJsonFileAsync()
        {
            // Zkontrolujeme, zda JSON soubor neexistuje
            if (!File.Exists(_filePath))
            {
                // Načteme uživatele z databáze
                var uzivatele = await _context.Uzivatele.ToListAsync();

                // Uložíme uživatele do JSON souboru
                await SaveUzivateleAsync(uzivatele);
            }
        }
    }
}
