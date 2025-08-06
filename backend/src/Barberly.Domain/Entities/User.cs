using System;

namespace Barberly.Domain.Entities
{
    public sealed class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Role { get; set; } = null!; // "customer" | "barber" | "shopowner" | "admin"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
