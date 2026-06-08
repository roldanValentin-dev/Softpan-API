using FluentValidation;
using Softpan.Application.DTOs;

namespace Softpan.Application.Validators;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("Email inválido")
            .MaximumLength(100).WithMessage("El email no puede exceder 100 caracteres");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida")
            .MinimumLength(4).WithMessage("La contraseña debe tener al menos 4 caracteres");
    }
}

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("Email inválido")
            .MaximumLength(100).WithMessage("El email no puede exceder 100 caracteres");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(50).WithMessage("El nombre no puede exceder 50 caracteres");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("El apellido es requerido")
            .MaximumLength(50).WithMessage("El apellido no puede exceder 50 caracteres");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida")
            .MinimumLength(4).WithMessage("La contraseña debe tener al menos 4 caracteres");
    }
}

public class RegisterClienteOnlineDtoValidator : AbstractValidator<RegisterClienteOnlineDto>
{
    public RegisterClienteOnlineDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("Email inválido")
            .MaximumLength(100).WithMessage("El email no puede exceder 100 caracteres");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(50).WithMessage("El nombre no puede exceder 50 caracteres");

        RuleFor(x => x.Apellido)
            .NotEmpty().WithMessage("El apellido es requerido")
            .MaximumLength(50).WithMessage("El apellido no puede exceder 50 caracteres");

        RuleFor(x => x.Telefono)
            .NotEmpty().WithMessage("El teléfono es requerido")
            .Matches(@"^\+?[0-9\s\-()]{8,20}$").WithMessage("Formato de teléfono inválido");

        RuleFor(x => x.Direccion)
            .NotEmpty().WithMessage("La dirección es requerida")
            .MaximumLength(200).WithMessage("La dirección no puede exceder 200 caracteres");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida")
            .MinimumLength(4).WithMessage("La contraseña debe tener al menos 4 caracteres");
    }
}

public class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
{
    public RefreshTokenDtoValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("El token es requerido");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("El refresh token es requerido");
    }
}
