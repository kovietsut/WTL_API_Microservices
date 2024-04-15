using Contracts.Domains;

namespace AccessControl.API.Entities
{
    public class Action: EntityBase<long>
    {
        public string? Name { get; set; }

        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }
}
