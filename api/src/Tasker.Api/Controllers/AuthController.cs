using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasker.Application.Commands;
using Tasker.Application.DTOs;
using Tasker.Shared.Abstractions.Commands;

namespace Tasker.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ICommandDispatcher commandDispatcher) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResultDto>> Register([FromBody] RegisterCommand command)
    {
        var result = await commandDispatcher.DispatchAsync<AuthResultDto>(command);
        
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }
        
        return Ok(result);
    }
    
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResultDto>> Login([FromBody] LoginCommand command)
    {
        var result = await commandDispatcher.DispatchAsync<AuthResultDto>(command);
        
        if (!result.Success)
        {
            return Unauthorized(new { error = result.Error });
        }
        
        return Ok(result);
    }
    
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResultDto>> Refresh()
    {
        // Read refresh token from cookie
        var refreshToken = Request.Cookies["refreshToken"];
        
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { error = "Refresh token not found" });
        }
        
        var command = new RefreshTokenCommand(refreshToken);
        var result = await commandDispatcher.DispatchAsync<AuthResultDto>(command);
        
        if (!result.Success)
        {
            return Unauthorized(new { error = result.Error });
        }
        
        return Ok(result);
    }
    
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        // Read refresh token from cookie
        var refreshToken = Request.Cookies["refreshToken"];
        
        if (string.IsNullOrEmpty(refreshToken))
        {
            return BadRequest(new { error = "Refresh token not found" });
        }
        
        var command = new LogoutCommand(refreshToken);
        var result = await commandDispatcher.DispatchAsync<bool>(command);
        
        if (!result)
        {
            return BadRequest(new { error = "Failed to logout" });
        }
        
        return NoContent();
    }
}