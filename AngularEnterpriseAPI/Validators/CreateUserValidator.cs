using AngularEnterpriseAPI.DTOs.User;
using FluentValidation;

namespace AngularEnterpriseAPI.Validators
{
    public class CreateUserValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters")
                .MaximumLength(50).WithMessage("Username must not exceed 50 characters")
                .Matches("^[a-zA-Z0-9_]*$").WithMessage("Username can only contain letters, numbers and underscore");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MinimumLength(2).WithMessage("First name must be at least 2 characters")
                .MaximumLength(50).WithMessage("First name must not exceed 50 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MinimumLength(2).WithMessage("Last name must be at least 2 characters")
                .MaximumLength(50).WithMessage("Last name must not exceed 50 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .MaximumLength(20).WithMessage("Password must not exceed 20 characters")
                .Matches(@"[A-Z]+").WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"[a-z]+").WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"[0-9]+").WithMessage("Password must contain at least one number");

            RuleFor(x => x.Role)
                .IsInEnum().WithMessage("Invalid role specified");
        }
    }
}
