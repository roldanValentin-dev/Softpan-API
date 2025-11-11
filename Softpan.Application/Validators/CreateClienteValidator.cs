
using FluentValidation;
using Softpan.Application.DTOs;

namespace Softpan.Application.Validators;

public class CreateClienteValidator : AbstractValidator<CreateClienteDto>
{
    public CreateClienteValidator()
    {
        RuleFor(c => c.Nombre)
            .NotEmpty().WithMessage("El nombre del cliente es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres");

        RuleFor(c => c.Telefono)
            .MaximumLength(20).WithMessage("El numero del telefono no puede exceder los 20 caracteres")
            .When(c => !string.IsNullOrEmpty(c.Telefono));

        RuleFor(c => c.Direccion)
            .MaximumLength(200).WithMessage("La direccion no puede exceder los 200 caracteres")
            .When(c => !string.IsNullOrEmpty(c.Direccion));

        RuleFor(x => x.TipoCliente)
            .IsInEnum().WithMessage("Tipo de cliente inválido");

    }
}
