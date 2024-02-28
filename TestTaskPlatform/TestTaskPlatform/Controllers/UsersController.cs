using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestTaskPlatform.Models;
using TestTaskPlatform.Repositories.Interfaces;

namespace TestTaskPlatform.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult> Register([FromBody] UserCredentials credentials)
    {
        if (await _userRepository.Register(credentials))
        {
            return Ok();
        }

        return Conflict();
    }

    [HttpPost("token")]
    [AllowAnonymous]
    public async Task<string> GetToken([FromBody] UserCredentials credentials)
    {
        return await _userRepository.GetToken(credentials);
    }
}