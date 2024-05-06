using FluentValidation;
using Shared.SeedWork;

namespace Shared.DTOs.Permission
{
    public class UpdatePermissionDto
    {
        public long? ActionId { get; set; }
    }

    public class UpdatePermissionValidator : AbstractValidator<UpdatePermissionDto>
    {
        public UpdatePermissionValidator()
        {
            RuleFor(x => x.ActionId).NotEmpty().WithMessage("ActionId is required")
                .Must((actionId) => CheckValidationHelper.IsIntOrLong(actionId));
        }
    }
}
