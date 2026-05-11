using FluentValidation;
using Softpan.Application.DTOs;

namespace Softpan.Application.Validators;

public class UpdateClienteOnlineDtoValidator : AbstractValidator<UpdateClienteOnlineDto>
{
    public UpdateClienteOnlineDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(50).WithMessage("El nombre no puede exceder 50 caracteres");

        RuleFor(x => x.Apellido)
            .NotEmpty().WithMessage("El apellido es requerido")
            .MaximumLength(50).WithMessage("El apellido no puede exceder 50 caracteres");

        RuleFor(x => x.Telefono)
            .Matches(@"^\+?[0-9\s\-()]{8,20}$")
            .When(x => !string.IsNullOrEmpty(x.Telefono))
            .WithMessage("Formato de teléfono inválido");

        RuleFor(x => x.Direccion)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Direccion))
            .WithMessage("La dirección no puede exceder 200 caracteres");
    }
}
