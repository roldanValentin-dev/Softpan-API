namespace Softpan.Domain.Entities;

public class DatosBancarios
{
    public int Id { get; set; }
    public string Banco { get; set; } = string.Empty;
    public string Titular { get; set; } = string.Empty;
    public string TipoCuenta { get; set; } = string.Empty;
    public string NumeroCuenta { get; set; } = string.Empty;
    public string? CVU { get; set; }
    public string? Alias { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
