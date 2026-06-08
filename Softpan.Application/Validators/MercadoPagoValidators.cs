using FluentValidation;
using Softpan.Application.DTOs;

namespace Softpan.Application.Validators;

public class MercadoPagoPreferenceRequestValidator : AbstractValidator<MercadoPagoDto.MercadoPagoPreferenceRequestDto>
{
    public MercadoPagoPreferenceRequestValidator()
    {
        RuleFor(x => x.EmailPagador)
            .EmailAddress().WithMessage("El email del pagador no es válido")
            .When(x => !string.IsNullOrEmpty(x.EmailPagador));
    }
}
