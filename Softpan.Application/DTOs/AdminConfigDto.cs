namespace Softpan.Application.DTOs;

public class ConfiguracionPagoDto
{
    public int Id { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

public class UpdateConfiguracionPagoDto
{
    public string Valor { get; set; } = string.Empty;
}

public class DatosBancariosDto
{
    public int Id { get; set; }
    public string Banco { get; set; } = string.Empty;
    public string Titular { get; set; } = string.Empty;
    public string TipoCuenta { get; set; } = string.Empty;
    public string NumeroCuenta { get; set; } = string.Empty;
    public string? CVU { get; set; }
    public string? Alias { get; set; }
    public bool Activo { get; set; }
}

public class CreateDatosBancariosDto
{
    public string Banco { get; set; } = string.Empty;
    public string Titular { get; set; } = string.Empty;
    public string TipoCuenta { get; set; } = string.Empty;
    public string NumeroCuenta { get; set; } = string.Empty;
    public string? CVU { get; set; }
    public string? Alias { get; set; }
}

public class UpdateDatosBancariosDto
{
    public string Banco { get; set; } = string.Empty;
    public string Titular { get; set; } = string.Empty;
    public string TipoCuenta { get; set; } = string.Empty;
    public string NumeroCuenta { get; set; } = string.Empty;
    public string? CVU { get; set; }
    public string? Alias { get; set; }
    public bool Activo { get; set; }
}

public class DireccionRetiroDto
{
    public int Id { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public string? HorarioInicio { get; set; }
    public string? HorarioFin { get; set; }
    public string? Telefono { get; set; }
}

public class UpdateDireccionRetiroDto
{
    public string Direccion { get; set; } = string.Empty;
    public string? HorarioInicio { get; set; }
    public string? HorarioFin { get; set; }
    public string? Telefono { get; set; }
}

public class PedidoPendientePagoDto
{
    public int Id { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal? MontoConDescuento { get; set; }
    public string? TipoPago { get; set; }
    public string? ReferenciaTransaccion { get; set; }
    public DateTime FechaPago { get; set; }
    public DateTime FechaPedido { get; set; }
}
