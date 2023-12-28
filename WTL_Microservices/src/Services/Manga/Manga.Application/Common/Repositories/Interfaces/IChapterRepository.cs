using Manga.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.Application.Common.Repositories.Interfaces
{
    public interface IChapterRepository
    {
        Task<Chapter> GetChapterById(long chapterId);
    }
}
