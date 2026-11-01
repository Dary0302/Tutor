using AutoMapper;
using Auth.Application.DTOs;
using Auth.Domain.Entities;

namespace Auth.Application.Mappers;

public class UserMapper: Profile
{
    public UserMapper()
    {
        CreateMap<User, UserResponseDTO>().ReverseMap();
    }
}