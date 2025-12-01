using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Softpan.Application.Interfaces;

namespace Softpan.API.Controllers;

[Authorize]
[ApiController]
[Route("api/estadisticas")]
public class EstadisticasController(IEstadisticasService estadisticasService) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard = await estadisticasService.GetDashboardAsync();
        return Ok(dashboard);
    }

    [HttpGet("ventas/hoy")]
    public async Task<IActionResult> GetVentasHoy()
    {
        var ventas = await estadisticasService.GetVentasHoyAsync();
        return Ok(ventas);
    }

    [HttpGet("ventas/semana")]
    public async Task<IActionResult> GetVentasSemana()
    {
        var ventas = await estadisticasService.GetVentasSemanaAsync();
        return Ok(ventas);
    }

    [HttpGet("ventas/mes")]
    public async Task<IActionResult> GetVentasMes()
    {
        var ventas = await estadisticasService.GetVentasMesAsync();
        return Ok(ventas);
    }

    [HttpGet("ventas/periodo")]
    public async Task<IActionResult> GetVentasPeriodo([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
    {
        var ventas = await estadisticasService.GetVentasByPeriodoAsync(fechaInicio, fechaFin);
        return Ok(ventas);
    }

    [HttpGet("productos/top")]
    public async Task<IActionResult> GetTopProductos([FromQuery] int top = 5)
    {
        var productos = await estadisticasService.GetTopProductosAsync(top);
        return Ok(productos);
    }

    [HttpGet("productos/top/periodo")]
    public async Task<IActionResult> GetTopProductosPeriodo(
        [FromQuery] int top, 
        [FromQuery] DateTime fechaInicio, 
        [FromQuery] DateTime fechaFin)
    {
        var productos = await estadisticasService.GetTopProductosByPeriodoAsync(top, fechaInicio, fechaFin);
        return Ok(productos);
    }

    [HttpGet("deudas/resumen")]
    public async Task<IActionResult> GetResumenDeudas()
    {
        var deudas = await estadisticasService.GetResumenDeudasAsync();
        return Ok(deudas);
    }

    [HttpGet("deudas/clientes")]
    public async Task<IActionResult> GetClientesConMayorDeuda([FromQuery] int top = 5)
    {
        var clientes = await estadisticasService.GetClientesConMayorDeudaAsync(top);
        return Ok(clientes);
    }

    [HttpGet("comparativa/mensual")]
    public async Task<IActionResult> GetComparativaMensual()
    {
        var comparativa = await estadisticasService.GetComparativaMensualAsync();
        return Ok(comparativa);
    }

    [HttpGet("comparativa/semanal")]
    public async Task<IActionResult> GetComparativaSemanal()
    {
        var comparativa = await estadisticasService.GetComparativaSemanalAsync();
        return Ok(comparativa);
    }

    [HttpGet("ventas/por-dia-semana")]
    public async Task<IActionResult> GetVentasPorDiaSemana()
    {
        var ventas = await estadisticasService.GetVentasPorDiaSemanaAsync();
        return Ok(ventas);
    }
}
