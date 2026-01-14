namespace genial_dotnet_crm.Helpers;

public static class AuthHelper
{
    public static bool IsAuthenticated(HttpContext context)
    {
        return context.Session.GetString("IsAuthenticated") == "true";
    }

    public static string? GetUserId(HttpContext context)
    {
        return context.Session.GetString("UserId");
    }

    public static string? GetUserEmail(HttpContext context)
    {
        return context.Session.GetString("UserEmail");
    }

    public static string GetStage(HttpContext context)
    {
        return context.Session.GetString("Stage") ?? "hml";
    }

    public static void SetStage(HttpContext context, string stage)
    {
        context.Session.SetString("Stage", stage);
    }
}

