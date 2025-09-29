namespace AsyncScapeIA.Application.Validation;

using AsyncScapeIA.Domain;
using FluentValidation;

public sealed class ArchitectureComponentValidator : AbstractValidator<ArchitectureComponent>
{
    public ArchitectureComponentValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Domain).NotEmpty();
        RuleFor(x => x.Slug).Matches("^[a-z0-9]+(-[a-z0-9]+)*$");
    }
}

