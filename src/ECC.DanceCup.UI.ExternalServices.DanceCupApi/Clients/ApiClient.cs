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

    public async Task<Result<CreateTournamentResponse>> CreateTournamentAsync(CreateTournamentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _client.CreateTournamentAsync(request, cancellationToken: cancellationToken);
            
            return response;
        }
        catch (RpcException e)
        {
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<GetRefereesResponse>> GetRefereesAsync(GetRefereesRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _client.GetRefereesAsync(request, cancellationToken: cancellationToken);
            
            return response;
        }
        catch (RpcException e)
        {
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<CreateRefereeResponse>> CreateRefereeAsync(CreateRefereeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _client.CreateRefereeAsync(request, cancellationToken: cancellationToken);
            
            return response;
        }
        catch (RpcException e)
        {
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<StartTournamentRegistrationResponse>> StartTournamentRegistrationAsync(StartTournamentRegistrationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _client.StartTournamentRegistrationAsync(request, cancellationToken: cancellationToken);
            
            return response;
        }
        catch (RpcException e)
        {
            return Result.Fail(e.Message);
        }
    }
    
    public async Task<Result<FinishTournamentRegistrationResponse>> FinishTournamentRegistrationAsync(FinishTournamentRegistrationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _client.FinishTournamentRegistrationAsync(request, cancellationToken: cancellationToken);
            
            return response;
        }
        catch (RpcException e)
        {
            return Result.Fail(e.Message);
        }
    }
    
    public async Task<Result<ReopenTournamentRegistrationResponse>> ReopenTournamentRegistrationAsync(ReopenTournamentRegistrationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _client.ReopenTournamentRegistrationAsync(request, cancellationToken: cancellationToken);
            
            return response;
        }
        catch (RpcException e)
        {
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<RegisterCoupleForTournamentResponse>> RegisterCoupleForTournamentAsync(RegisterCoupleForTournamentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _client.RegisterCoupleForTournamentAsync(request, cancellationToken: cancellationToken);
            
            return response;
        }
        catch (RpcException e)
        {
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<GetTournamentRegistrationResultResponse>> GetTournamentRegistrationResultAsync(GetTournamentRegistrationResultRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _client.GetTournamentRegistrationResultAsync(request, cancellationToken: cancellationToken);
            
            return response;
        }
        catch (RpcException e)
        {
            return Result.Fail(e.Message);
        } 
    }

    public async Task<Result<AddTournamentAttachmentResponse>> AddTournamentAttachmentAsync(long tournamentId, string fileName, Stream fileStream, CancellationToken cancellationToken)
    {
        try
        {
            using var call = _client.AddTournamentAttachment(cancellationToken: cancellationToken);
            
            // Send attachment info first
            await call.RequestStream.WriteAsync(new AddTournamentAttachmentRequest
            {
                AttachmentInfo = new AddTournamentAttachmentRequest.Types.AttachmentInfo
                {
                    TournamentId = tournamentId,
                    Name = fileName
                }
            });

            // Stream file content in chunks
            const int chunkSize = 64 * 1024; // 64KB chunks
            var buffer = new byte[chunkSize];
            int bytesRead;
            
            while ((bytesRead = await fileStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
            {
                var chunk = new byte[bytesRead];
                Array.Copy(buffer, chunk, bytesRead);
                
                await call.RequestStream.WriteAsync(new AddTournamentAttachmentRequest
                {
                    AttachmentBytes = Google.Protobuf.ByteString.CopyFrom(chunk)
                });
            }
            
            await call.RequestStream.CompleteAsync();
            var response = await call.ResponseAsync;
            
            return response;
        }
        catch (RpcException e)
        {
            return Result.Fail($"Не удалось загрузить файл: {e.Message}");
        }
    }

    public async Task<Result<ListTournamentAttachmentsResponse>> ListTournamentAttachmentsAsync(ListTournamentAttachmentsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _client.ListTournamentAttachmentsAsync(request, cancellationToken: cancellationToken);
            
            return response;
        }
        catch (RpcException e)
        {
            return Result.Fail($"Не удалось получить список файлов: {e.Message}");
        }
    }

    public async Task<Result<(string fileName, Stream fileStream)>> GetTournamentAttachmentAsync(GetTournamentAttachmentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            using var call = _client.GetTournamentAttachment(request, cancellationToken: cancellationToken);
            
            string fileName = string.Empty;
            var memoryStream = new MemoryStream();
            
            await foreach (var response in call.ResponseStream.ReadAllAsync(cancellationToken))
            {
                if (response.ContentCase == GetTournamentAttachmentResponse.ContentOneofCase.AttachmentInfo)
                {
                    fileName = response.AttachmentInfo.Name;
                }
                else if (response.ContentCase == GetTournamentAttachmentResponse.ContentOneofCase.AttachmentBytes)
                {
                    await memoryStream.WriteAsync(response.AttachmentBytes.Memory, cancellationToken);
                }
            }
            
            memoryStream.Position = 0;
            // Note: The caller (ASP.NET Core File() method) is responsible for disposing the stream
            return (fileName, memoryStream);
        }
        catch (RpcException e)
        {
            return Result.Fail($"Не удалось скачать файл: {e.Message}");
        }
    }
}