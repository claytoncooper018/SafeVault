using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SafeVault.Data;
using Microsoft.EntityFrameworkCore;

namespace SafeVault.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        public AuthService(AppDbContext db, IConfiguration config) { _db = db; _config = config; }

        public async Task<bool> RegisterAsync(UserRegisterDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Username == dto.Username)) return false;
            var user = new Models.User { Username = dto.Username, PasswordHash = dto.Password, Role = dto.Role };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<string?> AuthenticateAsync(UserLoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username && u.PasswordHash == dto.Password);
            if (user == null) return null;
            var jwtKey = _config["Jwt:Key"] ?? "ReplaceThisWithASecureKeyForProd";
            var key = Encoding.ASCII.GetBytes(jwtKey);
            var tokenHandler = new JwtSecurityTokenHandler();
            var claims = new[] { new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, user.Role) };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
