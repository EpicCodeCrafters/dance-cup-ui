using ECC.DanceCup.UI.ExternalServices.DanceCupApi.Clients;
using ECC.DanceCup.UI.ExternalServices.DanceCupApi.Tools;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECC.DanceCup.UI.ExternalServices.DanceCupApi;

public static class Registrar
{
    public static IServiceCollection AddDanceCupApiClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IApiClient, ApiClient>(serviceProvider =>
        {
            var balancer = serviceProvider.GetRequiredService<IDanceCupApiClientSideBalancer>();
            return new ApiClient(balancer.GetRandomClient());
        });

        services.AddScoped<IDanceCupApiClientSideBalancer, DanceCupApiClientSideBalancer>();

        return services;
    }

    public static async Task CheckDanceCupApiHealthAsync(this IServiceProvider serviceProvider, IConfiguration configuration)
    {
        var danceCupApiAddresses = configuration
            .GetSection("DanceCupApiOptions:Addresses")
            .Get<string[]>();
        if (danceCupApiAddresses is null or [])
        {
            throw new ArgumentNullException(nameof(danceCupApiAddresses));
        }

        await Task.WhenAll(
            danceCupApiAddresses.Select((address, i) => CheckDanceCupApiHealthAsync(
                serviceProvider, 
                address, 
                postfix: i > 0 ? i.ToString() : string.Empty
                )
            )
        );
    }

    private static async Task CheckDanceCupApiHealthAsync(
        this IServiceProvider serviceProvider, 
        string danceCupApiAddress,
        string postfix)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(typeof(Registrar).FullName ?? nameof(Registrar));
        var serviceName = string.IsNullOrWhiteSpace(postfix)
            ? "dance-cup-api"
            : $"dance-cup-api-{postfix}";

        try
        {
            var channel = GrpcChannel.ForAddress(danceCupApiAddress);
            var healthClient = new Health.HealthClient(channel);

            var response = await healthClient.CheckAsync(new HealthCheckRequest());

            if (response.Status is HealthCheckResponse.Types.ServingStatus.Serving)
            {
                logger.LogInformation("Сервис {service} доступен", serviceName);
                return;
            }
        }
        catch (RpcException)
        {
            // ignored
        }

        logger.LogError("Сервис {service} не доступен", serviceName);
    }
}