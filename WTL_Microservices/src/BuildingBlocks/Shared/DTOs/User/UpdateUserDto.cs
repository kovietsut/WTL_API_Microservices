using FluentValidation;
using Shared.Common;
using Shared.SeedWork;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.User
{
    public class UpdateUserDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public long RoleId { get; set; }
        public string? FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Gender { get; set; }
    }

    public class UpdateUserValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserValidator()
        {
            RuleFor(x => x.RoleId).NotEmpty().WithMessage("RoleId is required")
                .Must((roleId) => CheckValidationHelper.IsIntOrLong(roleId));
            RuleFor(x => x.Email).NotNull().WithMessage("Email is required")
                .NotEmpty().WithMessage("Email not empty")
                .EmailAddress().WithMessage("Invalid email address");
            RuleFor(x => x.PhoneNumber).NotNull().WithMessage("Phone number is required")
                .NotEmpty().WithMessage("Phone number cannot be empty")
                .Matches(RegexHelper.PhoneNumberRegexVietNam).WithMessage("Invalid phone number format");
            RuleFor(x => x.FullName).Must((fullName) => CheckValidationHelper.IsNullOrDefault(fullName))
                .WithMessage("FullName cannot be empty");
            RuleFor(x => x.Address).Must((address) => CheckValidationHelper.IsNullOrDefault(address))
                .WithMessage("Address cannot be empty");
            RuleFor(x => x.Gender).Must((gender) => CheckValidationHelper.IsNullOrDefault(gender))
                .WithMessage("Gender cannot be empty");
        }
    }
}
