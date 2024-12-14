using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Configurations
{
    public class ApiConfiguration
    {
        public string ApiName { get; set; }

        public string ApiVersion { get; set; }

        public string IdentityServerBaseUrl { get; set; }

        public string IssuerUri { get; set; }

        public string ApiBaseUrl { get; set; }

        public string ClientId { get; set; }
    }
}
