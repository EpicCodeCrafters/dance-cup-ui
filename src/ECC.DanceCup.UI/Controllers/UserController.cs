using ECC.DanceCup.Auth.Presentation.Grpc;
using ECC.DanceCup.UI.Auth;
using ECC.DanceCup.UI.ExternalServices.DanceCupAuth.Clients;
using ECC.DanceCup.UI.Utils.Extensions;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECC.DanceCup.UI.Controllers;

public class UserController : Controller
{
    private readonly IAuthClient _authClient;
    private readonly ILogger<UserController> _logger;
    public UserController(
        IAuthClient authClient,
        ILogger<UserController> logger,
        ICurrentUser currentUser)
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
    public async Task<IActionResult> Login(string Login, string password)
    {
        var getUserTokenResult = await _authClient.GetUserTokenAsync(
            new GetUserTokenRequest
            {
                Username = Login,
                Password = password
            },
            HttpContext.RequestAborted
        );
        if (getUserTokenResult.IsFailed)
        { 
            return RedirectToAction("Error", "Home", "Failed to retrieve user token. Please check your credentials and try again."); // Add proper error handling
        }
        
        HttpContext.Session.SetString("token", getUserTokenResult.Value.Token);
        
        return Redirect("/Home");
    }

    
    [HttpPost]
    public async Task<IActionResult> Register(string Login, string password)
    {
        var createUserResult = await _authClient.CreateUserAsync(
            new CreateUserRequest
            {
                Username = Login,
                Password = password
            },
            HttpContext.RequestAborted
        );
        if (createUserResult.IsFailed)
        {
            _logger.LogDebug(createUserResult.StringifyErrors());
            return RedirectToAction("Error", "Home", "Registration failed. Please try again later.");// Proper error handling
        }
        var getUserTokenResult = await _authClient.GetUserTokenAsync(
            new GetUserTokenRequest
            {
                Username = Login,
                Password = password
            },
            HttpContext.RequestAborted
        );
        if (getUserTokenResult.IsFailed)
        {
            return RedirectToAction("Error", "Home", "Failed to retrieve a token during registration. Please try again later.");//добавить нормальную обработку ошибки
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
}