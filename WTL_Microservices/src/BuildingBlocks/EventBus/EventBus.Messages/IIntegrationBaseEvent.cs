using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Messages
{
    public interface IIntegrationBaseEvent<TKey>
    {
        TKey Id { get; set; }
        bool IsEnabled { get; set; }
    }
}
