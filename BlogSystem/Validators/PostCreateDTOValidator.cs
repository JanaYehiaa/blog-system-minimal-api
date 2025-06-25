using FluentValidation;

public class PostCreateDTOValidator : AbstractValidator<PostCreateDTO>
{
    public PostCreateDTOValidator()
    {
        RuleFor(PostCreateDTO => PostCreateDTO.Title)
        .NotEmpty()
        .WithMessage("Title can't be empty");

        RuleFor(PostCreateDTO => PostCreateDTO.Description)
        .NotEmpty().WithMessage("Description cannot be empty.")
        .Length(1, 120).WithMessage("Description must be between 1 and 120 characters.");

        RuleFor(PostCreateDTO => PostCreateDTO.Body)
        .NotEmpty()
        .MinimumLength(50)
        .WithMessage("Body must be at least 50 characters.");

        RuleFor(x => x.CustomUrl)
            .NotEmpty()
            .Length(3, 100)
            .Matches("^[a-z0-9]+(?:[-_][a-z0-9]+)*$")
            .WithMessage("Custom URL must be lowercase, alphanumeric, and may include dashes or underscores.");


        RuleForEach(PostCreateDTO => PostCreateDTO.Metadata.Tags)
            .MaximumLength(30)
            .NotEmpty();

        RuleFor(PostCreateDTO => PostCreateDTO.Metadata.Tags)
            .Must(tags => tags.Distinct().Count() == tags.Count)
            .WithMessage("Duplicate tags are not allowed.");

        RuleForEach(PostCreateDTO => PostCreateDTO.Metadata.Categories)
            .MaximumLength(30)
            .NotEmpty();

        RuleFor(PostCreateDTO => PostCreateDTO.Assets)
            .Must(list => list == null || list.Count <= 10)
            .WithMessage("A maximum of 10 assets are allowed.");

        RuleForEach(PostCreateDTO => PostCreateDTO.Assets)
            .NotEmpty().WithMessage("Asset paths cannot be empty.")
            .When(PostCreateDTO => PostCreateDTO.Assets != null);

    }
}