using ECC.DanceCup.Api.Presentation.Grpc;
using FluentResults;
using Grpc.Core;

namespace ECC.DanceCup.UI.ExternalServices.DanceCupApi.Clients;

public class ApiClient : IApiClient
{
    private readonly Api.Presentation.Grpc.DanceCupApi.DanceCupApiClient _client;

    public ApiClient(Api.Presentation.Grpc.DanceCupApi.DanceCupApiClient client)
    {
        _client = client;
    }

    public async Task<Result<GetDancesResponse>> GetDancesAsync(GetDancesRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _client.GetDancesAsync(request, cancellationToken: cancellationToken);
            
            return response;
        }
        catch (RpcException)
        {
            return Result.Fail("Не удалось получить список танцев");
        }
    }

    public async Task<Result<GetTournamentsResponse>> GetTournamentsAsync(GetTournamentsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _client.GetTournamentsAsync(request, cancellationToken: cancellationToken);
            
            return response;
        }
        catch (RpcException)
        {
            return Result.Fail("Не удалось получить список турниров");
        }
    }
}