using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Softpan.Application.DTOs;
using Softpan.Application.Exceptions;
using Softpan.Application.Interfaces;
using Softpan.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Softpan.Application.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    IClienteOnlineService clienteOnlineService) : IAuthService
{
    private const int RefreshTokenExpiryDays = 30;

    public async Task<AuthResponseDto> LoginAsync(LoginDto login)
    {
        var user = await userManager.FindByEmailAsync(login.Email);
        if (user is null)
            throw new UnauthorizedException("Email o contraseña incorrectos");

        var isValidPassword = await userManager.CheckPasswordAsync(user, login.Password);
        if (!isValidPassword)
            throw new UnauthorizedException("Email o contraseña incorrectos");

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto register)
    {
        var existingUser = await userManager.FindByEmailAsync(register.Email);
        if (existingUser != null)
            throw new BadRequestException("El usuario ya existe");

        var user = new ApplicationUser
        {
            Email = register.Email,
            UserName = register.Email,
            FirstName = register.FirstName,
            LastName = register.LastName
        };

        var result = await userManager.CreateAsync(user, register.Password);
        if (!result.Succeeded)
            throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, "Vendedor");

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> RegisterClienteOnlineAsync(RegisterClienteOnlineDto register)
    {
        var existingUser = await userManager.FindByEmailAsync(register.Email);
        if (existingUser != null)
            throw new BadRequestException("El usuario ya existe");

        var user = new ApplicationUser
        {
            Email = register.Email,
            UserName = register.Email,
            FirstName = register.Nombre,
            LastName = register.Apellido
        };

        var result = await userManager.CreateAsync(user, register.Password);
        if (!result.Succeeded)
            throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, "Cliente");

        await clienteOnlineService.CreateAsync(register, user.Id);

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        var principal = GetPrincipalFromExpiredToken(refreshTokenDto.Token);
        var email = principal.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(email))
            throw new UnauthorizedException("Token inválido");

        var user = await userManager.FindByEmailAsync(email);
        if (user == null || user.RefreshToken != refreshTokenDto.RefreshToken)
            throw new UnauthorizedException("Token inválido");

        if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            throw new UnauthorizedException("Refresh token expirado");

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<bool> RevokeTokenAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await userManager.UpdateAsync(user);

        return true;
    }

    public async Task<string> GenerateJwtTokenAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            throw new NotFoundException("Usuario no encontrado");

        var roles = await userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Role, roles.FirstOrDefault() ?? string.Empty)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["JWT:Issuer"],
            audience: configuration["JWT:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(ApplicationUser user)
    {
        var token = await GenerateJwtTokenAsync(user.Email!);
        var refreshToken = GenerateRefreshToken();
        var roles = await userManager.GetRolesAsync(user);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays);
        await userManager.UpdateAsync(user);

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false, // No validar expiración
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["JWT:Issuer"],
            ValidAudience = configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]!))
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new UnauthorizedException("Token inválido");
        }

        return principal;
    }
}
