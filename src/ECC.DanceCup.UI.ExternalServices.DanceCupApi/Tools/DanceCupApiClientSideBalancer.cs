using ECC.DanceCup.UI.Utils.Extensions;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECC.DanceCup.UI.ExternalServices.DanceCupApi.Tools;

internal class DanceCupApiClientSideBalancer : IDanceCupApiClientSideBalancer, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DanceCupApiClientSideBalancer> _logger;
    
    private GrpcChannel? _channel;

    public DanceCupApiClientSideBalancer(
        IConfiguration configuration,
        ILogger<DanceCupApiClientSideBalancer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Api.Presentation.Grpc.DanceCupApi.DanceCupApiClient GetRandomClient()
    {
        if (_channel is null)
        {
            var danceCupApiAddresses = _configuration
                .GetSection("DanceCupApiOptions:Addresses")
                .Get<string[]>();
            if (danceCupApiAddresses is null or [])
            {
                throw new ArgumentNullException(nameof(danceCupApiAddresses));
            }
        
            var randomAddress = danceCupApiAddresses.Random();
            _channel = GrpcChannel.ForAddress(new Uri(randomAddress, UriKind.Absolute));
            _logger.LogDebug("Открыт канал для {address}", randomAddress);
        }
        
        return new Api.Presentation.Grpc.DanceCupApi.DanceCupApiClient(_channel);
    }

    void IDisposable.Dispose()
    {
        _channel?.Dispose();
    }
}