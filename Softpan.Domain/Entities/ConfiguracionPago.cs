namespace Softpan.Domain.Entities;

public class ConfiguracionPago
{
    public int Id { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
}
