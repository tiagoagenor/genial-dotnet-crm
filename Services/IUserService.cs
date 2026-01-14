using genial_dotnet_crm.Models;

namespace genial_dotnet_crm.Services;

public interface IUserService
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(string email, string password);
    Task<bool> VerifyPasswordAsync(string email, string password);
    Task<bool> EmailExistsAsync(string email);
}




