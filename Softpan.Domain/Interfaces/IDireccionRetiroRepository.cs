using Softpan.Domain.Entities;

namespace Softpan.Domain.Interfaces;

public interface IDireccionRetiroRepository
{
    Task<DireccionRetiro?> GetActivaAsync();
    Task<DireccionRetiro> CreateAsync(DireccionRetiro direccion);
    Task<DireccionRetiro> UpdateAsync(DireccionRetiro direccion);
}
