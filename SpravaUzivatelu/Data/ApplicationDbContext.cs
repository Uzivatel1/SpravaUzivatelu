using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SpravaUzivatelu.Models;

namespace SpravaUzivatelu.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<SpravaUzivatelu.Models.Uzivatel> Uzivatele { get; set; } = default!;
    }
}
