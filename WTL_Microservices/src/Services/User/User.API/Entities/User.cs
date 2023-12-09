using Contracts.Domains;
using Newtonsoft.Json.Linq;
using System.Data;
using System;

namespace User.API.Entities
{
    public class User: EntityAuditBase<long>
    {
        public long RoleId { get; set; }

        public string Email { get; set; } = null!;

        public string? PasswordHash { get; set; }

        public string? SecurityStamp { get; set; }

        public string? GoogleUserId { get; set; }

        public string? FullName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public string? Gender { get; set; }

        public string? AvatarPath { get; set; }

        public virtual Role Role { get; set; } = null!;
        public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();

    }
}
