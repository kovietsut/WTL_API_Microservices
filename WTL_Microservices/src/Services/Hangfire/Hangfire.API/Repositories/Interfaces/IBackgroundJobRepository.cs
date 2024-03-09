namespace Hangfire.API.Repositories.Interfaces
{
    public interface IBackgroundJobRepository
    {
        string SendEmailContent(string email, string subject, string emailContent, DateTimeOffset enqueueAt);
    }
}
