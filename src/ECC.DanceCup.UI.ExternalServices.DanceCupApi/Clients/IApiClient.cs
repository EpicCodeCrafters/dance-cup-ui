using ECC.DanceCup.Api.Presentation.Grpc;
using FluentResults;

namespace ECC.DanceCup.UI.ExternalServices.DanceCupApi.Clients;

public interface IApiClient
{
    Task<Result<GetDancesResponse>> GetDancesAsync(GetDancesRequest request, CancellationToken cancellationToken);
    Task<Result<GetTournamentsResponse>> GetTournamentsAsync(GetTournamentsRequest request, CancellationToken cancellationToken);
    Task<Result<CreateTournamentResponse>> CreateTournamentAsync(CreateTournamentRequest request, CancellationToken cancellationToken);
    Task<Result<GetRefereesResponse>> GetRefereesAsync(GetRefereesRequest request, CancellationToken cancellationToken);
    Task<Result<CreateRefereeResponse>> CreateRefereeAsync(CreateRefereeRequest request, CancellationToken cancellationToken);
}