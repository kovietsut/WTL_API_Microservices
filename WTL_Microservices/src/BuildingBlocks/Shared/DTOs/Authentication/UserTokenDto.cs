using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs.Authentication
{
    public class UserTokenDto
    {
        [Required]
        public long UserId { get; set; }
        [Required]
        public string Email { get; set; }
    }
}
