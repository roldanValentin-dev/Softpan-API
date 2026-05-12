using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Softpan.Application.DTOs;
using Softpan.Application.Exceptions;
using Softpan.Domain.Entities;
using Softpan.Domain.Enums;
using Softpan.Domain.Interfaces;
using Softpan.Infrastructure.Data;
using Softpan.Infrastructure.Services;
using Xunit;
using static Softpan.Application.DTOs.MercadoPagoDto;

namespace Softpan.Tests.Unit.Services;

public class MercadoPagoServiceTests
{
    // Helper: crea un mock de IConfiguration con AccessToken configurado
    private static IConfiguration CrearConfig()
    {
        // Usar ConfigurationBuilder en vez de Moq para el indexer con ":"
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "MercadoPago:AccessToken", "test-token-123" }
            })
            .Build();
    }

    // Helper: crea DbContext InMemory con nombre único por test
    private ApplicationDbContext CrearContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    // Helper: crea el service con dependencias mockeadas
    private MercadoPagoService CrearService(
        Mock<IPedidoRepository>? pedidoRepo = null,
        Mock<IProductoRepository>? productoRepo = null,
        ApplicationDbContext? context = null,
        string dbName = "testdb")
    {
        return new MercadoPagoService(
            (pedidoRepo ?? new Mock<IPedidoRepository>()).Object,
            (productoRepo ?? new Mock<IProductoRepository>()).Object,
            context ?? CrearContext(dbName),
            CrearConfig(),
            new Mock<IHttpClientFactory>().Object
        );
    }

    // ========================================================================
    // CREAR PREFERENCIA DE PAGO
    // ========================================================================
    [Fact]
    public async Task CrearPreferenciaPagoAsync_PedidoNoExiste_LanzaNotFound()
    {
        var mockPedidoRepo = new Mock<IPedidoRepository>();
        mockPedidoRepo.Setup(x => x.GetByIdWithDetallesAsync(999)).ReturnsAsync((Pedido?)null);

        var service = CrearService(pedidoRepo: mockPedidoRepo, dbName: "test_noexiste");

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CrearPreferenciaPagoAsync(999, "test@email.com"));
    }

    [Fact]
    public async Task CrearPreferenciaPagoAsync_PedidoNoPendiente_LanzaBadRequest()
    {
        var mockPedidoRepo = new Mock<IPedidoRepository>();
        mockPedidoRepo.Setup(x => x.GetByIdWithDetallesAsync(1))
            .ReturnsAsync(new Pedido { Id = 1, Estado = EstadoPedidoEnum.Confirmado });

        var service = CrearService(pedidoRepo: mockPedidoRepo, dbName: "test_no_pendiente");

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.CrearPreferenciaPagoAsync(1, null));
    }

    // ========================================================================
    // CONSULTAR ESTADO
    // ========================================================================
    [Fact]
    public async Task ConsultarEstadoPagoAsync_PreferenceNoExiste_RetornaNoEncontrado()
    {
        var service = CrearService(dbName: "test_consultar_vacio");
        var result = await service.ConsultarEstadoPagoAsync("pref_no_existe");
        result.Estado.Should().Be("no_encontrado");
    }

    [Fact]
    public async Task ConsultarEstadoPagoAsync_PreferenceExiste_RetornaEstado()
    {
        using var context = CrearContext("test_consultar_existe");
        context.Pedidos.Add(new Pedido
        {
            Id = 1,
            ClienteOnlineId = 1,
            MercadoPagoPreferenceId = "pref-123",
            PaymentStatus = "approved"
        });
        await context.SaveChangesAsync();

        var service = CrearService(context: context, dbName: "test_consultar_existe");
        var result = await service.ConsultarEstadoPagoAsync("pref-123");

        result.Estado.Should().Be("approved");
    }

    // ========================================================================
    // PROCESAR WEBHOOK
    // ========================================================================
    [Fact]
    public async Task ProcesarWebhookMercadoPagoAsync_TipoInvalido_RetornaError()
    {
        var service = CrearService(dbName: "test_webhook_tipo");
        var json = @"{""type"":""merchant_order"",""data"":{""id"":""123""}}";
        var result = await service.ProcesarWebhookMercadoPagoAsync(json, "", "");

        result.Exitoso.Should().BeFalse();
        result.Mensaje.Should().Contain("no soportado");
    }

    [Fact]
    public async Task ProcesarWebhookMercadoPagoAsync_SinDataId_RetornaError()
    {
        var service = CrearService(dbName: "test_webhook_sindata");
        var json = @"{""type"":""payment"",""data"":{}}";
        var result = await service.ProcesarWebhookMercadoPagoAsync(json, "", "");

        result.Exitoso.Should().BeFalse();
    }
}
