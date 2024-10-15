using ECC.DanceCup.UI.ExternalServices.DanceCupApi.Clients;
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
        services.AddScoped<IApiClient, ApiClient>();

        var danceCupApiAddress = configuration["DanceCupApiOptions:Address"];
        if (danceCupApiAddress is null)
        {
            throw new ArgumentNullException(nameof(danceCupApiAddress));
        }
        services.AddGrpcClient<Api.Presentation.Grpc.DanceCupApi.DanceCupApiClient>(cfg =>
        {
            cfg.Address = new Uri(danceCupApiAddress, UriKind.Absolute);
        });

        return services;
    }

    public static async Task CheckDanceCupApiHealthAsync(this IServiceProvider serviceProvider, IConfiguration configuration)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(typeof(Registrar).FullName ?? nameof(Registrar));

        try
        {
            var danceCupApiAddress = configuration["DanceCupApiOptions:Address"];
            if (danceCupApiAddress is null)
            {
                throw new ArgumentNullException(nameof(danceCupApiAddress));
            }

            var channel = GrpcChannel.ForAddress(danceCupApiAddress);
            var healthClient = new Health.HealthClient(channel);

            var response = await healthClient.CheckAsync(new HealthCheckRequest());

            if (response.Status is HealthCheckResponse.Types.ServingStatus.Serving)
            {
                logger.LogInformation("DanceCupApi доступен");
                return;
            }
        }
        catch (RpcException)
        {
            // ignored
        }

        logger.LogError("DanceCupApi не доступен");
    }
}