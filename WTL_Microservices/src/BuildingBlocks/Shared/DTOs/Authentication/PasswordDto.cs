using FluentValidation;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs.Authentication
{
    public class PasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }
    }
    public class PasswordValidator : AbstractValidator<PasswordDto>
    {
        public PasswordValidator()
        {
            RuleFor(x => x.CurrentPassword).NotNull().NotEmpty().WithMessage("Current password cannot be empty");
            RuleFor(x => x.NewPassword).NotNull().NotEmpty().WithMessage("New password cannot be empty")
                .MinimumLength(8).WithMessage("Password length at least 8");
            RuleFor(x => x.ConfirmPassword).NotNull().NotEmpty().WithMessage("Confirm password password cannot be empty")
                .Equal(x => x.NewPassword).WithMessage("New password does not match new password")
                .MinimumLength(8).WithMessage("Password length at least 8");
        }
    }
}
