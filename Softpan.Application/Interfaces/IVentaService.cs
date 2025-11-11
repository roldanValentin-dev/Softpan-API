

using Softpan.Application.DTOs;


namespace Softpan.Application.Interfaces;

public interface IVentaService
{
    public Task<VentaDto?> GetVentaByIdAsync(int id);
    public Task<IEnumerable<VentaDto>> GetAllVentasAsync();
    public Task<IEnumerable<VentaDto>> GetAllVentasByClienteAsync(int clienteId);

    public Task<IEnumerable<VentaDto>> GetAllVentasPendientesAsync();
    Task<IEnumerable<VentaDto>> GetAllVentasPagadasAsync();
    Task<IEnumerable<VentaDto>> GetAllVentasByFechaAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<VentaDto> CreateVentaAsync(CreateVentaDto dto);
    Task<bool> DeleteVentaAsync(int id);

}
