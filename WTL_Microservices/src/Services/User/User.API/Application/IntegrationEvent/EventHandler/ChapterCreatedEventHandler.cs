using EventBus.Messages.IntegrationEvents.Events;
using MassTransit;
using User.API.Repositories.Interfaces;
using ILogger = Serilog.ILogger;

namespace User.API.Application.IntegrationEvent.EventHandler
{
    public class ChapterCreatedEventHandler: IConsumer<ChapterCreatedEvent>
    {
        private readonly ILogger _logger;
        private readonly IUserRepository _userRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public ChapterCreatedEventHandler(ILogger logger, IUserRepository userRepository, IPublishEndpoint publishEndpoint)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(logger));
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<ChapterCreatedEvent> context)
        {
            var contextMessage = context.Message;
            var emails = _userRepository.GetListEmail(contextMessage.ListUser);
            _logger.Information("ChapterCreatedEvent consumed successfully. ", emails);
        }
    }
}
