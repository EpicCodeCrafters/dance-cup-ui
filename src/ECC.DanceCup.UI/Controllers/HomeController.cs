using System.Diagnostics;
using ECC.DanceCup.Api.Presentation.Grpc;
using ECC.DanceCup.UI.Auth;
using ECC.DanceCup.UI.ExternalServices.DanceCupApi.Clients;
using Microsoft.AspNetCore.Mvc;
using ECC.DanceCup.UI.Models;
using ECC.DanceCup.UI.Utils.Extensions;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authorization;

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
            if (GetLastTournament.IsFailed)
            {
                return Error("вместо последнего турнира тебе шиш"); //добавить нормальную обработку ошибки
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

    [Authorize]
    public async Task<IActionResult>  CreateTournament()
    {
        var getDancesResult = await _apiClient.GetDancesAsync(
            new GetDancesRequest(),
            HttpContext.RequestAborted
        );
        var getRefereesResult = await _apiClient.GetRefereesAsync(
            new GetRefereesRequest
            {
                PageNumber = 1,
                PageSize = 10
            },
            HttpContext.RequestAborted
        );
        if (getDancesResult.IsFailed)
        {
            return Error("dances");
        }
        if (getRefereesResult.IsFailed)
        {
            return Error("referees");
        }
        
        return View((getDancesResult.Value, getRefereesResult.Value));
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateTournament(string name, string description, DateTime date, List<CreateCategoryModel> categories)
    {
        var CreateTournament = await _apiClient.CreateTournamentAsync(
            new CreateTournamentRequest
            {
                UserId = _currentUser.Id,
                Name = name,
                Description = description,
                Date = date.ToUniversalTime().ToTimestamp(),
                CreateCategoryModels = { categories }
            },
            HttpContext.RequestAborted
        );

        if (CreateTournament.IsFailed)
        {
            return Error(CreateTournament.StringifyErrors()); //добавить нормальную обработку ошибки
        }
        else
        {
            return RedirectToAction("Index");
        }
    }


    public IActionResult Error(string text)
    {
        return View(text);
    }

    [Authorize]
    public async Task<IActionResult> Tournaments(int? pageNumber)
    {
        if (_currentUser.Authenticated)
        {
            var GetTournaments = await _apiClient.GetTournamentsAsync(
                new GetTournamentsRequest
                {
                    UserId = _currentUser.Id,
                    PageNumber = pageNumber??1,
                    PageSize = 10
                }
                ,
                HttpContext.RequestAborted
            );

            if (GetTournaments.IsFailed)
            {
                return Error("вместо последнего турнира тебе шиш"); //добавить нормальную обработку ошибки
            }

            var tournamentList = GetTournaments.Value.Tournaments;
                return View(tournamentList.ToList());

        }
        else
        {
            return RedirectToAction("Login", "User");
        }
    }

    [Authorize]
    public IActionResult CreateReferee()
    {
        return View();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateReferee(string refereeName)
    {
        var CreateRefereeResult = await _apiClient.CreateRefereeAsync(
            new CreateRefereeRequest
            {
                FullName = refereeName
            }
            ,HttpContext.RequestAborted
        );
        
        if (CreateRefereeResult.IsFailed)
        {
            return Error(CreateRefereeResult.StringifyErrors()); //добавить нормальную обработку ошибки
        }
        else
        {
            return RedirectToAction("Index");
        }
    }

    [Route("Home/Redactor/{id}")]
    public async Task<IActionResult> Redactor(long id)
    {
        var GetTournaments = await _apiClient.GetTournamentsAsync(
            new GetTournamentsRequest
            {
                UserId = _currentUser.Id,
                PageNumber = 1,
                PageSize = 10
            }
            ,
            HttpContext.RequestAborted
        );

        if (GetTournaments.IsFailed)
        {
            return Error("вместо последнего турнира тебе шиш"); //добавить нормальную обработку ошибки
        }

        var tournamentList = GetTournaments.Value.Tournaments;
        var tournament = tournamentList.Where(t => t.Id == id);
        if (tournament.Any())
        {
            return View(tournament.First());
        }
        else
        {
            return View(null);
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> StartRegistration(long tournamentId)
    {
        await _apiClient.StartTournamentRegistrationAsync(
            new StartTournamentRegistrationRequest
            {
                TournamentId = tournamentId
            },
            HttpContext.RequestAborted
        );
        return RedirectToAction("Index", new { id = tournamentId });
    }

    [HttpPost]
    public async Task<IActionResult> FinishRegistration(long tournamentId)
    {
        await _apiClient.FinishTournamentRegistrationAsync(
            new FinishTournamentRegistrationRequest
            {
                TournamentId = tournamentId
            },
            HttpContext.RequestAborted
        );
        return RedirectToAction("Index", new { id = tournamentId });
    }

    [HttpPost]
    public async Task<IActionResult> ReopenRegistration(long tournamentId)
    {
        await _apiClient.ReopenTournamentRegistrationAsync(
            new ReopenTournamentRegistrationRequest
            {
                TournamentId = tournamentId
            },
            HttpContext.RequestAborted
        );
        return RedirectToAction("Index", new { id = tournamentId });
    }

    [Route("Home/PairRegistration/{id}")]
    public IActionResult PairRegistration(long id)
    {
        ViewBag.TournamentId = id;
        return View();
    }
    
    public IActionResult Shablon()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> RegisterCouple(RegisterCoupleForTournamentRequest request)
    {
        await _apiClient.RegisterCoupleForTournamentAsync(request, HttpContext.RequestAborted);
        return RedirectToAction("RegistrationResult", new { id = request.TournamentId });
    }
    
    [Route("Home/RegistrationResult/{id}")]
    public async Task<IActionResult> RegistrationResult(long id)
    {
        var request = new GetTournamentRegistrationResultRequest { TournamentId = id };
        var response = await _apiClient.GetTournamentRegistrationResultAsync(request, HttpContext.RequestAborted);
        return View(response.Value);
    }
    
    [Route("Home/TournamentBracket/{id}")]
    public async Task<IActionResult> TournamentBracket(long id)
    {
        var request = new GetTournamentRegistrationResultRequest { TournamentId = id };
        var response = await _apiClient.GetTournamentRegistrationResultAsync(request, HttpContext.RequestAborted);

        if (response.IsFailed)
        {
            _logger.LogError("Ошибка при получении данных о регистрации на турнир: {Errors}", response.StringifyErrors());
            return Error("Не удалось получить данные о регистрации на турнир.");
        }

        return View(response.Value);
    }
}