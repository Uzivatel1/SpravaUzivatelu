using SpravaUzivatelu.Models;
using System.Text.Json;

namespace SpravaUzivatelu.Data
{
    public class JsonDataService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _filePath = Path.Combine(Directory.GetCurrentDirectory(), "uzivatele.json");

        public JsonDataService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Uloží uživatele do databáze a také do JSON souboru
        public async Task AddUzivatelAsync(Uzivatel uzivatel)
        {
            // Uložení do databáze
            _context.Add(uzivatel);
            await _context.SaveChangesAsync();

            // Uložení do JSON souboru
            var uzivatele = await GetUzivateleAsync();
            uzivatele.Add(uzivatel);
            await SaveUzivateleAsync(uzivatele);
        }

        // Uloží změněného uživatele do databáze a JSON souboru
        public async Task EditUzivatelAsync(Uzivatel uzivatel)
        {
            // Uložení do databáze
            _context.Update(uzivatel);
            await _context.SaveChangesAsync();

            // Uložení do JSON souboru
            var uzivatele = await GetUzivateleAsync();
            var existingUzivatel = uzivatele.FirstOrDefault(u => u.Id == uzivatel.Id);
            if (existingUzivatel != null)
            {
                existingUzivatel.Jmeno = uzivatel.Jmeno;
                existingUzivatel.Prijmeni = uzivatel.Prijmeni;
                await SaveUzivateleAsync(uzivatele);
            }
        }

        // Uloží uživatele do JSON souboru
        public async Task SaveUzivateleAsync(List<Uzivatel> uzivatele)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath)); // Vytvoří složku, pokud neexistuje

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var jsonString = JsonSerializer.Serialize(uzivatele, options);
                await File.WriteAllTextAsync(_filePath, jsonString);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Chyba při ukládání do JSON souboru: {ex.Message}");
                throw;
            }
        }

        // Načte všechny uživatele z JSON souboru
        public async Task<List<Uzivatel>> GetUzivateleAsync()
        {
            if (!File.Exists(_filePath))
            {
                return new List<Uzivatel>();
            }

            var jsonString = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<Uzivatel>>(jsonString) ?? new List<Uzivatel>();
        }

        // Smaže uživatele jak z databáze, tak z JSON souboru
        public async Task DeleteUzivatelAsync(int id)
        {
            var uzivatele = await GetUzivateleAsync();
            var uzivatel = uzivatele.FirstOrDefault(u => u.Id == id);
            if (uzivatel != null)
            {
                uzivatele.Remove(uzivatel);
                await SaveUzivateleAsync(uzivatele);
            }

            var dbUzivatel = await _context.Uzivatele.FindAsync(id);
            if (dbUzivatel != null)
            {
                _context.Uzivatele.Remove(dbUzivatel);
                await _context.SaveChangesAsync();
            }
        }
    }
}
