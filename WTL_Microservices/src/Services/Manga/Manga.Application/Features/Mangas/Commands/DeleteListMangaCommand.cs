using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.Application.Features.Mangas.Commands
{
    public class DeleteListMangaCommand(string ids) : IRequest<IActionResult>
    {
        public string Ids { get; private set; } = ids;
    }

    public class DeleteListMangaCommandHandler : IRequestHandler<DeleteListMangaCommand, IActionResult>
    {
        private readonly IMangaRepository _mangaRepository;
        private readonly ILogger _logger;

        public DeleteListMangaCommandHandler(IMangaRepository mangaRepository, ILogger logger)
        {
            _mangaRepository = mangaRepository;
            _logger = logger;
        }

        private const string MethodName = "DeleteListMangaCommandHandler";
        public async Task<IActionResult> Handle(DeleteListMangaCommand query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName} - Id: {query.Ids}");
            var manga = await _mangaRepository.RemoveSoftListManga(query.Ids);
            _logger.Information($"END: {MethodName} - Id: {query.Ids}");
            return manga;
        }
    }
}
