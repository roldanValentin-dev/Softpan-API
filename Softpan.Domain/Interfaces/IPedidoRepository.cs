using Softpan.Domain.Entities;
using Softpan.Domain.Enums;

namespace Softpan.Domain.Interfaces;
public interface IPedidoRepository
{
    Task<Pedido?> GetByIdAsync(int id);
    Task<Pedido?> GetByIdWithDetallesAsync(int id);
    Task<List<Pedido>> GetByClienteIdAsync(int clienteId);
    Task<List<Pedido>> GetAllAsync();
    Task<List<Pedido>> GetByEstadoAsync(EstadoPedidoEnum estado);
    Task<Pedido> CreateAsync(Pedido pedido);
    Task<Pedido> UpdateAsync(Pedido pedido);
    Task<Pedido?> GetCarritoByClienteIdAsync(int clienteId);
    Task<Pedido?> GetByPreferenceIdAsync(string preferenceId);
}
