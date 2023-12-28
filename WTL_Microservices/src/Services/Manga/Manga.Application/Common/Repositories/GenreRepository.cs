using Contracts.Domains.Interfaces;
using Infrastructure.Common.Repositories;
using Manga.Application.Common.Repositories.Interfaces;
using Manga.Infrastructure.Entities;
using Manga.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.DTOs;
using Shared.DTOs.Genre;
using Shared.SeedWork;

namespace Manga.Application.Common.Repositories
{
    public class GenreRepository: RepositoryBase<Genre, long, MangaContext>, IGenreRepository
    {
        private readonly ErrorCode _errorCodes;
        public GenreRepository(MangaContext dbContext, IUnitOfWork<MangaContext> unitOfWork, IOptions<ErrorCode> errorCode) :
            base(dbContext, unitOfWork)
        {
            _errorCodes = errorCode.Value;
        }

        public Task<Genre> GetGenreById(long genreId) => FindByCondition(x => x.Id == genreId).SingleOrDefaultAsync();

        public async Task<IActionResult> GetGenre(long genreId)
        {
            var genre = await GetGenreById(genreId);
            if (genre == null)
            {
                return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Genre does not exist");
            }
            var genreResult = new
            {
                genre.Id,
                genre.IsEnabled,
                genre.Name
            };
            return JsonUtil.Success(genreResult);
        }

        public async Task<IActionResult> GetListGenre(int? pageNumber, int? pageSize, string? searchText)
        {
            try
            {
                pageNumber ??= 1; pageSize ??= 10;
                var list = FindAll().Where(x => x.IsEnabled == true && (searchText == null || x.Name.Contains(searchText.Trim())))
                    .Select(x => new
                    {
                        GenreId = x.Id,
                        x.IsEnabled,
                        x.Name
                    });
                var listData = list.Skip(((int)pageNumber - 1) * (int)pageSize)
                    .Take((int)pageSize).OrderByDescending(x => x.GenreId).ToList();
                if (list != null)
                {
                    var totalRecords = list.Count();
                    return JsonUtil.Success(listData, dataCount: totalRecords);
                }
                return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Empty List Data");
            }
            catch (Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, ex.Message);
            }
        }

        public async Task<IActionResult> CreateGenre(GenreDto model)
        {
            try
            {
                // Validator
                var validator = new GenreValidator();
                var check = await validator.ValidateAsync(model);
                if (!check.IsValid)
                {
                    return JsonUtil.Errors(StatusCodes.Status400BadRequest, _errorCodes.Status400.ConstraintViolation, check.Errors);
                }
                // Genre
                Genre genre = new()
                {
                    IsEnabled = true,
                    Name = model.Name,
                };
                await CreateAsync(genre);
                return JsonUtil.Success(genre);
            }
            catch (Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.APIServerError, ex.Message);
            }
        }

        public async Task<IActionResult> UpdateGenre(long genreId, GenreDto model)
        {
            try
            {
                // Validator
                var validator = new GenreValidator();
                var check = await validator.ValidateAsync(model);
                if (!check.IsValid)
                {
                    return JsonUtil.Errors(StatusCodes.Status400BadRequest, _errorCodes.Status400.ConstraintViolation, check.Errors);
                }
                // Genre
                var genre = await GetByIdAsync(genreId);
                if (genre == null)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Genre does not exist");
                }
                genre.Name = model.Name;
                await UpdateAsync(genre);
                return JsonUtil.Success(genre);
            }
            catch (Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.APIServerError, ex.Message);
            }
        }

        public async Task<IActionResult> RemoveSoftGenre(long genreId)
        {
            try
            {
                // Genre
                var genre = await GetByIdAsync(genreId);
                if (genre == null)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Genre does not exist");
                }
                genre.IsEnabled = false;
                await UpdateAsync(genre);
                return JsonUtil.Success(genre.Id);
            }
            catch (Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.APIServerError, ex.Message);
            }
        }

        public async Task<IActionResult> RemoveSoftListGenre(string ids)
        {
            try
            {
                await BeginTransactionAsync();
                var list = new List<Genre>();
                if (ids.IsNullOrEmpty())
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Ids cannot be null");
                }
                var listIds = Util.SplitStringToArray(ids);
                // List Genre
                var genres = FindAll().Where(x => listIds.Contains(x.Id));
                if (genres == null || genres.Count() == 0)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Cannot get list genre");
                }
                foreach (var genre in genres)
                {
                    genre.IsEnabled = false;
                    list.Add(genre);
                }
                var listRemoved = genres.Select(x => x.Id).ToList();
                if (list.Count != 0)
                {
                    await UpdateListAsync(list);
                }
                await EndTransactionAsync();
                return JsonUtil.Success(listRemoved);
            }
            catch (Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.APIServerError, ex.Message);
            }
        }
    }
}
