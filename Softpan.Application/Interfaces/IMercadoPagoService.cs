using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Softpan.Application.DTOs.MercadoPagoDto;

namespace Softpan.Application.Interfaces
{
    public interface IMercadoPagoService
    {
        Task<MercadoPagoPreferenceResponseDto> CrearPreferenciaPagoAsync(
        int pedidoId, string? emailPagador);
        Task<PagoResultadoDto> ProcesarWebhookMercadoPagoAsync(
            string webhookJson, string xSignatureHeader, string xRequestId);
        Task<EstadoPagoDto> ConsultarEstadoPagoAsync(string preferenceId);
    }
}
