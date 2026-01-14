using Microsoft.AspNetCore.Http;

namespace genial_dotnet_crm.Services;

public class UserSessionService : IUserSessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string IsAuthenticatedKey = "IsAuthenticated";
    private const string UserIdKey = "UserId";
    private const string UserEmailKey = "UserEmail";
    private const string StageKey = "Stage";
    private const string DefaultStage = "hml";

    public UserSessionService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession? Session => _httpContextAccessor.HttpContext?.Session;

    public bool IsAuthenticated()
    {
        return Session?.GetString(IsAuthenticatedKey) == "true";
    }

    public string GetUserId()
    {
        return Session?.GetString(UserIdKey) ?? string.Empty;
    }

    public string GetUserEmail()
    {
        return Session?.GetString(UserEmailKey) ?? string.Empty;
    }

    public string GetStage()
    {
        return Session?.GetString(StageKey) ?? DefaultStage;
    }

    public void SetUserId(string userId)
    {
        Session?.SetString(UserIdKey, userId);
    }

    public void SetUserEmail(string email)
    {
        Session?.SetString(UserEmailKey, email);
    }

    public void SetIsAuthenticated(bool isAuthenticated)
    {
        Session?.SetString(IsAuthenticatedKey, isAuthenticated ? "true" : "false");
    }

    public void SetStage(string stage)
    {
        Session?.SetString(StageKey, stage);
    }

    public void Clear()
    {
        Session?.Clear();
    }
}

