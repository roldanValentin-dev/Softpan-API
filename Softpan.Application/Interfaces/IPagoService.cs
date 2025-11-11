using Softpan.Application.DTOs;
using Softpan.Domain.Enums;

namespace Softpan.Application.Interfaces;

public interface IPagoService
{
    Task<PagoDto?> GetPagoByIdAsync(int id);
    Task<PagoDetalleDto?> GetPagoDetalleByIdAsync(int id);
    Task<IEnumerable<PagoDto>> GetAllPagosAsync();
    Task<IEnumerable<PagoDto>> GetPagosByClienteAsync(int clienteId);
    Task<IEnumerable<PagoDto>> GetPagosByTipoAsync(TipoPagoEnum tipo);
    Task<IEnumerable<PagoDto>> GetPagosByFechaAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<PagoDto> CreatePagoAsync(CreatePagoDto dto);
    Task<bool> DeletePagoAsync(int id);
}
