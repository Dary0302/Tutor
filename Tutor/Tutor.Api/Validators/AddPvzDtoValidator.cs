/*using FluentValidation;
using Tutor.Application.Dto;

namespace Tutor.Api.Validators;

public class AddPvzDtoValidator : AbstractValidator<AddPvzDto>
{
    public AddPvzDtoValidator()
    {
        RuleFor(dto => dto.Address.Length)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Адрес не может быть пустым");
    }
}*/