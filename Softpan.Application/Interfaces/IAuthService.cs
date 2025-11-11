using Softpan.Application.DTOs;


namespace Softpan.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
    Task<string> GenerateJwtTokenAsync(string email);
}
