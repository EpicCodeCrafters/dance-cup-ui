using ECC.DanceCup.Auth.Presentation.Grpc;
using FluentResults;
using Grpc.Core;

namespace ECC.DanceCup.UI.ExternalServices.DanceCupAuth.Clients;

public class AuthClient : IAuthClient
{
    private readonly Auth.Presentation.Grpc.DanceCupAuth.DanceCupAuthClient _client;

    public AuthClient(Auth.Presentation.Grpc.DanceCupAuth.DanceCupAuthClient client)
    {
        _client = client;
    }

    public async Task<Result<CreateUserResponse>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _client.CreateUserAsync(request, cancellationToken: cancellationToken);

            return response;
        }
        catch (RpcException exception) when (exception.StatusCode is StatusCode.InvalidArgument)
        {
            return Result.Fail(exception.Message);
        }
        catch (RpcException)
        {
            return Result.Fail("Не удалось создать пользователя");
        }
    }

    public async Task<Result<GetUserTokenResponse>> GetUserTokenAsync(GetUserTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _client.GetUserTokenAsync(request, cancellationToken: cancellationToken);
            
            return response;
        }
        catch (RpcException exception) when (exception.StatusCode is StatusCode.Unauthenticated)
        {
            return Result.Fail(exception.Message);
        }
        catch (RpcException exception) when (exception.StatusCode is StatusCode.NotFound)
        {
            return Result.Fail(exception.Message);
        }
        catch (RpcException exception) when (exception.StatusCode is StatusCode.InvalidArgument)
        {
            return Result.Fail(exception.Message);
        }
        catch (RpcException)
        {
            return Result.Fail("Не удалось получить токен пользователя");
        }
    }
}