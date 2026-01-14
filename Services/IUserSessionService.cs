namespace genial_dotnet_crm.Services;

public interface IUserSessionService
{
    bool IsAuthenticated();
    string GetUserId();
    string GetUserEmail();
    string GetStage();
    void SetUserId(string userId);
    void SetUserEmail(string email);
    void SetIsAuthenticated(bool isAuthenticated);
    void SetStage(string stage);
    void Clear();
}

