namespace Softpan.Domain.Entities;

public class DireccionRetiro
{
    public int Id { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public string? HorarioInicio { get; set; }
    public string? HorarioFin { get; set; }
    public string? Telefono { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
