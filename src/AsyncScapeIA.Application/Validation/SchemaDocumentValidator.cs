namespace AsyncScapeIA.Application.Validation;

using AsyncScapeIA.Domain;
using FluentValidation;

public sealed class SchemaDocumentValidator : AbstractValidator<SchemaDocument>
{
    public SchemaDocumentValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Version).NotEmpty();
        RuleFor(x => x.Checksum).NotEmpty();
    }
}

