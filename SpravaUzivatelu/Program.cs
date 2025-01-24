using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SpravaUzivatelu.Data;
using SpravaUzivatelu.Models;

namespace SpravaUzivatelu
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Pøidání služeb do kontejneru služeb.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                                   ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString)); // Konfigurace použití SQL Serveru
            builder.Services.AddDatabaseDeveloperPageExceptionFilter(); // Filtrování výjimek pøi vývoji databáze

            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                // Konfigurace politiky hesel
                options.Password.RequireDigit = false; // Nevyžadovat èíslo
                options.Password.RequireLowercase = false; // Nevyžadovat malé písmeno
                options.Password.RequireUppercase = false; // Nevyžadovat velké písmeno
                options.Password.RequireNonAlphanumeric = false; // Nevyžadovat speciální znak
                options.Password.RequiredLength = 4; // Minimální délka hesla nastavena na 4 znaky
            })
            .AddEntityFrameworkStores<ApplicationDbContext>() // Ukládání identit do databáze
            .AddErrorDescriber<CzechIdentityErrorDescriberViewModel>(); // Použití èeských chybových hlášení

            builder.Services.ConfigureApplicationCookie(options =>
            {
                // Pøesmìrování na pøihlašovací stránku pøi pokusu o pøístup bez oprávnìní
                options.AccessDeniedPath = "/Account/Login";
            });

            builder.Services.AddControllersWithViews(); // Povolení MVC kontrolerù a pohledù

            builder.Services.AddScoped<JsonDataService>(); // Registrace JsonDataService jako závislosti

            var app = builder.Build(); // Vytvoøení aplikace

            // Konfigurovatelná IP adresa a porty
            var ipAddress = builder.Configuration["ServerSettings:IPAddress"] ?? "192.168.1.20"; // Výchozí IP
            var httpsPort = builder.Configuration["ServerSettings:HttpsPort"] ?? "5000"; // Výchozí HTTPS port
            var httpPort = builder.Configuration["ServerSettings:HttpPort"] ?? "5001"; // Výchozí HTTP port

            // Pøidání HTTPS: odeberte komentáø pro povolení pøipojení pøes HTTPS
            app.Urls.Add($"https://{ipAddress}:{httpsPort}");

            // Pøidání HTTP: odeberte komentáø pro povolení pøipojení pøes HTTP
            app.Urls.Add($"http://{ipAddress}:{httpPort}");

            // Konfigurace HTTP pipeline
            if (app.Environment.IsDevelopment()) // Vývojový režim
            {
                app.UseMigrationsEndPoint(); // Povolí zobrazení konce migrace
            }
            else // Produkèní režim
            {
                app.UseExceptionHandler("/Home/Error"); // Obsluha výjimek s pøesmìrováním na stránku s chybou
                app.UseHsts(); // Povolení HTTP Strict Transport Security (HSTS)
            }

            using (var scope = app.Services.CreateScope())
            {
                // Inicializace databáze
                await DatabaseSeeder.SeedAsync(scope.ServiceProvider);

                // Volání metody pro inicializaci JSON souboru
                var jsonDataService = scope.ServiceProvider.GetRequiredService<JsonDataService>();
                await jsonDataService.InitializeJsonFileAsync(); // Inicializace souboru pøi spuštìní aplikace
            }

            app.UseHttpsRedirection(); // Pøesmìrování HTTP na HTTPS
            app.UseStaticFiles(); // Povolí obsluhu statických souborù

            app.UseRouting(); // Povolí smìrování

            app.UseAuthorization(); // Povolí autorizaci

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Uzivatele}/{action=Index}/{id?}"); // Výchozí routovací šablona

            app.Run(); // Spuštìní aplikace
        }
    }
}
