using FluentValidation;
using Softpan.Application.DTOs;
using Softpan.Domain.Interfaces;

namespace Softpan.Application.Validators;

public class CreatePagoValidator : AbstractValidator<CreatePagoDto>
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IVentaRepository _ventaRepository;

    public CreatePagoValidator(IClienteRepository clienteRepository, IVentaRepository ventaRepository)
    {
        _clienteRepository = clienteRepository;
        _ventaRepository = ventaRepository;

        RuleFor(x => x.ClienteId)
            .GreaterThan(0)
            .WithMessage("El ClienteId debe ser mayor a 0")
            .MustAsync(ClienteExiste)
            .WithMessage("El cliente no existe");

        RuleFor(x => x.Monto)
            .GreaterThan(0)
            .WithMessage("El monto debe ser mayor a 0");

        RuleFor(x => x.TipoPago)
            .IsInEnum()
            .WithMessage("El tipo de pago no es válido");

        RuleFor(x => x.Observaciones)
            .MaximumLength(500)
            .WithMessage("Las observaciones no pueden exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Observaciones));

        RuleFor(x => x.VentasAAplicar)
            .NotEmpty()
            .WithMessage("Debe especificar al menos una venta para aplicar el pago");

        RuleForEach(x => x.VentasAAplicar)
            .SetValidator(new AplicarPagoVentaValidator(_ventaRepository));

        RuleFor(x => x)
            .MustAsync(MontoTotalCoincide)
            .WithMessage("La suma de los montos aplicados debe ser igual al monto del pago");
    }

    private async Task<bool> ClienteExiste(int clienteId, CancellationToken cancellationToken)
    {
        return await _clienteRepository.ExistsAsync(clienteId);
    }

    private async Task<bool> MontoTotalCoincide(CreatePagoDto dto, CancellationToken cancellationToken)
    {
        var sumaAplicada = dto.VentasAAplicar.Sum(v => v.MontoAplicado);
        return sumaAplicada == dto.Monto;
    }
}

public class AplicarPagoVentaValidator : AbstractValidator<AplicarPagoVentaDto>
{
    private readonly IVentaRepository _ventaRepository;

    public AplicarPagoVentaValidator(IVentaRepository ventaRepository)
    {
        _ventaRepository = ventaRepository;

        RuleFor(x => x.VentaId)
            .GreaterThan(0)
            .WithMessage("El VentaId debe ser mayor a 0")
            .MustAsync(VentaExiste)
            .WithMessage("La venta no existe");

        RuleFor(x => x.MontoAplicado)
            .GreaterThan(0)
            .WithMessage("El monto aplicado debe ser mayor a 0");
    }

    private async Task<bool> VentaExiste(int ventaId, CancellationToken cancellationToken)
    {
        return await _ventaRepository.ExistsAsync(ventaId);
    }
}
