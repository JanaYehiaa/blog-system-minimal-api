using FluentValidation;
public class PostUpdateDTOValidator : AbstractValidator<PostUpdateDTO>
{
    public PostUpdateDTOValidator()
    {
        When(postUpdateDTO => !string.IsNullOrWhiteSpace(postUpdateDTO.Title), () =>
        {
            RuleFor(postUpdateDTO => postUpdateDTO.Title)
                .MaximumLength(40)
                .WithMessage("Title must be under 40 characters.");
        });

        When(postUpdateDTO => !string.IsNullOrWhiteSpace(postUpdateDTO.Description), () =>
        {
            RuleFor(postUpdateDTO => postUpdateDTO.Description)
                .Length(1, 120)
                .WithMessage("Description must be between 1 and 120 characters.");
        });

        When(postUpdateDTO => !string.IsNullOrWhiteSpace(postUpdateDTO.Body), () =>
        {
            RuleFor(postUpdateDTO => postUpdateDTO.Body)
                .MinimumLength(50)
                .WithMessage("Body must be at least 50 characters.");
        });

        When(postUpdateDTO => postUpdateDTO.Status.HasValue, () =>
        {
            RuleFor(postUpdateDTO => postUpdateDTO.Status)
                .IsInEnum()
                .WithMessage("Invalid post status.");
        });

        When(postUpdateDTO => postUpdateDTO.Metadata?.Tags != null && postUpdateDTO.Metadata.Tags.Any(), () =>
        {
            RuleForEach(postUpdateDTO => postUpdateDTO.Metadata!.Tags)
                .MaximumLength(30)
                .WithMessage("Each tag must be at most 30 characters.");

            RuleFor(postUpdateDTO => postUpdateDTO.Metadata!.Tags)
                .Must(tags => tags.Distinct().Count() == tags.Count)
                .WithMessage("Duplicate tags are not allowed.");
        });

        When(postUpdateDTO => postUpdateDTO.Metadata?.Categories != null && postUpdateDTO.Metadata.Categories.Any(), () =>
        {
            RuleForEach(postUpdateDTO => postUpdateDTO.Metadata!.Categories)
                .MaximumLength(30)
                .WithMessage("Each category must be at most 30 characters.");
        });

        When(postUpdateDTO => postUpdateDTO.Assets != null, () =>
        {
            RuleFor(postUpdateDTO => postUpdateDTO.Assets)
                .Must(list => list == null || list.Count <= 5)
                .WithMessage("A maximum of 5 assets are allowed.");

            RuleForEach(postUpdateDTO => postUpdateDTO.Assets)
                .NotEmpty()
                .WithMessage("Asset paths cannot be empty.");
        });
    }
}
