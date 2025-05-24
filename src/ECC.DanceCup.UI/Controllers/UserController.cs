using ECC.DanceCup.Auth.Presentation.Grpc;
using ECC.DanceCup.UI.ExternalServices.DanceCupAuth.Clients;
using ECC.DanceCup.UI.Utils.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace ECC.DanceCup.UI.Controllers;

public class UserController : Controller
{
    private readonly IAuthClient _authClient;
    private readonly ILogger<UserController> _logger;

    public UserController(
        IAuthClient authClient,
        ILogger<UserController> logger)
    {
        _authClient = authClient;
        _logger = logger;
    }

    public IActionResult Login()
    {
        return View();
    }
    
    public IActionResult Register()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Login(string phoneNumber, string password)
    {
        var getUserTokenResult = await _authClient.GetUserTokenAsync(
            new GetUserTokenRequest
            {
                Username = phoneNumber,
                Password = password
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

    [HttpPost]
    public async Task<IActionResult> Register(string phoneNumber, string password)
    {
        var createUserResult = await _authClient.CreateUserAsync(
            new CreateUserRequest
            {
                Username = phoneNumber,
                Password = password
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
                Username = phoneNumber,
                Password = password
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
}