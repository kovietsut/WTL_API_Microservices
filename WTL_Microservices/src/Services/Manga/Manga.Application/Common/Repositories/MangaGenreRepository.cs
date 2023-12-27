using Contracts.Domains.Interfaces;
using Infrastructure.Common.Repositories;
using Manga.Application.Common.Repositories.Interfaces;
using Manga.Infrastructure.Entities;
using Manga.Infrastructure.Persistence;
using Shared.DTOs.MangaGenre;

namespace Manga.Application.Common.Repositories
{
    public class MangaGenreRepository : RepositoryBase<MangaGenre, long, MangaContext>, IMangaGenreRepository
    {
        public MangaGenreRepository(MangaContext dbContext, IUnitOfWork<MangaContext> unitOfWork) :
            base(dbContext, unitOfWork){}

        public async Task CreateMangaGenre(CreateMangaGenreDto model)
        {
            var mangaGenres = new List<MangaGenre>();
            model.ListGenreId.ForEach(id =>
            {
                mangaGenres.Add(new MangaGenre
                {
                    IsEnabled = true,
                    MangaId = model.MangaId,
                    GenreId = id
                });
            });
            await CreateListAsync(mangaGenres);
        }

        public async Task UpdateMangaGenre(UpdateMangaGenreDto model)
        {
            // Remove List Old Genres
            await RemoveMangaGenre(model.MangaId);
            // Create List New Genres
            var mangaGenres = new List<MangaGenre>();
            model.ListGenreId.ForEach(id =>
            {
                mangaGenres.Add(new MangaGenre
                {
                    IsEnabled = true,
                    MangaId = model.MangaId,
                    GenreId = id
                });
            });
            await CreateListAsync(mangaGenres);
        }

        public async Task RemoveSoftMangaGenre(long mangaId)
        {

        }

        private async Task RemoveMangaGenre(long mangaId)
        {
            var mangaGenres = FindAll().Where(x => x.MangaId == mangaId);
            if (mangaGenres != null && mangaGenres.Count() > 0)
            {
                await DeleteListAsync(mangaGenres);
            }
        }
    }
}
