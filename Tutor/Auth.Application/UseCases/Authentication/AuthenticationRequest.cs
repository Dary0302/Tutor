using Auth.Application.HandlerResponse;
using MediatR;

namespace Auth.Application.UseCases.Authentication;

public record AuthenticationRequest
(
    string Email,
    string Password): IRequest<Response>;