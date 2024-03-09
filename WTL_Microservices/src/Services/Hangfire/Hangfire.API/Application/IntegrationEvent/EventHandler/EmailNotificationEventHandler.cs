using EventBus.Messages.IntegrationEvents.Events;
using Hangfire.API.Application.Workers;
using MassTransit;
using Shared.Common.Interfaces;
using ILogger = Serilog.ILogger;

namespace Hangfire.API.Application.IntegrationEvent.EventHandler
{
    public class EmailNotificationEventHandler : IConsumer<EmailEvent>
    {
        private readonly ILogger _logger;
        private readonly IScheduledJobService _jobService;

        public EmailNotificationEventHandler(ILogger logger, IScheduledJobService jobService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jobService = jobService;
        }

        public async Task Consume(ConsumeContext<EmailEvent> context)
        {
            var subject = "Notification: New Chapter Released!";
            var body = "We are thrilled to announce that a new chapter released!";
            var contextMessage = context.Message;
            _logger.Information("EmailNotificationEventHandler consumed successfully. ", contextMessage);
            foreach (var recipient in contextMessage.Emails)
            {
                _jobService.Enqueue<EmailSendingWorker>(worker => worker.SendEmail(recipient, subject, body));
            }
        }
    }
}
