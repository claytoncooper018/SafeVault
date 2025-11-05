using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SafeVault.Data;
using SafeVault.Middleware;
using SafeVault.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("SafeVaultDb"));
builder.Services.AddScoped<IAuthService, AuthService>();

var jwtKey = builder.Configuration["Jwt:Key"] ?? "ReplaceThisWithASecureKeyForProd";
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed sample users
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbSeeder.Seed(db);
}

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Endpoints
app.MapPost("/api/auth/register", async (UserRegisterDto dto, IAuthService auth) =>
{
    var errors = dto.Validate();
    if (errors.Any()) return Results.BadRequest(new { success = false, errors });
    var created = await auth.RegisterAsync(dto);
    if (!created) return Results.Conflict(new { success = false, message = "Username already exists" });
    return Results.Ok(new { success = true });
});

app.MapPost("/api/auth/login", async (UserLoginDto dto, IAuthService auth) =>
{
    var token = await auth.AuthenticateAsync(dto);
    if (token == null) return Results.Unauthorized();
    return Results.Ok(new { success = true, token });
});

app.MapGet("/api/users", [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")] (AppDbContext db) =>
{
    var users = db.Users.Select(u => new { u.Username, u.Role }).ToList();
    return Results.Ok(new { success = true, data = users });
});

app.MapGet("/api/profile", [Microsoft.AspNetCore.Authorization.Authorize] (System.Security.Claims.ClaimsPrincipal user, AppDbContext db) =>
{
    var username = user.Identity?.Name;
    var u = db.Users.FirstOrDefault(x => x.Username == username);
    if (u == null) return Results.NotFound(new { success = false });
    return Results.Ok(new { success = true, data = new { u.Username, u.Role } });
});

app.MapPost("/api/secrets", [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")] async (SecretCreateDto dto, AppDbContext db) =>
{
    var errors = dto.Validate();
    if (errors.Any()) return Results.BadRequest(new { success = false, errors });
    var s = new Secret { Id = Guid.NewGuid(), Title = dto.Title, Data = dto.Data, CreatedAt = DateTime.UtcNow };
    db.Secrets.Add(s);
    await db.SaveChangesAsync();
    return Results.Created($"/api/secrets/{s.Id}", new { success = true, data = s });
});

app.MapGet("/api/secrets", [Microsoft.AspNetCore.Authorization.Authorize] (AppDbContext db) =>
{
    var list = db.Secrets.Select(s => new { s.Id, s.Title, s.CreatedAt }).ToList();
    return Results.Ok(new { success = true, data = list });
});

app.Run();

// DTOs and minimal models used in Program for binding (kept simple)
public record UserRegisterDto(string Username, string Password, string Role)
{
    public IEnumerable<string> Validate()
    {
        var errs = new List<string>();
        if (string.IsNullOrWhiteSpace(Username)) errs.Add("Username required");
        if (string.IsNullOrWhiteSpace(Password) || Password.Length < 6) errs.Add("Password must be >= 6 chars");
        if (string.IsNullOrWhiteSpace(Role)) errs.Add("Role required");
        return errs;
    }
}
public record UserLoginDto(string Username, string Password);

public record SecretCreateDto(string Title, string Data)
{
    public IEnumerable<string> Validate()
    {
        var errs = new List<string>();
        if (string.IsNullOrWhiteSpace(Title)) errs.Add("Title required");
        if (string.IsNullOrWhiteSpace(Data)) errs.Add("Data required");
        if (Title != null && Title.ToLower().Contains("<script")) errs.Add("Invalid input (possible XSS)") ;
        return errs;
    }
}

public record Secret(Guid Id, string Title, string Data, DateTime CreatedAt);
