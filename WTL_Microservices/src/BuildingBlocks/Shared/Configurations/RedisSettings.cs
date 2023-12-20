using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Configurations
{
    public class RedisSettings
    {
        public string ConnectionString { get; set; }
        public string DBProvider {  get; set; }
    }
}
