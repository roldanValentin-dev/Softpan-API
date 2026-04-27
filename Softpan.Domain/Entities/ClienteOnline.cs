using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Softpan.Domain.Entities;

public class ClienteOnline
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }

    // Relación con la entidad de usuario de Identity
    public string UsuarioIdentityId { get; set; } = string.Empty;
    // Propiedad de navegación para el usuario de Identity
    public ApplicationUser Usuario { get; set; } = null!;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public bool Activo { get; set; } = true;

  
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

}
