using Manga.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using MangaEntity = Manga.Infrastructure.Entities.Manga;
using ILogger = Serilog.ILogger;

namespace Manga.Infrastructure.Persistence
{
    public class MangaContextSeed
    {
        private readonly ILogger _logger;
        private readonly MangaContext _context;

        public MangaContextSeed(ILogger logger, MangaContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task InitialiseAsync()
        {
            try
            {
                if (_context.Database.IsSqlServer())
                {
                    await _context.Database.MigrateAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while initialising the database.");
                throw;
            }
        }

        public async Task SeedAsync()
        {
            try
            {
                await TrySeedAsync();
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        public async Task TrySeedAsync()
        {
            if (!_context.Mangas.Any())
            {
                await _context.Mangas.AddRangeAsync(
                    new MangaEntity
                    {
                        IsEnabled = true,
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = null,
                        CreatedBy = 1,
                        ModifiedBy = null,
                        Name = "Manga 1",
                        Preface = "Manga 1",
                        Type = "TruyenTranh",
                        Status = "DuyetThanhCong",
                        AmountOfReadings = 0,
                        CoverImage = "https://webtruyenloofficial.blob.core.windows.net/test/f8578341-37ec-4b60-9618-2469852b23d09999atest96.jpeg",
                        Language = (Shared.Enums.LanguageEnum)1,
                        HasAdult = true
                    },
                    new MangaEntity
                    {
                        IsEnabled = true,
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = null,
                        CreatedBy = 1,
                        ModifiedBy = null,
                        Name = "Manga 2",
                        Preface = "Manga 2",
                        Type = "TruyenTranh",
                        Status = "DuyetThanhCong",
                        AmountOfReadings = 0,
                        CoverImage = "https://webtruyenloofficial.blob.core.windows.net/test/f8578341-37ec-4b60-9618-2469852b23d09999atest96.jpeg",
                        Language = (Shared.Enums.LanguageEnum)1,
                        HasAdult = true
                    }
                    );
                await _context.SaveChangesAsync();
            }
            if (!_context.Genres.Any())
            {
                await _context.Genres.AddRangeAsync(
                    new Genre
                    {
                        IsEnabled = true,
                        Name = "Drama"
                    },
                    new Genre
                    {
                        IsEnabled = true,
                        Name ="Fantasy"
                    },
                    new Genre
                    {
                        IsEnabled = true,
                        Name = "Comedy"
                    },
                    new Genre
                    {
                        IsEnabled = true,
                        Name = "Action"
                    },
                    new Genre
                    {
                        IsEnabled = true,
                        Name = "Slice of life"
                    },
                    new Genre
                    {
                        IsEnabled = true,
                        Name = "Romance"
                    },
                    new Genre
                    {
                        IsEnabled = true,
                        Name = "Superhero"
                    },
                    new Genre
                    {
                        IsEnabled = true,
                        Name = "Sci-fi"
                    },
                    new Genre
                    {
                        IsEnabled = true,
                        Name = "Thriller"
                    },
                    new Genre
                    {
                        IsEnabled = true,
                        Name = "Supernatural"
                    },
                    new Genre
                    {
                        IsEnabled = true,
                        Name = "Mystery"
                    },
                    new Genre
                    {
                        IsEnabled = true,
                        Name = "Sports"
                    },
                    new Genre
                    {
                        IsEnabled = true,
                        Name = "Historical"
                    },
                    new Genre
                    {
                        IsEnabled = true,
                        Name = "Heartwarming"
                    },
                    new Genre
                    {
                        IsEnabled = true,
                        Name = "Horror"
                    },
                    new Genre
                    {
                        IsEnabled = true,
                        Name = "Informative"
                    });
                await _context.SaveChangesAsync();
            }
            if (!_context.MangasGenres.Any())
            {
                await _context.MangasGenres.AddRangeAsync(
                    new MangaGenre
                    {
                        IsEnabled = true,
                        GenreId = 1,
                        MangaId = 1,
                    },
                    new MangaGenre
                    {
                        IsEnabled = true,
                        GenreId = 2,
                        MangaId = 2,
                    }
                    );
                await _context.SaveChangesAsync();
            }
            if (!_context.Chapters.Any())
            {
                await _context.Chapters.AddRangeAsync(
                    new Chapter
                    {
                        IsEnabled = true,
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = null,
                        CreatedBy = 1,
                        ModifiedBy = null,
                        NumberOfChapter = 10,
                        Name = "Chapter 1",
                        HasDraft = true,
                        ThumbnailImage = "https://webtruyenloofficial.blob.core.windows.net/test/f8578341-37ec-4b60-9618-2469852b23d09999atest96.jpeg",
                        PublishDate = DateTimeOffset.ParseExact("2024-01-15 10:30:00.0000000 +00:00", "yyyy-MM-dd HH:mm:ss.fffffff zzz", System.Globalization.CultureInfo.InvariantCulture),
                        HasComment = true,
                        Language = (Shared.Enums.LanguageEnum)1,
                        MangaId = 1,
                    });
                await _context.SaveChangesAsync();
            }
            if(!_context.MangaInteractions.Any())
            {
                await _context.MangaInteractions.AddRangeAsync(
                    new MangaInteraction
                    {
                        IsEnabled = true,
                        UserId = 1,
                        MangaId = 1,
                        InteractionType = "Favorite"
                    },
                    new MangaInteraction
                    {
                        IsEnabled = true,
                        UserId = 1,
                        MangaId = 2,
                        InteractionType = "Favorite"
                    }
                );
                await _context.SaveChangesAsync();
            }
            if (!_context.ChapterComments.Any())
            {
                await _context.ChapterComments.AddRangeAsync(
                    new ChapterComment
                    {
                        IsEnabled = true,
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow,
                        CreatedBy = 1,
                        ModifiedBy = 1,
                        ChapterId = 1,
                        ParentCommentId = null,
                        Text = "Truyen hay du doi"
                    }
                );
                await _context.SaveChangesAsync();
            }
            if (!_context.ChapterCommentReactions.Any())
            {
                await _context.ChapterCommentReactions.AddRangeAsync(
                    new ChapterCommentReaction
                    {
                        IsEnabled = true,
                        IsLiked = true,
                        UserId = 1,
                        ChapterCommentId = 1
                    }    
                );
            }
            if (!_context.ChapterImages.Any())
            {
                await _context.ChapterImages.AddRangeAsync(
                    new ChapterImage
                    {
                        IsEnabled = true,
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = null,
                        CreatedBy = 1,
                        ModifiedBy = null,
                        ChapterId = 1,
                        Name = "f8578341-37ec-4b60-9618-2469852b23d09999atest96.jpeg",
                        FileSize = "12MB",
                        MimeType = "JPEG",
                        FilePath = "https://webtruyenloofficial.blob.core.windows.net/test/f8578341-37ec-4b60-9618-2469852b23d09999atest96.jpeg"
                    }
                );
                await _context.SaveChangesAsync();
            }
        }
    }
}
