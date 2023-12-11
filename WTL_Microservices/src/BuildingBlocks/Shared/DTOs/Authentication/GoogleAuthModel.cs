using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs.Authentication
{
    public class GoogleAuthModel
    {
        public string Provider { get; set; }
        public string TokenId { get; set; }
        public string UserId { get; set; }
        public int RoleId { get; set; }
    }
}
