using FluentValidation;
using Softpan.Application.DTOs;
using Softpan.Domain.Interfaces;

namespace Softpan.Application.Validators;

public class CreateVentaValidator : AbstractValidator<CreateVentaDto>
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IProductoRepository _productoRepository;

    public CreateVentaValidator(IClienteRepository clienteRepository, IProductoRepository productoRepository)
    {
        _clienteRepository = clienteRepository;
        _productoRepository = productoRepository;

        RuleFor(x => x.ClienteId)
            .GreaterThan(0)
            .WithMessage("El ClienteId debe ser mayor a 0")
            .MustAsync(ClienteExiste)
            .WithMessage("El cliente no existe");

        RuleFor(x => x.Detalles)
            .NotEmpty()
            .WithMessage("La venta debe tener al menos un detalle");

        RuleForEach(x => x.Detalles)
            .SetValidator(new CreateDetalleVentaValidator(_productoRepository));
    }

    private async Task<bool> ClienteExiste(int clienteId, CancellationToken cancellationToken)
    {
        return await _clienteRepository.ExistsAsync(clienteId);
    }
}

public class CreateDetalleVentaValidator : AbstractValidator<CreateDetalleVentaDto>
{
    private readonly IProductoRepository _productoRepository;

    public CreateDetalleVentaValidator(IProductoRepository productoRepository)
    {
        _productoRepository = productoRepository;

        RuleFor(x => x.ProductoId)
            .GreaterThan(0)
            .WithMessage("El ProductoId debe ser mayor a 0")
            .MustAsync(ProductoExiste)
            .WithMessage("El producto no existe");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0)
            .WithMessage("La cantidad debe ser mayor a 0");

        RuleFor(x => x.PrecioUnitario)
            .GreaterThan(0)
            .WithMessage("El precio unitario debe ser mayor a 0");
    }

    private async Task<bool> ProductoExiste(int productoId, CancellationToken cancellationToken)
    {
        return await _productoRepository.ExistsAsync(productoId);
    }
}