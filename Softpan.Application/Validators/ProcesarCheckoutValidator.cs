using FluentValidation;
using Softpan.Application.DTOs;
using Softpan.Domain.Enums;

namespace Softpan.Application.Validators;

public class ProcesarCheckoutValidator : AbstractValidator<ProcesarCheckoutDto>
{
    public ProcesarCheckoutValidator()
    {
        RuleFor(x => x.FechaEntrega)
            .NotEmpty().WithMessage("La fecha de entrega es requerida")
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("La fecha de entrega no puede ser anterior a hoy");

        RuleFor(x => x.Observaciones)
            .MaximumLength(500).WithMessage("Las observaciones no pueden exceder 500 caracteres");

        RuleFor(x => x.TipoPago)
            .IsInEnum().WithMessage("Tipo de pago inválido")
            .When(x => x.TipoPago.HasValue);
    }
}
