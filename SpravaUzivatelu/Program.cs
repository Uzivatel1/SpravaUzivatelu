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

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false; // Nevyžadovat èíslo
                options.Password.RequireLowercase = false; // Nevyžadovat malé písmeno
                options.Password.RequireUppercase = false; // Nevyžadovat velké písmeno
                options.Password.RequireNonAlphanumeric = false; // Nevyžadovat speciální znak
                options.Password.RequiredLength = 4; // Nastavit minimální délku (napø. 4 znaky)
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddErrorDescriber<CzechIdentityErrorDescriberViewModel>(); // Použití èeských chybových zpráv;

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/Account/Login"; // Pøesmìrování na pøihlášení pøi pøístupu bez oprávnìní
            });

            builder.Services.AddControllersWithViews();

            builder.Services.AddScoped<JsonDataService>();

            var app = builder.Build();

            // Nastavení konfigurovatelné IP adresy a portu
            var ipAddress = builder.Configuration["ServerSettings:IPAddress"] ?? "192.168.1.20";
            var httpPort = builder.Configuration["ServerSettings:HttpPort"] ?? "5000"; // HTTP port
            var httpsPort = builder.Configuration["ServerSettings:HttpsPort"] ?? "5001"; // HTTPS port            

            // Pøidání HTTP
            app.Urls.Add($"http://{ipAddress}:{httpPort}");

            // Pøidání HTTPS
            app.Urls.Add($"https://{ipAddress}:{httpsPort}");

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var context = services.GetRequiredService<ApplicationDbContext>();
                var jsonDataService = services.GetRequiredService<JsonDataService>();

                context.Database.EnsureCreated();
                await DbInitializer.InitializeAsync(context, jsonDataService);

                // Vytvoøení role administrátora a pøidání uživatele synchronnì
                RoleManager<IdentityRole> roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                UserManager<IdentityUser> userManager = services.GetRequiredService<UserManager<IdentityUser>>();

                if (!roleManager.RoleExistsAsync(UserRoles.Admin).Result)
                {
                    roleManager.CreateAsync(new IdentityRole(UserRoles.Admin)).Wait();
                }

                IdentityUser? defaultAdminUser = await userManager.FindByNameAsync("admin");
                if (defaultAdminUser is null)
                {
                    var adminUser = new IdentityUser { UserName = "admin" };
                    var createUserResult = userManager.CreateAsync(adminUser, "1234").Result;

                    if (createUserResult.Succeeded)
                    {
                        userManager.AddToRoleAsync(adminUser, UserRoles.Admin).Wait();
                    }
                }
                else if (!userManager.IsInRoleAsync(defaultAdminUser, UserRoles.Admin).Result)
                {
                    userManager.AddToRoleAsync(defaultAdminUser, UserRoles.Admin).Wait();
                }
            }

            // app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Uzivatele}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
