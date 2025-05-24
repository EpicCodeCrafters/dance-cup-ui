using System.Diagnostics;
using ECC.DanceCup.Api.Presentation.Grpc;
using ECC.DanceCup.UI.Auth;
using ECC.DanceCup.UI.ExternalServices.DanceCupApi.Clients;
using Microsoft.AspNetCore.Mvc;
using ECC.DanceCup.UI.Models;

namespace ECC.DanceCup.UI.Controllers;

public class HomeController : Controller
{
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<HomeController> _logger;
    private readonly IApiClient _apiClient;
    
    public HomeController(ILogger<HomeController> logger, ICurrentUser currentUser, IApiClient apiClient)
    {
        _logger = logger;
        _currentUser = currentUser;
        _apiClient = apiClient;
    }

    
    public async Task<IActionResult> Index()
    {
        if (_currentUser.Authenticated)
        {
            var GetLastTournament = await _apiClient.GetTournamentsAsync(
                new GetTournamentsRequest
                {
                    UserId = _currentUser.Id,
                    PageNumber = 1,
                    PageSize = 1
                }
                ,
                HttpContext.RequestAborted
            );
            if(GetLastTournament.IsFailed)
            {
                return Redirect("/Home/Error"); //добавить нормальную обработку ошибки
            }
            var tournamentList = GetLastTournament.Value.Tournaments;
            if (tournamentList.Any())
            {
                var tournament = tournamentList.First();
                return View(tournament);
            }
            else
            {
                return View(null);
            }
            
        }
        else
        {
            return RedirectToAction("Login", "User");
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
