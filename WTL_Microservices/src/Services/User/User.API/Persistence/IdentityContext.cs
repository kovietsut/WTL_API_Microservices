using Contracts.Domains.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;
using User.API.Entities;
using UserEntity = User.API.Entities.User;

namespace User.API.Persistence
{
    public class IdentityContext: DbContext
    {
        public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
        {
        }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public virtual DbSet<Token> Tokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name).HasMaxLength(200);
            });

            modelBuilder.Entity<Token>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.AccessToken).IsUnicode(false);
                entity.Property(e => e.AccessTokenExpiration);
                entity.Property(e => e.RefreshToken).IsUnicode(false);
                entity.Property(e => e.RefreshTokenExpiration);

                entity.HasOne(d => d.User).WithMany(p => p.Tokens)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<UserEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Address).IsUnicode(false);
                entity.Property(e => e.CreatedAt);
                entity.Property(e => e.Email)
                    .HasMaxLength(200)
                    .IsUnicode(false);
                entity.Property(e => e.FullName).HasMaxLength(200);
                entity.Property(e => e.Gender)
                    .HasMaxLength(200)
                    .IsUnicode(false);
                entity.Property(e => e.GoogleUserId)
                    .HasMaxLength(255)
                    .IsUnicode(false);
                entity.Property(e => e.ModifiedAt);
                entity.Property(e => e.PasswordHash).IsUnicode(false);
                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(200)
                    .IsUnicode(false);
                entity.Property(e => e.SecurityStamp).IsUnicode(false);
                entity.HasOne(d => d.Role).WithMany(p => p.Users)
                    .HasForeignKey(d => d.RoleId);
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
