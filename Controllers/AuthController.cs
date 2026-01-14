using Microsoft.AspNetCore.Mvc;
using genial_dotnet_crm.Models;
using genial_dotnet_crm.Services;

namespace genial_dotnet_crm.Controllers;

public class AuthController : Controller
{
    private readonly ILogger<AuthController> _logger;
    private readonly IUserService _userService;
    private readonly IStageService _stageService;

    public AuthController(ILogger<AuthController> logger, IUserService userService, IStageService stageService)
    {
        _logger = logger;
        _userService = userService;
        _stageService = stageService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // Se já estiver autenticado, redirecionar
        if (IsAuthenticated())
        {
            return RedirectToLocal(returnUrl);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var isValid = await _userService.VerifyPasswordAsync(model.Email, model.Password);
            
            if (!isValid)
            {
                ModelState.AddModelError(string.Empty, "Email ou senha inválidos.");
                return View(model);
            }

            var user = await _userService.GetUserByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email ou senha inválidos.");
                return View(model);
            }

            // Criar sessão
            HttpContext.Session.SetString("UserId", user.Id);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("IsAuthenticated", "true");
            
            // Get default stage from database (first one by order)
            var stages = await _stageService.GetAllStagesAsync();
            var defaultStage = stages.FirstOrDefault()?.Key ?? "hml";
            HttpContext.Session.SetString("Stage", defaultStage);

            _logger.LogInformation("User {Email} logged in successfully with stage {Stage}", user.Email, defaultStage);

            return RedirectToLocal(returnUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao fazer login. Tente novamente.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Register()
    {
        // Se já estiver autenticado, redirecionar
        if (IsAuthenticated())
        {
            return RedirectToAction("Index", "Collections");
        }

        return View();
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Verificar se o email já existe
            var emailExists = await _userService.EmailExistsAsync(model.Email);
            if (emailExists)
            {
                ModelState.AddModelError(nameof(model.Email), "Este email já está cadastrado.");
                return View(model);
            }

            // Criar usuário
            var user = await _userService.CreateUserAsync(model.Email, model.Password);
            
            _logger.LogInformation("User {Email} registered successfully", user.Email);

            TempData["SuccessMessage"] = "Registro realizado com sucesso! Faça login para continuar.";
            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao criar a conta. Tente novamente.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        _logger.LogInformation("User logged out");
        return RedirectToAction(nameof(Login));
    }

    private bool IsAuthenticated()
    {
        return HttpContext.Session.GetString("IsAuthenticated") == "true";
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Collections");
    }
}

