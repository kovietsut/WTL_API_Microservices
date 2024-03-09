namespace EventBus.Messages.IntegrationEvents.Interfaces
{
    public interface IEmailEvent
    {
        List<string> Emails { get; set; }
    }
}
