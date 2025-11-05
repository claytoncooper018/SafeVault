# SafeVault Backend

This is an educational demo backend demonstrating secure patterns.

## Run
1. Install .NET 7 SDK.
2. cd backend
3. dotnet restore
4. dotnet run
5. Use Swagger at /swagger in Development

Default seeded users:
- admin / AdminPass123 (Admin)
- bob / BobPass1 (User)

Login to get JWT: POST /api/auth/login { "username":"admin", "password":"AdminPass123" }
