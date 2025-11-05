using Xunit;
using SafeVault.Services;
using SafeVault.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SafeVault.Tests
{
    public class AuthTests
    {
        private AppDbContext GetDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("TestDb").Options;
            var db = new AppDbContext(options);
            db.Users.Add(new Models.User { Username = "tester", PasswordHash = "pass", Role = "User" });
            db.SaveChanges();
            return db;
        }

        [Fact]
        public async Task Authenticate_ReturnsToken_ForValidUser()
        {
            var db = GetDb();
            var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder().AddInMemoryCollection().Build();
            var svc = new AuthService(db, config);
            var token = await svc.AuthenticateAsync(new UserLoginDto("tester", "pass"));
            Assert.NotNull(token);
        }
    }
}
