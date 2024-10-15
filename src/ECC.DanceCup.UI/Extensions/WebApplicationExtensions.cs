using ECC.DanceCup.UI.ExternalServices.DanceCupApi;
using ECC.DanceCup.UI.ExternalServices.DanceCupAuth;

namespace ECC.DanceCup.UI.Extensions;

internal static class WebApplicationExtensions
{
    public static async Task CheckExternalServicesHealthAsync(this WebApplication webApplication)
    {
        await Task.WhenAll(
            webApplication.Services.CheckDanceCupApiHealthAsync(webApplication.Configuration),
            webApplication.Services.CheckDanceCupAuthHealthAsync(webApplication.Configuration)
        );
    }
}