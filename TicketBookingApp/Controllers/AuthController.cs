using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TicketBookingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpGet("user")]
        public IActionResult GetCurrentUser()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var name = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { message = "Not authenticated" });

            if (!Services.AuthorizationService.IsAuthorized(email))
                return Forbid("Your email is not authorized to access this app");

            return Ok(new { email, name });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request?.Email))
                return BadRequest(new { message = "Email is required" });

            if (!Services.AuthorizationService.IsAuthorized(request.Email))
                return Unauthorized(new { message = "Email not authorized to access this app" });

            // Return a simple token (in production, use JWT)
            var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(request.Email));
            return Ok(new { token, email = request.Email });
        }

        [HttpGet("verify")]
        public IActionResult VerifyToken()
        {
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader))
                return Unauthorized(new { message = "No token provided" });

            try
            {
                var token = authHeader.Replace("Bearer ", "");
                var email = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(token));

                if (!Services.AuthorizationService.IsAuthorized(email))
                    return Unauthorized(new { message = "Token invalid or email not authorized" });

                return Ok(new { email });
            }
            catch
            {
                return Unauthorized(new { message = "Invalid token" });
            }
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            // Frontend will handle logout by clearing tokens
            return Ok(new { message = "Logout successful" });
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
    }
}
