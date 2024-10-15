using ECC.DanceCup.Api.Presentation.Grpc;
using FluentResults;

namespace ECC.DanceCup.UI.ExternalServices.DanceCupApi.Clients;

public interface IApiClient
{
    Task<Result<GetDancesResponse>> GetDancesAsync(GetDancesRequest request, CancellationToken cancellationToken);
}