

namespace Softpan.Application.DTOs;

public class ClienteOnlineDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public DateTime FechaRegistro { get; set; }
}

public class RegisterClienteOnlineDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }

}

public class UpdateClienteOnlineDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
}
