namespace ECC.DanceCup.UI.ExternalServices.DanceCupApi.Tools;

internal interface IDanceCupApiClientSideBalancer
{
    Api.Presentation.Grpc.DanceCupApi.DanceCupApiClient GetRandomClient();
}