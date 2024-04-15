using Contracts.Domains;

namespace AccessControl.API.Entities
{
    public class Permission : EntityBase<long>
    {
        public long? ActionId { get; set; }
        public long? MangaId { get; set; }
        public long? UserId { get; set; }
        public long? AlbumId { get; set; }
        public string? PermissionType { get; set; } // Grant / Deny
        public virtual Action Action { get; set; } = null!;
    }
}
