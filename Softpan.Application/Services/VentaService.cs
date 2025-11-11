

using Mapster;
using Softpan.Application.DTOs;
using Softpan.Application.Interfaces;
using Softpan.Domain.Entities;
using Softpan.Domain.Enums;
using Softpan.Domain.Interfaces;

namespace Softpan.Application.Services;
public class VentaService(IVentaRepository ventaRepository) : IVentaService
{
    public async Task<VentaDto?> GetVentaByIdAsync(int id)
    {
        var venta = await ventaRepository.GetByIdAsync(id);
        if (venta == null)
        {
            return null;
        }
        return MapToDto(venta);
    }
    public async Task<IEnumerable<VentaDto>> GetAllVentasAsync()
    {
        var ventas = await ventaRepository.GetAllAsync();
        return ventas.Select(MapToDto);
    }
    public async Task<IEnumerable<VentaDto>> GetAllVentasByClienteAsync(int id)
    {
        var ventas = await ventaRepository.GetVentasByClienteAsync(id);
        return ventas.Select(MapToDto);
    }
    public async Task<IEnumerable<VentaDto>> GetAllVentasPendientesAsync()
    {
        var ventas = await ventaRepository.GetVentasPendientesAsync();

        return ventas.Select(MapToDto);
    }
    public async Task<IEnumerable<VentaDto>> GetAllVentasPagadasAsync()
    {
        var ventas = await ventaRepository.GetVentasByEstadoAsync(EstadoVentaEnum.Pagada);
        return ventas.Select(MapToDto);
    }
    public async Task<VentaDto> CreateVentaAsync(CreateVentaDto createVentaDto)
    {
        var venta = createVentaDto.Adapt<Venta>();

        venta.CalcularMontoTotal();
        venta.ActualizarEstado(); 

        var createdVenta = await ventaRepository.CreateAsync(venta);

        return (await ventaRepository.GetByIdAsync(createdVenta.Id))!.Adapt<VentaDto>();
    }

    public async Task<IEnumerable<VentaDto>> GetAllVentasByFechaAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var ventas = await ventaRepository.GetVentasByFechaAsync(fechaInicio, fechaFin);
        return ventas.Select(MapToDto);
    }
    public async Task<bool> DeleteVentaAsync(int id)
    {
        return await ventaRepository.DeleteAsync(id);
    }


    private static VentaDto MapToDto(Venta venta) => venta.Adapt<VentaDto>();
}
