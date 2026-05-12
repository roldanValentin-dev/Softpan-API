using FluentAssertions;
using Softpan.Application.DTOs;
using Softpan.Application.Validators;
using Xunit;

namespace Softpan.Tests.Unit.Validators;

public class CreatePedidoValidatorTests
{
    private readonly CreatePedidoValidator _validator;

    public CreatePedidoValidatorTests()
    {
        _validator = new CreatePedidoValidator();
    }

    [Fact]
    public void Validate_DatosValidos_NoTieneErrores()
    {
        // Arrange
        var dto = new CreatePedidoDto
        {
            FechaEntrega = DateTime.Today.AddDays(2),
            Observaciones = "Observaciones de prueba",
            Detalles = new List<CreatePedidoDetalleDto>
            {
                new() { ProductoId = 1, Cantidad = 2 }
            }
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_FechaEntregaAnteriorAHoy_TieneError()
    {
        // Arrange
        var dto = new CreatePedidoDto
        {
            FechaEntrega = DateTime.Today.AddDays(-1),
            Detalles = new List<CreatePedidoDetalleDto>
            {
                new() { ProductoId = 1, Cantidad = 2 }
            }
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FechaEntrega");
    }

    [Fact]
    public void Validate_SinDetalles_TieneError()
    {
        // Arrange
        var dto = new CreatePedidoDto
        {
            FechaEntrega = DateTime.Today.AddDays(2),
            Detalles = new List<CreatePedidoDetalleDto>()
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Detalles");
    }

    [Fact]
    public void Validate_CantidadCero_TieneError()
    {
        // Arrange
        var dto = new CreatePedidoDto
        {
            FechaEntrega = DateTime.Today.AddDays(2),
            Detalles = new List<CreatePedidoDetalleDto>
            {
                new() { ProductoId = 1, Cantidad = 0 }
            }
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Cantidad"));
    }

    [Fact]
    public void Validate_ProductoIdInvalido_TieneError()
    {
        // Arrange
        var dto = new CreatePedidoDto
        {
            FechaEntrega = DateTime.Today.AddDays(2),
            Detalles = new List<CreatePedidoDetalleDto>
            {
                new() { ProductoId = 0, Cantidad = 2 }
            }
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("ProductoId"));
    }

    [Fact]
    public void Validate_ObservacionesMuyLargas_TieneError()
    {
        // Arrange
        var dto = new CreatePedidoDto
        {
            FechaEntrega = DateTime.Today.AddDays(2),
            Observaciones = new string('a', 501),
            Detalles = new List<CreatePedidoDetalleDto>
            {
                new() { ProductoId = 1, Cantidad = 2 }
            }
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Observaciones");
    }
}

public class ProcesarCheckoutValidatorTests
{
    private readonly ProcesarCheckoutValidator _validator;

    public ProcesarCheckoutValidatorTests()
    {
        _validator = new ProcesarCheckoutValidator();
    }

    [Fact]
    public void Validate_DatosValidos_NoTieneErrores()
    {
        var dto = new ProcesarCheckoutDto
        {
            FechaEntrega = DateTime.Today.AddDays(1),
            Observaciones = "Test"
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_FechaPasada_TieneError()
    {
        var dto = new ProcesarCheckoutDto
        {
            FechaEntrega = DateTime.Today.AddDays(-1)
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FechaEntrega");
    }

    [Fact]
    public void Validate_ObservacionesMuyLargas_TieneError()
    {
        var dto = new ProcesarCheckoutDto
        {
            FechaEntrega = DateTime.Today.AddDays(1),
            Observaciones = new string('x', 501)
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Observaciones");
    }
}

public class MercadoPagoPreferenceRequestValidatorTests
{
    private readonly MercadoPagoPreferenceRequestValidator _validator;

    public MercadoPagoPreferenceRequestValidatorTests()
    {
        _validator = new MercadoPagoPreferenceRequestValidator();
    }

    [Fact]
    public void Validate_EmailValido_NoTieneErrores()
    {
        var dto = new MercadoPagoDto.MercadoPagoPreferenceRequestDto
        {
            EmailPagador = "test@email.com"
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmailInvalido_TieneError()
    {
        var dto = new MercadoPagoDto.MercadoPagoPreferenceRequestDto
        {
            EmailPagador = "email-invalido"
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EmailPagador");
    }

    [Fact]
    public void Validate_EmailVacio_NoTieneError()
    {
        var dto = new MercadoPagoDto.MercadoPagoPreferenceRequestDto
        {
            EmailPagador = null
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue(); // Es opcional, no se valida si es null
    }
}

public class UpdateClienteOnlineDtoValidatorTests
{
    private readonly UpdateClienteOnlineDtoValidator _validator;

    public UpdateClienteOnlineDtoValidatorTests()
    {
        _validator = new UpdateClienteOnlineDtoValidator();
    }

    [Fact]
    public void Validate_DatosValidos_NoTieneErrores()
    {
        // Arrange
        var dto = new UpdateClienteOnlineDto
        {
            Nombre = "Juan",
            Apellido = "Pérez",
            Telefono = "+54 11 1234-5678",
            Direccion = "Av. Siempre Viva 123"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_NombreVacio_TieneError()
    {
        // Arrange
        var dto = new UpdateClienteOnlineDto
        {
            Nombre = "",
            Apellido = "Pérez",
            Telefono = "+54 11 1234-5678"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nombre");
    }

    [Fact]
    public void Validate_ApellidoVacio_TieneError()
    {
        // Arrange
        var dto = new UpdateClienteOnlineDto
        {
            Nombre = "Juan",
            Apellido = "",
            Telefono = "+54 11 1234-5678"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Apellido");
    }

    [Fact]
    public void Validate_TelefonoInvalido_TieneError()
    {
        // Arrange
        var dto = new UpdateClienteOnlineDto
        {
            Nombre = "Juan",
            Apellido = "Pérez",
            Telefono = "123" // Muy corto
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Telefono");
    }

    [Fact]
    public void Validate_DireccionMuyLarga_TieneError()
    {
        // Arrange
        var dto = new UpdateClienteOnlineDto
        {
            Nombre = "Juan",
            Apellido = "Pérez",
            Direccion = new string('a', 201)
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Direccion");
    }
}
