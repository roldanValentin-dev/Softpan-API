using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Softpan.Application.DTOs;
using Softpan.Application.Interfaces;
using Softpan.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Softpan.Application.Services;

public class AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration) : IAuthService
{
    public async Task<AuthResponseDto> LoginAsync(LoginDto login)
    {
        var user = await userManager.FindByEmailAsync(login.Email);
        if (user is null)
            throw new UnauthorizedAccessException("Email o contraseña incorrectos");

        var isValidPassword = await userManager.CheckPasswordAsync(user, login.Password);
        if (!isValidPassword)
            throw new UnauthorizedAccessException("Email o contraseña incorrectos");

        var token = await GenerateJwtTokenAsync(user.Email!);
        var roles = await userManager.GetRolesAsync(user);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles
        };
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto register)
    {
        var existingUser = await userManager.FindByEmailAsync(register.Email);
        if (existingUser != null)
            throw new ArgumentException("El usuario ya existe");

        var user = new ApplicationUser
        {
            Email = register.Email,
            UserName = register.Email,
            FirstName = register.FirstName,
            LastName = register.LastName
        };

        var result = await userManager.CreateAsync(user, register.Password);
        if (!result.Succeeded)
            throw new ArgumentException(string.Join(", ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, "Vendedor");

        var token = await GenerateJwtTokenAsync(user.Email!);
        var roles = await userManager.GetRolesAsync(user);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles
        };
    }

    public async Task<string> GenerateJwtTokenAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            throw new ArgumentException("Usuario no encontrado");

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
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
