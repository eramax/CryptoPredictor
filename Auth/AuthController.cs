using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    public record GetTokenRequest(string Username, string Password);

    [AllowAnonymous]
    [HttpPost("access_token")]
    public IActionResult Authorize([FromServices] JWTAuthenticationManager authenticationManager, [FromBody] GetTokenRequest request)
    {
        var token = authenticationManager.Authenticate(request.Username, request.Password);
        return token == null ? Unauthorized() : Ok(token);
    }
}
