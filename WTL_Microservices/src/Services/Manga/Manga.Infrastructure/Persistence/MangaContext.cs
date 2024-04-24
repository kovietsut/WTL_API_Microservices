using Manga.Infrastructure.Entities;
using MangaEntity = Manga.Infrastructure.Entities.Manga;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared.Enums;

namespace Manga.Infrastructure.Persistence
{
    public class MangaContext : DbContext
    {
        public MangaContext() { }

        public MangaContext(DbContextOptions<MangaContext> options): base(options) { }

        public virtual DbSet<Chapter> Chapters { get; set; }

        public virtual DbSet<ChapterComment> ChapterComments { get; set; }

        public virtual DbSet<ChapterImage> ChapterImages { get; set; }

        public virtual DbSet<Genre> Genres { get; set; }

        public virtual DbSet<MangaEntity> Mangas { get; set; }

        public virtual DbSet<MangaGenre> MangasGenres { get; set; }

        public virtual DbSet<MangaInteraction> MangaInteractions { get; set; }
        public virtual DbSet<ChapterCommentReaction> ChapterCommentReactions { get; set; }
        public virtual DbSet<Album> Albums { get; set; }
        public virtual DbSet<AlbumManga> AlbumsMangas { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("ConnectionStrings");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Chapter>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.CreatedAt);
                entity.Property(e => e.ModifiedAt);
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.PublishDate);
                entity.Property(e => e.Status).HasMaxLength(200);
                entity.Property(e => e.ThumbnailImage).IsUnicode(false);
                entity.Property(e => e.Content).IsUnicode(false);
                entity.Property(x => x.Language).HasConversion(new EnumToStringConverter<LanguageEnum>());

                entity.HasOne(d => d.Manga).WithMany(p => p.Chapters)
                .HasForeignKey(d => d.MangaId);
            });

            modelBuilder.Entity<ChapterComment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.CreatedAt);
                entity.Property(e => e.ModifiedAt);

                entity.HasOne(d => d.Chapter).WithMany(p => p.ChapterComments)
                    .HasForeignKey(d => d.ChapterId);

                entity.HasOne(d => d.ParentComment).WithMany(p => p.InverseParentComment)
                    .HasForeignKey(d => d.ParentCommentId);
            });

            modelBuilder.Entity<ChapterCommentReaction>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UserId);
                entity.Property(e => e.IsLiked);

                entity.HasOne(d => d.ChapterComment).WithMany(p => p.ChapterCommentReactions)
                    .HasForeignKey(d => d.ChapterCommentId);
            });

            modelBuilder.Entity<ChapterImage>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.CreatedAt);
                entity.Property(e => e.FileSize).HasMaxLength(200);
                entity.Property(e => e.MimeType).HasMaxLength(200);
                entity.Property(e => e.ModifiedAt);

                entity.HasOne(d => d.Chapter).WithMany(p => p.ChapterImages)
                    .HasForeignKey(d => d.ChapterId);
            });

            modelBuilder.Entity<Genre>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name).HasMaxLength(200);
            });

            modelBuilder.Entity<MangaEntity>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.CreatedAt);
                entity.Property(e => e.ModifiedAt);
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.Status).HasMaxLength(200);
                entity.Property(e => e.Type).HasMaxLength(200);
                entity.Property(x => x.Language).HasConversion(new EnumToStringConverter<LanguageEnum>());
            });

            modelBuilder.Entity<MangaGenre>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(d => d.Genre).WithMany(p => p.MangasGenres)
                    .HasForeignKey(d => d.GenreId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Manga).WithMany(p => p.MangasGenres)
                    .HasForeignKey(d => d.MangaId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<MangaInteraction>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.InteractionType).HasMaxLength(200);

                entity.HasOne(d => d.Manga).WithMany(p => p.UserMangaInteractions)
                    .HasForeignKey(d => d.MangaId);

                entity.HasOne(d => d.Chapter).WithMany(p => p.MangaInteractions)
                    .HasForeignKey(d => d.ChapterId); 
            });

            modelBuilder.Entity<Album>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.CreatedAt);
                entity.Property(e => e.CreatedBy);
                entity.Property(e => e.ModifiedAt);
                entity.Property(e => e.ModifiedBy);
                entity.Property(e => e.CoverImage).IsUnicode(false);
            });

            modelBuilder.Entity<AlbumManga>(entity => 
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(m => m.Manga).WithMany(am => am.AlbumsMangas)
                    .HasForeignKey(am => am.MangaId);

                entity.HasOne(a => a.Album).WithMany(am => am.AlbumsMangas)
                    .HasForeignKey(am => am.AlbumId);
            });
        }
    }
}
