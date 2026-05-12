using Microsoft.EntityFrameworkCore;
using Softpan.Domain.Entities;
using Softpan.Domain.Enums;
using Softpan.Domain.Interfaces;
using Softpan.Infrastructure.Data;


namespace Softpan.Infrastructure.Repositories
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly ApplicationDbContext _context;

        public PedidoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Pedido?> GetByIdAsync(int id)
        {
            return await _context.Pedidos
                .Include(p => p.ClienteOnline)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Pedido?> GetByIdWithDetallesAsync(int id)
        {
            return await _context.Pedidos
                .Include(p => p.ClienteOnline)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Pedido>> GetByClienteIdAsync(int clienteId)
        {
            return await _context.Pedidos
                .Include(p => p.ClienteOnline)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(p => p.ClienteOnlineId == clienteId)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();
        }

        public async Task<List<Pedido>> GetAllAsync()
        {
            return await _context.Pedidos
                .Include(p => p.ClienteOnline)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();
        }

        public async Task<List<Pedido>> GetByEstadoAsync(EstadoPedidoEnum estado)
        {
            return await _context.Pedidos
                .Include(p => p.ClienteOnline)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(p => p.Estado == estado)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();
        }

        public async Task<Pedido> CreateAsync(Pedido pedido)
        {
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();
            return pedido;
        }

        public async Task<Pedido> UpdateAsync(Pedido pedido)
        {
            _context.Pedidos.Update(pedido);
            await _context.SaveChangesAsync();
            return pedido;
        }

        public async Task<Pedido?> GetCarritoByClienteIdAsync(int clienteId)
        {
            return await _context.Pedidos
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(p => p.ClienteOnlineId == clienteId && p.Estado == EstadoPedidoEnum.Carrito)
                .FirstOrDefaultAsync();
        }

        public async Task<Pedido?> GetByPreferenceIdAsync(string preferenceId)
        {
            return await _context.Pedidos
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.MercadoPagoPreferenceId == preferenceId);
        }

    }
}
