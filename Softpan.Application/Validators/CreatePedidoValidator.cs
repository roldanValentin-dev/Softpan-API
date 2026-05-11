using FluentValidation;
using Softpan.Application.DTOs;

namespace Softpan.Application.Validators;

public class CreatePedidoValidator : AbstractValidator<CreatePedidoDto>
{
    public CreatePedidoValidator()
    {
        RuleFor(x => x.FechaEntrega)
            .NotEmpty().WithMessage("La fecha de entrega es requerida")
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("La fecha de entrega no puede ser anterior a hoy");

        RuleFor(x => x.Detalles)
            .NotEmpty().WithMessage("El pedido debe tener al menos un producto")
            .Must(d => d != null && d.Count > 0)
            .WithMessage("El pedido debe tener al menos un producto");

        RuleFor(x => x.Observaciones)
            .MaximumLength(500).WithMessage("Las observaciones no pueden exceder 500 caracteres");

        RuleForEach(x => x.Detalles).ChildRules(detalle =>
        {
            detalle.RuleFor(d => d.ProductoId)
                .GreaterThan(0).WithMessage("ID de producto inválido");

            detalle.RuleFor(d => d.Cantidad)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0")
                .LessThanOrEqualTo(1000).WithMessage("La cantidad no puede exceder 1000 unidades");
        });
    }
}
