using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class CreateBlogPostValidator : AbstractValidator<CreateBlogPostCommand>
{
    public CreateBlogPostValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Content)
            .NotEmpty();

        RuleFor(x => x.Excerpt)
            .MaximumLength(500);

        RuleFor(x => x.Category)
            .MaximumLength(100);

        RuleFor(x => x.AuthorName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.FeaturedImage)
            .MaximumLength(500);
    }
}
