using FluentValidation;
using Shared.Dtos.UserPermissions;

namespace Shared.Validators.UserPermissions
{
    /// <summary>
    /// Validator for CreateSignUpKeyRequest
    /// </summary>
    public class CreateSignUpKeyValidator : AbstractValidator<CreateSignUpKeyRequest>
    {
        public CreateSignUpKeyValidator()
        {
            RuleFor(x => x.Key)
                .MaximumLength(255)
                .WithMessage("Key must not exceed 255 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Key));

            RuleFor(x => x.ExpiresAt)
                .NotEmpty()
                .WithMessage("Expiration date is required")
                .Must(BeAtLeastOneHourInFuture)
                .WithMessage("Expiration date must be at least 1 hour in the future")
                .Must(BeNoMoreThanFiveDaysInFuture)
                .WithMessage("Expiration date must not be more than 5 days in the future");
        }

        private bool BeAtLeastOneHourInFuture(DateTime expiresAt)
        {
            var now = DateTime.UtcNow;
            var oneHourFromNow = now.AddHours(1);
            return expiresAt >= oneHourFromNow;
        }

        private bool BeNoMoreThanFiveDaysInFuture(DateTime expiresAt)
        {
            var now = DateTime.UtcNow;
            var fiveDaysFromNow = now.AddDays(5);
            return expiresAt <= fiveDaysFromNow;
        }
    }

    /// <summary>
    /// Validator for UpdateSignUpKeyRequest
    /// </summary>
    public class UpdateSignUpKeyValidator : AbstractValidator<UpdateSignUpKeyRequest>
    {
        public UpdateSignUpKeyValidator()
        {
            RuleFor(x => x.Key)
                .MaximumLength(255)
                .WithMessage("Key must not exceed 255 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Key));

            RuleFor(x => x.ExpiresAt)
                .Must(BeAtLeastOneHourInFuture)
                .WithMessage("Expiration date must be at least 1 hour in the future")
                .When(x => x.ExpiresAt.HasValue)
                .Must(BeNoMoreThanFiveDaysInFuture)
                .WithMessage("Expiration date must not be more than 5 days in the future")
                .When(x => x.ExpiresAt.HasValue);
        }

        private bool BeAtLeastOneHourInFuture(DateTime? expiresAt)
        {
            if (!expiresAt.HasValue)
                return true;

            var now = DateTime.UtcNow;
            var oneHourFromNow = now.AddHours(1);
            return expiresAt.Value >= oneHourFromNow;
        }

        private bool BeNoMoreThanFiveDaysInFuture(DateTime? expiresAt)
        {
            if (!expiresAt.HasValue)
                return true;

            var now = DateTime.UtcNow;
            var fiveDaysFromNow = now.AddDays(5);
            return expiresAt.Value <= fiveDaysFromNow;
        }
    }
}
