using Contracts.Domains.Interfaces;
using Infrastructure.Common;
using Infrastructure.Common.Repositories;
using Manga.Application.Common.Repositories.Interfaces;
using Manga.Infrastructure.Entities;
using Manga.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.DTOs;
using Shared.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.Application.Common.Repositories
{
    public class ChapterRepository : RepositoryBase<Chapter, long, MangaContext>, IChapterRepository
    {
        private readonly ErrorCode _errorCodes;
        private readonly IMangaRepository _mangaRepository;

        public ChapterRepository(MangaContext dbContext, IUnitOfWork<MangaContext> unitOfWork, IMangaRepository mangaRepository, IOptions<ErrorCode> errorCode) :
            base(dbContext, unitOfWork)
        {
            _errorCodes = errorCode.Value;
            _mangaRepository = mangaRepository;
        }

        public Task<Chapter> GetChapterById(long chapterId) => FindByCondition(x => x.Id == chapterId).SingleOrDefaultAsync();

        //public async Task<IActionResult> Get(int chapterId)
        //{
        //    try
        //    {
        //        if (chapterId == null)
        //        {
        //            return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "ChapterId is empty");
        //        }
        //        var chapter = await GetChapterById(chapterId);
        //        if (chapter == null)
        //        {
        //            return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Chapter does not exist");
        //        }
        //        var manga = await _mangaRepository.GetMangaById(chapter.MangaId);
        //        if (manga.Type.Equals("TruyenTranh"))
        //        {
        //            var list = _unitOfWork.RepositoryCRUD<ChapterImage>().GetAll(x => x.IsEnabled == true && x.ChapterId == chapterId)
        //            .Select(x => new
        //            {
        //                x.IsEnabled,
        //                FilePath = GenerateCoverImageUriWithSas(x.FilePath, _azureBlobService),
        //            });
        //            if (list != null)
        //            {
        //                var totalRecords = list.ToList().Count();
        //                return JsonUtil.Success(list, dataCount: totalRecords);
        //            }
        //            return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Empty List Data");
        //        }
        //        return JsonUtil.Success(chapter.Content);
        //    }
        //    catch (UnauthorizedAccessException e)
        //    {
        //        return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, e.Message);
        //    }
        //}
    }
}
