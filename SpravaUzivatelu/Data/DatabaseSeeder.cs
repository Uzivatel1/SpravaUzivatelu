using Microsoft.AspNetCore.Identity;

namespace SpravaUzivatelu.Data
{
    public class DatabaseSeeder
    {
        // Metoda pro inicializaci databáze a základních dat
        public static async Task SeedAsync(IServiceProvider services)
        {
            // Získání potřebných služeb pomocí Dependency Injection
            var context = services.GetRequiredService<ApplicationDbContext>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var jsonDataService = services.GetRequiredService<JsonDataService>();

            // Zajistí, že databáze je vytvořena (pokud ještě neexistuje)
            await context.Database.EnsureCreatedAsync();

            // Inicializuje data v databázi (uživatelé, role, atd.)
            await DbInitializer.InitializeAsync(context, jsonDataService);

            // Kontrola a vytvoření role Admin, pokud ještě neexistuje
            if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            }

            // Kontrola existence uživatele "admin"
            var adminUser = await userManager.FindByNameAsync("admin");
            if (adminUser == null)
            {
                // Pokud uživatel "admin" neexistuje, vytvoříme ho s výchozím heslem
                adminUser = new IdentityUser { UserName = "admin" };
                await userManager.CreateAsync(adminUser, "1234");
            }

            // Přidání role Admin uživateli "admin", pokud ještě není přiřazena
            if (!await userManager.IsInRoleAsync(adminUser, UserRoles.Admin))
            {
                await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
            }
        }
    }
}
