using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using genial_dotnet_crm.Models;
using genial_dotnet_crm.Services;

namespace genial_dotnet_crm.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUserSessionService _userSessionService;

    public HomeController(ILogger<HomeController> logger, IUserSessionService userSessionService)
    {
        _logger = logger;
        _userSessionService = userSessionService;
    }

    public IActionResult Index()
    {
        return RedirectToAction("Index", "Collections");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [Route("Home/Error/{statusCode}")]
    public IActionResult StatusCodeHandler(int statusCode)
    {
        // Se não estiver autenticado, redireciona para login
        if (!_userSessionService.IsAuthenticated())
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path });
        }

        if (statusCode == 404)
        {
            return View("NotFound");
        }

        return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}


