using AutoMapper;
using MediatR;
using Auth.Application.DTOs;
using Auth.Application.HandlerResponse;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;

namespace Auth.Application.UseCases.RefreshToken;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenRequest, Response>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RefreshTokenHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Response> Handle(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        User? user;
        try
        {
            user = await _userRepository.GetUserByRefreshCode(request.RefreshToken, cancellationToken);
            if (user is null)
            {
                return new Response("User not found", 404);
            }
        }
        catch
        {
            return new Response("Internal Server Error", 500);
        }

        user.GenerateRefreshToken();

        try
        {
            await _unitOfWork.Commit(cancellationToken);
        }
        catch
        {
            return new Response("Internal Server Error", 500);
        }

        var userDTO = _mapper.Map<UserResponseDTO>(user);

        return new Response("Token Refreshed", 200, userDTO);
    }
}