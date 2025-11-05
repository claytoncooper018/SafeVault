using System;

namespace SafeVault.Models
{
    public class Secret
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Data { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
