using FluentValidation;
using Softpan.Application.DTOs;

namespace Softpan.Application.Validators;

public class CreateProductoImagenValidator : AbstractValidator<CreateProductoImagenDto>
{
    public CreateProductoImagenValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("La URL de la imagen es requerida")
            .MaximumLength(500).WithMessage("La URL no puede exceder 500 caracteres");

        RuleFor(x => x.Orden)
            .GreaterThanOrEqualTo(0).WithMessage("El orden no puede ser negativo");
    }
}

public class UpdateProductoImagenValidator : AbstractValidator<UpdateProductoImagenDto>
{
    public UpdateProductoImagenValidator()
    {
        RuleFor(x => x.Orden)
            .GreaterThanOrEqualTo(0).WithMessage("El orden no puede ser negativo");
    }
}
