using EventBus.Messages.IntegrationEvents.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Messages.IntegrationEvents.Events
{
    public class AzureAttachmentEvent: IAzureAttachmentEvent
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}
