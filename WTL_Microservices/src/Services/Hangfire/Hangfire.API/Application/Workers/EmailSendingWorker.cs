using Hangfire.API.Repositories.Interfaces;
using ILogger = Serilog.ILogger;

namespace Hangfire.API.Application.Workers
{
    public class EmailSendingWorker
    {
        private readonly ILogger _logger;
        private readonly IBackgroundJobRepository _backgroundRepository;

        public EmailSendingWorker(IBackgroundJobRepository backgroundRepository, ILogger logger)
        {
            _backgroundRepository = backgroundRepository;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public void SendEmail(string recipient, string subject, string body)
        {
            try
            {
                _backgroundRepository.SendEmailContent(recipient, subject, body, DateTimeOffset.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.Information($"Error sending email to {recipient}: {ex.Message}");
            }
        }
    }
}
