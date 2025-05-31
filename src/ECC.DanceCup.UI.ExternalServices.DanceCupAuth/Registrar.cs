using ECC.DanceCup.UI.ExternalServices.DanceCupAuth.Clients;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECC.DanceCup.UI.ExternalServices.DanceCupAuth;

public static class Registrar
{
    public static IServiceCollection AddDanceCupAuthClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IAuthClient, AuthClient>();
        
        var danceCupAuthAddress = configuration["DanceCupAuthOptions:Address"];
        if (danceCupAuthAddress is null)
        {
            throw new ArgumentNullException(nameof(danceCupAuthAddress));
        }
        services.AddGrpcClient<Auth.Presentation.Grpc.DanceCupAuth.DanceCupAuthClient>(cfg =>
        {
            cfg.Address = new Uri(danceCupAuthAddress, UriKind.Absolute);
        });

        return services;
    }
    
    public static async Task CheckDanceCupAuthHealthAsync(this IServiceProvider serviceProvider, IConfiguration configuration)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(typeof(Registrar).FullName ?? nameof(Registrar));

        try
        {
            var danceCupAuthAddress = configuration["DanceCupAuthOptions:Address"];
            if (danceCupAuthAddress is null)
            {
                throw new ArgumentNullException(nameof(danceCupAuthAddress));
            }
            var channel = GrpcChannel.ForAddress(danceCupAuthAddress);
            var healthClient = new Health.HealthClient(channel);

            var response = await healthClient.CheckAsync(new HealthCheckRequest());

            if (response.Status is HealthCheckResponse.Types.ServingStatus.Serving)
            {
                logger.LogInformation("Сервис dance-cup-auth доступен");
                return;
            }
        }
        catch (RpcException)
        {
            // ignored
        }

        logger.LogError("Сервис dance-cup-auth не доступен");
    }
}