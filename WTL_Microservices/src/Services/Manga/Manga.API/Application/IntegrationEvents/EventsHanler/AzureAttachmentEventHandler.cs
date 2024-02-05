using EventBus.Messages.IntegrationEvents.Events;
using Manga.Application.Features.Chapters.Commands;
using MassTransit;
using MediatR;
using Shared.DTOs.ChapterImage;
using ILogger = Serilog.ILogger;

namespace Manga.API.Application.IntegrationEvents.EventsHanler
{
    public class AzureAttachmentEventHandler : IConsumer<AzureAttachmentEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public AzureAttachmentEventHandler(IMediator mediator, ILogger logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<AzureAttachmentEvent> context)
        {
            //var command = _mapper.Map<CreateChapterCommand>(context.Message);
            var contextMessage = context.Message;
            var imageList = new List<ChapterImageDto>
            {
                new ChapterImageDto { FilePath = contextMessage.FilePath, Name = contextMessage.FileName, MimeType = contextMessage.ContentType }
            };
            var command = new CreateChapterCommand()
            {
                ImageList = imageList
            };
            var result = await _mediator.Send(command);
            _logger.Information("AzureAttachmentEvent consumed successfully. ", result);
        }
    }
}
