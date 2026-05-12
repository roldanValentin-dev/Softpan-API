using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Softpan.Application.Interfaces;
using System.Security.Claims;
using static Softpan.Application.DTOs.MercadoPagoDto;

namespace Softpan.API.Controllers;

[ApiController]
[Route("api/mercadopago")]
public class MercadoPagoController(IMercadoPagoService mercadopagoService) : ControllerBase
{
    [Authorize]
    [HttpPost("crear-preferencia")]
    public async Task<IActionResult> CrearPreferencia(int pedidoId, [FromBody] MercadoPagoPreferenceRequestDto dto)
    {
        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var resultado = await mercadopagoService.CrearPreferenciaPagoAsync(pedidoId, dto.EmailPagador);
        return Ok(resultado);
    }

    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> RecibirWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        // Headers necesarios para validar autenticidad del webhook
        var xSignature = Request.Headers.TryGetValue("X-Signature", out var sig) ? sig.ToString() : string.Empty;
        var xRequestId = Request.Headers.TryGetValue("X-Request-Id", out var reqId) ? reqId.ToString() : string.Empty;

        var resultado = await mercadopagoService.ProcesarWebhookMercadoPagoAsync(body, xSignature, xRequestId);
        return Ok(resultado);
    }
}
