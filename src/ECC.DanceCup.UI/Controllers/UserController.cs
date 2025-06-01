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
            return RedirectToAction("Error", "Home","shish tebe a ne token dlya vhoda");//добавить нормальную обработку ошибки
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
            return RedirectToAction("Error", "Home","shish tebe a ne registachia");//добавить нормальную обработку ошибки
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
            return RedirectToAction("Error", "Home","shish tebe a ne token dlya registachii");//добавить нормальную обработку ошибки
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