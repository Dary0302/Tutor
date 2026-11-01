using Auth.Application.HandlerResponse;
using Auth.Domain.Entities;
using MediatR;

namespace Auth.Application.UseCases.CreateUser;

public record CreateUserRequest(
    string Email,
    string? Password,
    Roles Role): IRequest<Response>;