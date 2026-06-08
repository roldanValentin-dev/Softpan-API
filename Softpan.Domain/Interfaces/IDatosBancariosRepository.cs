using Softpan.Domain.Entities;

namespace Softpan.Domain.Interfaces;

public interface IDatosBancariosRepository
{
    Task<List<DatosBancarios>> GetAllAsync();
    Task<DatosBancarios?> GetByIdAsync(int id);
    Task<DatosBancarios?> GetActivoAsync();
    Task<DatosBancarios> CreateAsync(DatosBancarios datos);
    Task<DatosBancarios> UpdateAsync(DatosBancarios datos);
    Task<bool> DeleteAsync(int id);
}
