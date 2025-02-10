using ECC.DanceCup.Auth.Presentation.Grpc;
using FluentResults;

namespace ECC.DanceCup.UI.ExternalServices.DanceCupAuth.Clients;

public interface IAuthClient
{
    Task<Result<CreateUserResponse>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken);
    
    Task<Result<GetUserTokenResponse>> GetUserTokenAsync(GetUserTokenRequest request, CancellationToken cancellationToken);
}