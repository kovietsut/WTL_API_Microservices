using Contracts.Domains;

namespace User.API.Entities
{
    public class Role: EntityBase<long>
    {
        public string? Name { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
