namespace Softpan.Infrastructure.Services;

public static class EmailTemplates
{
    public static string PedidoCreado(string clienteNombre, int pedidoId, decimal total, string estado)
    {
        return $"""
        <h2>¡Pedido confirmado!</h2>
        <p>Hola {clienteNombre},</p>
        <p>Tu pedido <strong>#{pedidoId}</strong> fue creado exitosamente.</p>
        <p><strong>Total:</strong> ${total}</p>
        <p><strong>Estado:</strong> {estado}</p>
        """;
    }

    public static string PedidoEstadoActualizado(string clienteNombre, int pedidoId, string estadoNuevo)
    {
        return $"""
        <h2>Estado de tu pedido actualizado</h2>
        <p>Hola {clienteNombre},</p>
        <p>Tu pedido <strong>#{pedidoId}</strong> ahora está: <strong>{estadoNuevo}</strong></p>
        """;
    }

    public static string ResetPassword(string clienteNombre, string resetLink)
    {
        return $"""
        <h2>Recuperación de contraseña</h2>
        <p>Hola {clienteNombre},</p>
        <p>Hacé clic en el siguiente enlace para restablecer tu contraseña:</p>
        <p><a href="{resetLink}">Restablecer contraseña</a></p>
        <p>Si no solicitaste esto, ignorá este mensaje.</p>
        """;
    }
}