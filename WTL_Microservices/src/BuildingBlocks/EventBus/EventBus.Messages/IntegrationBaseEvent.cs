namespace EventBus.Messages
{
    public class IntegrationBaseEvent<TKey> : IIntegrationBaseEvent<TKey>
    {
        public TKey Id { get; set; }
        public bool IsEnabled { get; set; }
    }
}
