using Microsoft.EntityFrameworkCore;
using SafeVault.Models;

namespace SafeVault.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Secret> Secrets { get; set; } = null!;
    }

    public static class DbSeeder
    {
        public static void Seed(AppDbContext db)
        {
            if (db.Users.Any()) return;
            db.Users.Add(new User { Username = "admin", PasswordHash = "AdminPass123", Role = "Admin" });
            db.Users.Add(new User { Username = "bob", PasswordHash = "BobPass1", Role = "User" });
            db.SaveChanges();
        }
    }
}
