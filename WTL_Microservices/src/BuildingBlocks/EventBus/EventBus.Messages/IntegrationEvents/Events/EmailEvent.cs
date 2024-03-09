using EventBus.Messages.IntegrationEvents.Interfaces;

namespace EventBus.Messages.IntegrationEvents.Events
{
    public class EmailEvent: IEmailEvent
    {
        public List<string> Emails { get; set; }
    }
}
