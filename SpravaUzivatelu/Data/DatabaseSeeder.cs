using Microsoft.AspNetCore.Identity;

namespace SpravaUzivatelu.Data
{
    public class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var jsonDataService = services.GetRequiredService<JsonDataService>();

            // Zajisti vytvoření databáze
            await context.Database.EnsureCreatedAsync();
            await DbInitializer.InitializeAsync(context, jsonDataService);

            // Inicializace rolí a admin uživatele
            if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            }

            var adminUser = await userManager.FindByNameAsync("admin");
            if (adminUser == null)
            {
                adminUser = new IdentityUser { UserName = "admin" };
                await userManager.CreateAsync(adminUser, "1234");
            }

            if (!await userManager.IsInRoleAsync(adminUser, UserRoles.Admin))
            {
                await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
            }
        }
    }
}
