using ECC.DanceCup.Api.Presentation.Grpc;
using ECC.DanceCup.Auth.Presentation.Grpc;
using ECC.DanceCup.UI.ExternalServices.DanceCupApi.Clients;
using ECC.DanceCup.UI.ExternalServices.DanceCupAuth.Clients;
using ECC.DanceCup.UI.Utils.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECC.DanceCup.UI.Controllers;

public class TestController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly IAuthClient _authClient;
    private readonly ILogger<TestController> _logger;

    public TestController(
        IApiClient apiClient, 
        IAuthClient authClient,
        ILogger<TestController> logger)
    {
        _apiClient = apiClient;
        _authClient = authClient;
        _logger = logger;
    }

    public async Task<IActionResult> Login()
    {
       var createUserResult = await _authClient.CreateUserAsync(
            new CreateUserRequest
            {
                Username = "admin",
                Password = "adminadmin"
            },
            HttpContext.RequestAborted
        );
        if (createUserResult.IsFailed)
        {
            _logger.LogDebug(createUserResult.StringifyErrors());
        }

        var getUserTokenResult = await _authClient.GetUserTokenAsync(
            new GetUserTokenRequest
            {
                Username = "admin",
                Password = "adminadmin"
            },
            HttpContext.RequestAborted
        );
        if (getUserTokenResult.IsFailed)
        {
            return Redirect("/Home/Error");
        }
        
        HttpContext.Session.SetString("token", getUserTokenResult.Value.Token);
        
        return Redirect("/Home");
    }

    [Authorize]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("token");
        
        return Redirect("/Home");
    }

    [Authorize]
    public async Task<IActionResult> Dances()
    {
        var getDancesResult = await _apiClient.GetDancesAsync(
            new GetDancesRequest(),
            HttpContext.RequestAborted
        );
        if (getDancesResult.IsFailed)
        {
            return Redirect("/Home/Error");
        }
        
        return Json(getDancesResult.Value);
    }
}