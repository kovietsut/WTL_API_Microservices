
using EventBus.Messages.IntegrationEvents.Interfaces;

namespace EventBus.Messages.IntegrationEvents.Events
{
    public class AlbumCreatedEvent : IAlbumCreatedEvent
    {
        public long Id {  get; set; }
        public long? UserId { get; set; }
    }
}
