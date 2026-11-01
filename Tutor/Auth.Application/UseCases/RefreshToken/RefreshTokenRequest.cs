using Auth.Application.HandlerResponse;
using MediatR;

namespace Auth.Application.UseCases.RefreshToken;

public record RefreshTokenRequest(Guid RefreshToken): IRequest<Response>;