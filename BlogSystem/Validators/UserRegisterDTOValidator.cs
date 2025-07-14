using FluentValidation;

public class UserRegisterDTOValidator : AbstractValidator<UserRegisterDTO>
{
    public UserRegisterDTOValidator()
    {
        RuleFor(UserRegisterDTO => UserRegisterDTO.Username)
        .NotEmpty().WithMessage("Username can't be empty")
        .Length(6, 20).WithMessage("Username must be between 6 and 20 characters")
        .Matches("^[a-z0-9]+([._-]?[a-z0-9]+)*$")
        .WithMessage("Usernames must be lowercase alphanumeric and may include dots, dashes, or underscores (not starting or ending with them, and no spaces)");

        RuleFor(UserRegisterDTO => UserRegisterDTO.Email)
        .NotEmpty().WithMessage("Email can't be empty")
        .EmailAddress().WithMessage("Invalid email format");

        RuleFor(UserRegisterDTO => UserRegisterDTO.Password)
        .NotEmpty().WithMessage("Password can't be empty");
        //.MinimumLength(12).WithMessage("Passwords must be at least 12 characters")
        //.Matches(@"\d").WithMessage("Password must contain at least one number")
        //.Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
        //.Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
        //.Matches(@"[\W_]").WithMessage("Password must contain at least one special character");

        RuleFor(UserRegisterDTO => UserRegisterDTO.Role)
        .IsInEnum().WithMessage("Invalid role selected");
    }
}