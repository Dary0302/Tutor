using MediatR;
using Microsoft.AspNetCore.Mvc;
using Auth.Application.UseCases.Authentication;
using Auth.Application.UseCases.CreateUser;
using Auth.Application.UseCases.RefreshToken;
using Auth.WebAPI.Extensions;

namespace Auth.WebAPI.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    public AuthController(IMediator mediadtor)
    {
        _mediator = mediadtor;
    }

    [HttpPost("authentication")]
    public async Task<IActionResult> Authentication([FromBody] AuthenticationRequest request, CancellationToken cancellation)
    {
        var response = await _mediator.Send(request, cancellation);

        if (response is null) return BadRequest();
    
        if (response.Status == 200) response.Data?.Token = JwtExtension.Generate(response.Data);
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellation)
    {
        var response = await _mediator.Send(request, cancellation);

        if (response is null) return BadRequest();

        if (response.Status == 200) response.Data?.Token = JwtExtension.Generate(response.Data);
        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellation)
    {
        var response = await _mediator.Send(request, cancellation);

        if (response is null) return BadRequest();

        return Ok(response);
    }
}