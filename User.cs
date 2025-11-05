namespace SafeVault.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!; // For demo, this is plain text; in production use hashed
        public string Role { get; set; } = "User";
    }
}
