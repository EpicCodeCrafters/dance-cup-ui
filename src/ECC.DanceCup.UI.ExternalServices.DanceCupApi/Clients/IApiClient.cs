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
    Task<Result<StartTournamentRegistrationResponse>> StartTournamentRegistrationAsync(StartTournamentRegistrationRequest request, CancellationToken cancellationToken);
    Task<Result<FinishTournamentRegistrationResponse>> FinishTournamentRegistrationAsync(FinishTournamentRegistrationRequest request, CancellationToken cancellationToken);
    Task<Result<ReopenTournamentRegistrationResponse>> ReopenTournamentRegistrationAsync(ReopenTournamentRegistrationRequest request, CancellationToken cancellationToken);
    Task<Result<RegisterCoupleForTournamentResponse>> RegisterCoupleForTournamentAsync(RegisterCoupleForTournamentRequest request, CancellationToken cancellationToken);
    Task<Result<GetTournamentRegistrationResultResponse>> GetTournamentRegistrationResultAsync(GetTournamentRegistrationResultRequest request, CancellationToken cancellationToken);
}