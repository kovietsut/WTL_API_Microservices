using AccessControl.API.Repositories;
using AccessControl.API.Repositories.Interfaces;
using EventBus.Messages.IntegrationEvents.Events;
using MassTransit;
using Shared.DTOs.Album;
using Shared.DTOs.Permission;
using ILogger = Serilog.ILogger;

namespace AccessControl.API.Application.IntegrationEvents.EventsHandler
{
    public class AlbumCreatedEventHandler : IConsumer<AlbumCreatedEvent>
    {
        private readonly ILogger _logger;
        private readonly IAccessControlRepository _accessControlRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public AlbumCreatedEventHandler(ILogger logger, IAccessControlRepository accessControlRepository, IPublishEndpoint publishEndpoint)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accessControlRepository = accessControlRepository ?? throw new ArgumentNullException(nameof(logger));
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<AlbumCreatedEvent> context)
        {
            var contextMessage = context.Message;
            var model = new GrantPermissionDto() {
                ActionId = 2, //write
                AlbumId = contextMessage.Id,
                UserId = (long)contextMessage.UserId,
                MangaId = null,
                Type = "Grant"
            };
            var result = _accessControlRepository.GrantPermission(model);
            _logger.Information("AlbumCreatedEvent consumed successfully. ", result);
            
        }
    }
}
