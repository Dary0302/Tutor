using MediatR;
using Auth.Application.HandlerResponse;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
namespace Auth.Application.UseCases.CreateUser;

public class CreateUserHandler : IRequestHandler<CreateUserRequest, Response>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHashingService _service;

        public CreateUserHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IPasswordHashingService service)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _service = service;
        }

        public async Task<Response> Handle(CreateUserRequest request, CancellationToken cancellationToken)
        {

            Roles role;

            try
            {
                role = request.Role;
            }
            catch
            {
                return new Response("Internal Server Error", 500);
            }

            try
            {
                var isAvaliable = await _userRepository.AnyAsync(request.Email, cancellationToken);
                if (isAvaliable)
                {
                    return new Response("Email already in use", 404);
                }
            }
            catch
            {
                return new Response("Internal Server Error", 500);
            }

            // Generate User object
            var user = new User(request.Email, _service.HashPassword(request.Password));
            user.Role = role;

            try
            {
                _userRepository.Create(user);
                await _unitOfWork.Commit(cancellationToken);
            }
            catch
            {
                return new Response("Internal Server Error", 500);
            }

            return new Response("User created", 201);
        }
    }