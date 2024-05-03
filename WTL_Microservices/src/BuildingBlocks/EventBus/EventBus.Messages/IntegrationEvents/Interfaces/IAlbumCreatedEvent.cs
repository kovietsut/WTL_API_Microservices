using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Messages.IntegrationEvents.Interfaces
{
    public interface IAlbumCreatedEvent
    {
        long Id { get; set; }
        long? UserId { get; set; }
    }
}
