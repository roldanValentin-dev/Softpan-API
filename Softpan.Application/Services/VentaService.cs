

using Mapster;
using Softpan.Application.DTOs;
using Softpan.Application.Interfaces;
using Softpan.Domain.Entities;
using Softpan.Domain.Enums;
using Softpan.Domain.Interfaces;

namespace Softpan.Application.Services;

public class VentaService(
    IVentaRepository ventaRepository,
    IRedisCacheService cacheService) : IVentaService
{
    public async Task<VentaDto?> GetVentaByIdAsync(int id)
    {
        var cacheKey = $"venta:{id}";
        var cached = await cacheService.GetAsync<VentaDto>(cacheKey);
        if (cached != null)
            return cached;

        var venta = await ventaRepository.GetByIdAsync(id);
        if (venta == null)
            return null;

        var dto = MapToDto(venta);
        await cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(3));

        return dto;
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
        var result = await ventaRepository.DeleteAsync(id);

        if (result)
        {
            await cacheService.RemoveAsync($"venta:{id}");
        }

        return result;
    }


    private static VentaDto MapToDto(Venta venta) => venta.Adapt<VentaDto>();
}
