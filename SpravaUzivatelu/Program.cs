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

            // P�id�n� slu�eb do kontejneru slu�eb.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                                   ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString)); // Konfigurace pou�it� SQL Serveru
            builder.Services.AddDatabaseDeveloperPageExceptionFilter(); // Filtrov�n� v�jimek p�i v�voji datab�ze

            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                // Konfigurace politiky hesel
                options.Password.RequireDigit = false; // Nevy�adovat ��slo
                options.Password.RequireLowercase = false; // Nevy�adovat mal� p�smeno
                options.Password.RequireUppercase = false; // Nevy�adovat velk� p�smeno
                options.Password.RequireNonAlphanumeric = false; // Nevy�adovat speci�ln� znak
                options.Password.RequiredLength = 4; // Minim�ln� d�lka hesla nastavena na 4 znaky
            })
            .AddEntityFrameworkStores<ApplicationDbContext>() // Ukl�d�n� identit do datab�ze
            .AddErrorDescriber<CzechIdentityErrorDescriberViewModel>(); // Pou�it� �esk�ch chybov�ch hl�en�

            builder.Services.ConfigureApplicationCookie(options =>
            {
                // P�esm�rov�n� na p�ihla�ovac� str�nku p�i pokusu o p��stup bez opr�vn�n�
                options.AccessDeniedPath = "/Account/Login";
            });

            builder.Services.AddControllersWithViews(); // Povolen� MVC kontroler� a pohled�

            builder.Services.AddScoped<JsonDataService>(); // Registrace JsonDataService jako z�vislosti

            var app = builder.Build(); // Vytvo�en� aplikace

            // Konfigurovateln� IP adresa a porty
            var ipAddress = builder.Configuration["ServerSettings:IPAddress"] ?? "0.0.0.0"; // V�choz� IP, nap�. 192.162.1.20
            var httpsPort = builder.Configuration["ServerSettings:HttpsPort"] ?? "5000"; // V�choz� HTTPS port
            var httpPort = builder.Configuration["ServerSettings:HttpPort"] ?? "5001"; // V�choz� HTTP port

            // P�id�n� HTTPS: odeberte koment�� pro povolen� p�ipojen� p�es HTTPS
            app.Urls.Add($"https://{ipAddress}:{httpsPort}");

            // P�id�n� HTTP: odeberte koment�� pro povolen� p�ipojen� p�es HTTP
            app.Urls.Add($"http://{ipAddress}:{httpPort}");

            // Konfigurace HTTP pipeline
            if (app.Environment.IsDevelopment()) // V�vojov� re�im
            {
                app.UseMigrationsEndPoint(); // Povol� zobrazen� konce migrace
            }
            else // Produk�n� re�im
            {
                app.UseExceptionHandler("/Home/Error"); // Obsluha v�jimek s p�esm�rov�n�m na str�nku s chybou
                app.UseHsts(); // Povolen� HTTP Strict Transport Security (HSTS)
            }

            using (var scope = app.Services.CreateScope())
            {
                // Inicializace datab�ze
                await DatabaseSeeder.SeedAsync(scope.ServiceProvider);

                // Vol�n� metody pro inicializaci JSON souboru
                var jsonDataService = scope.ServiceProvider.GetRequiredService<JsonDataService>();
                await jsonDataService.InitializeJsonFileAsync(); // Inicializace souboru p�i spu�t�n� aplikace
            }

            app.UseHttpsRedirection(); // P�esm�rov�n� HTTP na HTTPS
            app.UseStaticFiles(); // Povol� obsluhu statick�ch soubor�

            app.UseRouting(); // Povol� sm�rov�n�

            // I p�esto �e middleware Identity se o autentizaci star� automaticky, nicm�n�, doporu�uje se ho ponechat,
            // aby byla aplikace p�ipravena na roz���en� o dal�� autentiza�n� metody a spr�vn� zpracov�n� po�adavk�.
            app.UseAuthentication();

            app.UseAuthorization(); // Povol� autorizaci

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Uzivatele}/{action=Index}/{id?}"); // V�choz� routovac� �ablona

            app.Run(); // Spu�t�n� aplikace
        }
    }
}
