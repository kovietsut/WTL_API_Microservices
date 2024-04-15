using AccessControl.API.Entities;
using Contracts.Domains.Interfaces;
using Microsoft.EntityFrameworkCore;
using ActionEntity = AccessControl.API.Entities.Action;

namespace AccessControl.API.Persistence
{
    public class AccessControlContext: DbContext
    {
        public AccessControlContext(DbContextOptions<AccessControlContext> options) : base(options)
        {
        }

        public DbSet<ActionEntity> Actions { get; set; }
        public DbSet<Permission> Permissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ActionEntity>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name).HasMaxLength(200);
            });

            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.PermissionType).HasMaxLength(200);

                entity.HasOne(d => d.Action).WithMany(p => p.Permissions)
                    .HasForeignKey(d => d.ActionId);
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var modified = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified ||
                            e.State == EntityState.Added ||
                            e.State == EntityState.Deleted);

            foreach (var item in modified)
            {
                switch (item.State)
                {
                    case EntityState.Added:
                        if (item.Entity is IDateTracking addedEntity)
                        {
                            addedEntity.CreatedAt = DateTime.UtcNow;
                            item.State = EntityState.Added;
                        }
                        break;

                    case EntityState.Modified:
                        Entry(item.Entity).Property("Id").IsModified = false;
                        if (item.Entity is IDateTracking modifiedEntity)
                        {
                            modifiedEntity.ModifiedAt = DateTime.UtcNow;
                            item.State = EntityState.Modified;
                        }
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
