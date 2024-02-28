using System.Text;
using TestTaskPlatform.Context;
using TestTaskPlatform.Models;
using TestTaskPlatform.Repositories.Interfaces;

namespace TestTaskPlatform.Repositories;

public class UserRepository : IUserRepository
{
    private ApiContext _context;

    public UserRepository(ApiContext context)
    {
        _context = context;
    }
    
    public async Task<bool> Register(UserCredentials credentials)
    {
        var user = _context.Users.FirstOrDefault(x => x.Username == credentials.Username);
        if (user is null)
        {
            var toCreate = new User
            {
                Username = credentials.Username,
                Password = credentials.Password
            };
            _context.Users.Add(toCreate);
            await _context.SaveChangesAsync();

            return true;
        }

        return false;
    }
    
    public async Task<string> GetToken(UserCredentials credentials)
    {
        return await Task.FromResult(
            $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{credentials.Username}:{credentials.Password}"))}"
        );
    }
    
    public async Task<bool> Authenticate(string username, string password)
    {
        if(await Task.FromResult(_context.Users.SingleOrDefault(x => x.Username == username && x.Password == password)) != null)
        {
            return true;
        }
        return false;
    }
}