using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Softpan.Application.DTOs;
using Softpan.Application.Interfaces;

namespace Softpan.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto login)
    {
        var response = await authService.LoginAsync(login);
        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto register)
    {
        var response = await authService.RegisterAsync(register);
        return Ok(response);
    }
}
