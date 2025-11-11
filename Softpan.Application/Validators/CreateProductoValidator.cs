using FluentValidation;
using Softpan.Application.DTOs;

namespace Softpan.Application.Validators;

public class CreateProductoValidator : AbstractValidator<CreateProductoDto>
{
    public CreateProductoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty()
            .WithMessage("El nombre es requerido")
            .MaximumLength(60)
            .WithMessage("El nombre no puede exceder 60 caracteres");

        RuleFor(x => x.Descripcion)
            .MaximumLength(100)
            .WithMessage("La descripción no puede exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Descripcion));

        RuleFor(x => x.PrecioBase)
            .GreaterThan(0)
            .WithMessage("El precio base debe ser mayor a 0");
    }
}

public class UpdateProductoValidator : AbstractValidator<UpdateProductoDto>
{
    public UpdateProductoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("El Id debe ser mayor a 0");

        RuleFor(x => x.Nombre)
            .NotEmpty()
            .WithMessage("El nombre es requerido")
            .MaximumLength(60)
            .WithMessage("El nombre no puede exceder 60 caracteres");

        RuleFor(x => x.Descripcion)
            .MaximumLength(100)
            .WithMessage("La descripción no puede exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Descripcion));

        RuleFor(x => x.PrecioBase)
            .GreaterThan(0)
            .WithMessage("El precio base debe ser mayor a 0");
    }
}