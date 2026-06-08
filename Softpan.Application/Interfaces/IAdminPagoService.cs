using Softpan.Application.DTOs;

namespace Softpan.Application.Interfaces;

public interface IAdminPagoService
{
    Task<ConfiguracionPagoDto> GetDescuentoAsync();
    Task<ConfiguracionPagoDto> UpdateDescuentoAsync(decimal porcentaje);

    Task<List<DatosBancariosDto>> GetDatosBancariosAsync();
    Task<DatosBancariosDto> CreateDatosBancariosAsync(CreateDatosBancariosDto dto);
    Task<DatosBancariosDto> UpdateDatosBancariosAsync(int id, UpdateDatosBancariosDto dto);
    Task<bool> DeleteDatosBancariosAsync(int id);

    Task<DireccionRetiroDto> GetDireccionRetiroAsync();
    Task<DireccionRetiroDto> UpdateDireccionRetiroAsync(UpdateDireccionRetiroDto dto);

    Task<List<PedidoPendientePagoDto>> GetPedidosPendientesPagoAsync();
    Task<PedidoDto> ConfirmarPagoPedidoAsync(int pedidoId);

    Task<CostoEnvioConfigDto> GetCostoEnvioConfigAsync();
    Task<CostoEnvioConfigDto> UpdateCostoEnvioConfigAsync(CostoEnvioConfigDto dto);
}
