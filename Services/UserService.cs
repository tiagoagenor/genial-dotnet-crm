using MongoDB.Driver;
using genial_dotnet_crm.Data;
using genial_dotnet_crm.Models;
using BCrypt.Net;

namespace genial_dotnet_crm.Services;

public class UserService : IUserService
{
    private readonly IMongoCollection<User> _users;

    public UserService(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _users = database.GetCollection<User>("_users");
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _users.Find(u => u.Email == email.ToLowerInvariant()).FirstOrDefaultAsync();
    }

    public async Task<User> CreateUserAsync(string email, string password)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        
        var user = new User
        {
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _users.InsertOneAsync(user);
        return user;
    }

    public async Task<bool> VerifyPasswordAsync(string email, string password)
    {
        var user = await GetUserByEmailAsync(email);
        if (user == null)
            return false;

        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var user = await GetUserByEmailAsync(email);
        return user != null;
    }
}


