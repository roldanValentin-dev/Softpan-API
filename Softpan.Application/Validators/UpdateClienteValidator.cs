

using FluentValidation;
using Softpan.Application.DTOs;

namespace Softpan.Application.Validators;

public class UpdateClienteValidator: AbstractValidator<UpdateClienteDto>
{
    public UpdateClienteValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("El ID debe ser mayor a 0");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Telefono)
            .MaximumLength(20).WithMessage("El teléfono no puede exceder 20 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Telefono));

        RuleFor(x => x.Direccion)
            .MaximumLength(200).WithMessage("La dirección no puede exceder 200 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Direccion));

        RuleFor(x => x.TipoCliente)
            .IsInEnum().WithMessage("Tipo de cliente inválido");
    }
}
