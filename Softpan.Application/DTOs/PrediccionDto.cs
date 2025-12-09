namespace Softpan.Application.DTOs;

// Predicción de demanda por producto
public class PrediccionDemandaDto
{
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string DiaSemana { get; set; } = string.Empty;
    public decimal PromedioVentas { get; set; }
    public decimal TendenciaCrecimiento { get; set; }  // Porcentaje
    public decimal SugerenciaProduccion { get; set; }
}
