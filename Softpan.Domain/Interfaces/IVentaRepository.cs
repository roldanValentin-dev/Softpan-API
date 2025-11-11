
using Softpan.Domain.Entities;
using Softpan.Domain.Enums;

namespace Softpan.Domain.Interfaces;

public interface IVentaRepository
{
    public Task<Venta?> GetByIdAsync(int id);

    public Task<IEnumerable<Venta>> GetAllAsync();

    public Task<Venta> CreateAsync(Venta venta);

    public Task<Venta> UpdateAsync(Venta venta);

    public Task<bool> DeleteAsync(int id);

    public Task<bool> ExistsAsync(int id);


    Task<IEnumerable<Venta>> GetVentasByClienteAsync(int clienteId);
    Task<IEnumerable<Venta>> GetVentasByEstadoAsync(EstadoVentaEnum estado);
    Task<IEnumerable<Venta>> GetVentasPendientesAsync();
    Task<IEnumerable<Venta>> GetVentasByFechaAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<decimal> GetTotalVentasByClienteAsync(int clienteId);
}
