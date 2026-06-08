using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Softpan.Application.DTOs;
using Softpan.Application.Interfaces;
using System.Security.Claims;

namespace Softpan.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class AuthController(IAuthService authService, IAuditService auditService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto login)
    {
        var response = await authService.LoginAsync(login);
        
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        await auditService.LogAsync(
            response.Email, 
            response.Email, 
            "Login", 
            "Auth", 
            null, 
            $"Usuario {response.Email} inició sesión",
            ipAddress
        );
        
        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto register)
    {
        var response = await authService.RegisterAsync(register);
        
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        await auditService.LogAsync(
            response.Email,
            response.Email,
            "Register",
            "Auth",
            null,
            $"Nuevo empleado registrado: {response.Email}",
            ipAddress
        );
        
        return Ok(response);
    }

    [HttpPost("register-cliente")]
    public async Task<IActionResult> RegisterCliente([FromBody] RegisterClienteOnlineDto register)
    {
        var response = await authService.RegisterClienteOnlineAsync(register);
        
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        await auditService.LogAsync(
            response.Email,
            response.Email,
            "RegisterCliente",
            "Auth",
            null,
            $"Nuevo cliente online registrado: {response.Email}",
            ipAddress
        );
        
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        var response = await authService.RefreshTokenAsync(refreshTokenDto);
        return Ok(response);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        await authService.ForgotPasswordAsync(dto);
        return Ok(new { message = "Si el email existe, recibirás un enlace de recuperación" });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        await authService.ResetPasswordAsync(dto);
        return Ok(new { message = "Contraseña actualizada exitosamente" });
    }

    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> RevokeToken()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await authService.RevokeTokenAsync(userId);
        
        if (result)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            await auditService.LogAsync(
                userId,
                userEmail ?? "unknown",
                "RevokeToken",
                "Auth",
                null,
                "Token revocado manualmente",
                ipAddress
            );
        }
        
        return result ? Ok(new { message = "Token revocado exitosamente" }) : BadRequest();
    }
}
