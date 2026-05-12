using Softpan.Domain.Entities;

namespace Softpan.Domain.Interfaces;

public interface IConfiguracionRepository
{
    Task<ConfiguracionPago?> GetByClaveAsync(string clave);
    Task<List<ConfiguracionPago>> GetAllAsync();
    Task<ConfiguracionPago> CreateAsync(ConfiguracionPago config);
    Task<ConfiguracionPago> UpdateAsync(ConfiguracionPago config);
}
