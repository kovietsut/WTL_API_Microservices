using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Authentication
{
    public class SignInDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
