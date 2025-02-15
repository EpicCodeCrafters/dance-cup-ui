using ECC.DanceCup.Auth.Presentation.Grpc;
using ECC.DanceCup.UI.ExternalServices.DanceCupAuth.Clients;
using ECC.DanceCup.UI.Utils.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECC.DanceCup.UI.Controllers;

public class TestController : Controller
{
    private readonly IAuthClient _authClient;
    private readonly ILogger<TestController> _logger;

    public TestController(IAuthClient authClient, ILogger<TestController> logger)
    {
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

        var getUserTokenResponse = await _authClient.GetUserTokenAsync(
            new GetUserTokenRequest
            {
                Username = "admin",
                Password = "adminadmin"
            },
            HttpContext.RequestAborted
        );
        if (getUserTokenResponse.IsFailed)
        {
            return Redirect($"/Home/Error");
        }
        
        HttpContext.Session.SetString("token", getUserTokenResponse.Value.Token);
        
        return Redirect("/Home");
    }

    [Authorize]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("token");
        
        return Redirect("/Home");
    }
}