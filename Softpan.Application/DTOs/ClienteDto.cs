
using Softpan.Domain.Enums;

namespace Softpan.Application.DTOs;

public class ClienteDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public TipoClienteEnum TipoCliente { get; set; }
    public string TipoClienteNombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}
public class CreateClienteDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public TipoClienteEnum TipoCliente { get; set; }
}

public class UpdateClienteDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public TipoClienteEnum TipoCliente { get; set; }
}
