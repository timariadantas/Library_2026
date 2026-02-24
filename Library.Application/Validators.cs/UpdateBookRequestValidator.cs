using FluentValidation;
using Library.Application.DTO;

namespace Library.Application.Validators;

public class UpdateBookRequestValidator : AbstractValidator<UpdateBookRequest>
{
    public UpdateBookRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Título é obrigatório.")
            .MaximumLength(150);

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Autor é obrigatório.")
            .MaximumLength(100);

        RuleFor(x => x.ReleaseYear)
            .InclusiveBetween(1500, DateTime.Now.Year)
            .WithMessage($"Ano deve estar entre 1500 e {DateTime.Now.Year}.");

        RuleFor(x => x.PageLength)
            .GreaterThan(0)
            .When(x => x.PageLength.HasValue);

        RuleFor(x => x.Publisher)
            .MaximumLength(120)
            .When(x => !string.IsNullOrEmpty(x.Publisher));

        RuleFor(x => x.Genres)
            .NotEmpty().WithMessage("Pelo menos um gênero deve ser informado.");

        RuleForEach(x => x.Genres)
            .NotEmpty().WithMessage("Gênero não pode ser vazio.");
    }
}
