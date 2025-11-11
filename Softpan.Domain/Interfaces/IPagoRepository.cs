
using Softpan.Domain.Entities;
using Softpan.Domain.Enums;

namespace Softpan.Domain.Interfaces;

public interface IPagoRepository
{
    public Task<Pago?> GetByIdAsync(int id);
    public Task<Pago> CreateAsync(Pago pago);

    public Task<Pago> UpdateAsync(Pago pago);

    public Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    public Task<IEnumerable<Pago>> GetAllAsync();
    Task<IEnumerable<Pago>> GetPagosByClienteAsync(int clienteId);
    Task<IEnumerable<Pago>> GetPagosByTipoAsync(TipoPagoEnum tipo);
    Task<IEnumerable<Pago>> GetPagosByFechaAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<decimal> GetTotalPagosByClienteAsync(int clienteId);

}
