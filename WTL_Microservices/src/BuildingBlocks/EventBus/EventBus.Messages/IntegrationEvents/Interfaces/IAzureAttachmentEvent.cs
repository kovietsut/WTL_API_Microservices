using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Messages.IntegrationEvents.Interfaces
{
    public interface IAzureAttachmentEvent
    {
        string FilePath { get; set; }
        string FileName { get; set; }
        string ContentType { get; set; }
    }
}
