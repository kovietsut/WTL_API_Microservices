using AutoMapper;
using EventBus.Messages.IntegrationEvents.Events;
using Manga.Application.Features.Chapters.Commands;
using MassTransit;
using MediatR;
using ILogger = Serilog.ILogger;

namespace Manga.API.Application.IntegrationEvents.EventsHanler
{
    public class AzureAttachmentEventHandler : IConsumer<AzureAttachmentEvent>
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public AzureAttachmentEventHandler(IMediator mediator, IMapper mapper, ILogger logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<AzureAttachmentEvent> context)
        {
            var command = _mapper.Map<CreateChapterCommand>(context.Message);
            var result = await _mediator.Send(command);
            _logger.Information("AzureAttachmentEvent consumed successfully. ", result);
        }
    }
}
