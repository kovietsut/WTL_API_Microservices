using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs.Encryption
{
    public class AppSetting
    {
        public AppSetting()
        {
        }

        public string Name { get; set; }
        public string Version { get; set; }
        public string Salt { get; set; }
    }
}
