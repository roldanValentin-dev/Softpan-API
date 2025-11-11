using Mapster;
using Softpan.Application.DTOs;
using Softpan.Application.Interfaces;
using Softpan.Domain.Entities;
using Softpan.Domain.Enums;
using Softpan.Domain.Interfaces;

namespace Softpan.Application.Services;

public class PagoService(IPagoRepository pagoRepository, IVentaRepository ventaRepository) : IPagoService
{
    public async Task<PagoDto?> GetPagoByIdAsync(int id)
    {
        var pago = await pagoRepository.GetByIdAsync(id);
        if (pago == null)
        {
            return null;
        }
        return MapToDto(pago);
    }

    public async Task<PagoDetalleDto?> GetPagoDetalleByIdAsync(int id)
    {
        var pago = await pagoRepository.GetByIdAsync(id);
        if (pago == null)
        {
            return null;
        }
        return pago.Adapt<PagoDetalleDto>();
    }

    public async Task<IEnumerable<PagoDto>> GetAllPagosAsync()
    {
        var pagos = await pagoRepository.GetAllAsync();
        return pagos.Select(MapToDto);
    }

    public async Task<IEnumerable<PagoDto>> GetPagosByClienteAsync(int clienteId)
    {
        var pagos = await pagoRepository.GetPagosByClienteAsync(clienteId);
        return pagos.Select(MapToDto);
    }

    public async Task<IEnumerable<PagoDto>> GetPagosByTipoAsync(TipoPagoEnum tipo)
    {
        var pagos = await pagoRepository.GetPagosByTipoAsync(tipo);
        return pagos.Select(MapToDto);
    }

    public async Task<IEnumerable<PagoDto>> GetPagosByFechaAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var pagos = await pagoRepository.GetPagosByFechaAsync(fechaInicio, fechaFin);
        return pagos.Select(MapToDto);
    }

    public async Task<PagoDto> CreatePagoAsync(CreatePagoDto dto)
    {
        var pago = dto.Adapt<Pago>();

        // Crear el pago
        var createdPago = await pagoRepository.CreateAsync(pago);

        // Aplicar el pago a las ventas especificadas
        foreach (var ventaAplicar in dto.VentasAAplicar)
        {
            var venta = await ventaRepository.GetByIdAsync(ventaAplicar.VentaId);
            if (venta != null)
            {
                // Crear la relación PagoVenta
                var pagoVenta = new PagoVenta
                {
                    PagoId = createdPago.Id,
                    VentaId = ventaAplicar.VentaId,
                    MontoAplicado = ventaAplicar.MontoAplicado
                };

                venta.PagosVenta.Add(pagoVenta);

                // Actualizar el monto pagado y estado de la venta
                venta.ActualizarMontoPagado();
                venta.ActualizarEstado();

                await ventaRepository.UpdateAsync(venta);
            }
        }

        // Re-consultar para obtener las relaciones
        return (await pagoRepository.GetByIdAsync(createdPago.Id))!.Adapt<PagoDto>();
    }

    public async Task<bool> DeletePagoAsync(int id)
    {
        return await pagoRepository.DeleteAsync(id);
    }

    private static PagoDto MapToDto(Pago pago) => pago.Adapt<PagoDto>();
}
