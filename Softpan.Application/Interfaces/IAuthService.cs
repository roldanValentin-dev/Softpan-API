using Softpan.Application.DTOs;


namespace Softpan.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto> RegisterClienteOnlineAsync(RegisterClienteOnlineDto registerDto);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
    Task<bool> RevokeTokenAsync(string userId);
    Task<string> GenerateJwtTokenAsync(string email);
}
