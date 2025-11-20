using Mapster;
using Softpan.Application.DTOs;
using Softpan.Application.Interfaces;
using Softpan.Domain.Entities;
using Softpan.Domain.Enums;
using Softpan.Domain.Interfaces;

namespace Softpan.Application.Services;

public class PagoService(
    IPagoRepository pagoRepository,
    IVentaRepository ventaRepository,
    IUnitOfWork unitOfWork) : IPagoService
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
        // TRANSACCIÓN: Inicia una transacción de base de datos
        // Esto garantiza que TODAS las operaciones se ejecuten juntas o NINGUNA
        await unitOfWork.BeginTransactionAsync();

        try
        {
            // PASO 1: Crear el pago
            // Nota: Aunque llamamos a CreateAsync, los cambios NO se guardan en BD aún
            // porque están dentro de una transacción. Se guardan en memoria temporal.
            var pago = dto.Adapt<Pago>();
            var createdPago = await pagoRepository.CreateAsync(pago);

            // PASO 2: Aplicar el pago a cada venta especificada
            // Recorremos todas las ventas a las que se debe aplicar el pago
            foreach (var ventaAplicar in dto.VentasAAplicar)
            {
                // Obtener la venta de la base de datos
                var venta = await ventaRepository.GetByIdAsync(ventaAplicar.VentaId);
                if (venta != null)
                {
                    // PASO 2.1: Crear la relación PagoVenta
                    // Esta tabla intermedia conecta el pago con la venta
                    var pagoVenta = new PagoVenta
                    {
                        PagoId = createdPago.Id,
                        VentaId = ventaAplicar.VentaId,
                        MontoAplicado = ventaAplicar.MontoAplicado
                    };

                    venta.PagosVenta.Add(pagoVenta);

                    // PASO 2.2: Actualizar el monto pagado de la venta
                    // Suma todos los pagos aplicados a esta venta
                    venta.ActualizarMontoPagado();

                    // PASO 2.3: Actualizar el estado de la venta
                    // Cambia de "Pendiente" a "Pagada" o "ParcialmentePagada" según corresponda
                    venta.ActualizarEstado();

                    // Guardar los cambios de la venta (aún en memoria, no en BD)
                    await ventaRepository.UpdateAsync(venta);
                }
            }

            // PASO 3: COMMIT - Si llegamos aquí, TODO salió bien
            // Ahora SÍ se guardan TODOS los cambios en la base de datos de forma atómica
            // Es decir, se guardan el pago, las relaciones PagoVenta y las actualizaciones de ventas
            // TODO junto, en una sola operación
            await unitOfWork.CommitTransactionAsync();

            // PASO 4: Re-consultar el pago para obtener todas las relaciones cargadas
            // Esto asegura que el DTO devuelto tenga toda la información completa
            return (await pagoRepository.GetByIdAsync(createdPago.Id))!.Adapt<PagoDto>();
        }
        catch
        {
            // ROLLBACK: Si ocurre CUALQUIER error en cualquier paso
            // Se deshacen TODOS los cambios (pago, relaciones, actualizaciones)
            // La base de datos queda exactamente como estaba antes de empezar
            // Esto garantiza que NO queden datos inconsistentes
            await unitOfWork.RollbackTransactionAsync();

            // Re-lanzamos la excepción para que el controlador la maneje
            throw;
        }
    }

    public async Task<bool> DeletePagoAsync(int id)
    {
        return await pagoRepository.DeleteAsync(id);
    }

    private static PagoDto MapToDto(Pago pago) => pago.Adapt<PagoDto>();
}
