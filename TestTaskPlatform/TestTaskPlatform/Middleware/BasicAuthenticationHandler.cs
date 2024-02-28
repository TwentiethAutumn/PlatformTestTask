using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using TestTaskPlatform.Repositories.Interfaces;

namespace TestTaskPlatform.Middleware;

public class BasicAuthenticationOptions : AuthenticationSchemeOptions { }

public class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
{
    private readonly IUserRepository _userRepository;
    
    public BasicAuthenticationHandler(
        IOptionsMonitor<BasicAuthenticationOptions> options, 
        ILoggerFactory logger, 
        UrlEncoder encoder, 
        ISystemClock clock,
        IUserRepository userRepository
        ) : base(options, logger, encoder, clock)
    {
        _userRepository = userRepository;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var endpoint = Context.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
        {
            return AuthenticateResult.NoResult();
        }
        
        var authorizationHeader = Request.Headers["Authorization"].ToString();
        Console.WriteLine(authorizationHeader);
        if (authorizationHeader != null && authorizationHeader.StartsWith("basic", StringComparison.OrdinalIgnoreCase))
        {
            var token = authorizationHeader.Substring("Basic ".Length).Trim();
            Console.WriteLine(token);
            var credentialsAsEncodedString = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var credentials = credentialsAsEncodedString.Split(':');
            if (await _userRepository.Authenticate(credentials[0], credentials[1]))
            {
                var claims = new[] { new Claim("name", credentials[0]), new Claim(ClaimTypes.Role, "User") };
                var identity = new ClaimsIdentity(claims, "Basic");
                var claimsPrincipal = new ClaimsPrincipal(identity);
                return await Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));
            }
        }
        
        Response.StatusCode = 401;
        Response.Headers.Add("WWW-Authenticate", "Basic realm=\"127.0.0.1\"");
        
        return await Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
    }
}