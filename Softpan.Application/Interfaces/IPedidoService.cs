
using Softpan.Application.DTOs;
using Softpan.Domain.Enums;

namespace Softpan.Application.Interfaces;

public interface IPedidoService
{
    //cliente
    Task<PedidoDto> CreatePedidoAsync(string ususarioIdentity,CreatePedidoDto dto);
    Task<List<PedidoDto>> GetMisPedidosAsync(string usuarioIdentity);
    Task<PedidoDto> GetPedidoByIdAsync(int id , string usuarioIdentity);
    Task<PedidoDto> CancelarPedidoAsync(int id, string usuarioIdentity);

    //admin
    Task<List<PedidoResumenDto>> GetAllPedidosAsync();
    Task<List<PedidoResumenDto>> GetPedidosByEstadoAsync(EstadoPedidoEnum estado);
    Task<PedidoDto> GetPedidoDetalleByIdAsync(int id);
    Task<PedidoDto> UpdateEstadoPedidoAsync(int id, UpdateEstadoPedidoDto dto);


    //carrito 
    Task<CarritoDto> ObtenerOCrearCarritoAsync(string usuarioId);
    Task<CarritoDto> AgregarItemAlCarritoAsync(string usuarioId, int productoId, int cantidad);
    Task<CarritoDto> ActualizarItemEnCarritoAsync(string usuarioId, int productoId, int cantidad);
    Task<bool> RemoverItemDelCarritoAsync(string usuarioId, int productoId);
    Task<CarritoDto> LimpiarCarritoAsync(string usuarioId);
    Task<PedidoDto> ProcesarCheckoutDesdeCarritoAsync(string usuarioId, ProcesarCheckoutDto dto);
}
