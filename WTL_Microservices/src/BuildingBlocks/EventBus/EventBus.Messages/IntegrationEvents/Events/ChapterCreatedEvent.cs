using EventBus.Messages.IntegrationEvents.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Messages.IntegrationEvents.Events
{
    public class ChapterCreatedEvent: IChapterCreatedEvent
    {
        public long Id { get; set; }
        public long? UserId { get; set; }
        public string? ChapterName { get; set; }
        public DateTimeOffset? PublishDate { get; set; }
        public List<long?> ListUser { get; set; }
    }
}
