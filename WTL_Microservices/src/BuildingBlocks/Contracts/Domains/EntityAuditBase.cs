using Contracts.Domains.Interfaces;

namespace Contracts.Domains
{
    public abstract class EntityAuditBase<T> : EntityBase<T>, IEntityAuditBase<T>
    {
        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? ModifiedAt { get; set; }
    }
}
