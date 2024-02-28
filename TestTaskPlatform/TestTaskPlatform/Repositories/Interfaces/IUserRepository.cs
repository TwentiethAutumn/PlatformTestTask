using TestTaskPlatform.Models;

namespace TestTaskPlatform.Repositories.Interfaces;

public interface IUserRepository
{
    Task<bool> Register(UserCredentials credentials);
    Task<string> GetToken(UserCredentials credentials);
    Task<bool> Authenticate(string username, string password);
}