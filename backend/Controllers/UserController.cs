
using MealPlannerApp.DTOs;
using MealPlannerApp.Models;
using MealPlannerApp.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace MealPlannerApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<ActionResult> GoogleLogin([FromBody] UserLoginDto dto, [FromServices] IConfiguration config)
        {
            var user = await _userService.LoginOrRegisterGoogleUser(dto.GoogleId, dto.Name, dto.Email, dto.AvatarUrl);

            // Generate JWT
            var token = _userService.GenerateJwtToken(user, config);

            // Set JWT as HTTP-only cookie
            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Set to false for HTTP in development
                SameSite = SameSiteMode.Lax, // More permissive for development
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            // Return token in response as well (optional)
            return Ok(new { token });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<User>> GetCurrentUser()
        {
            try
            {
                // Get user ID from JWT token claims - try multiple claim types
                var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub) ??
                                 User.FindFirst(ClaimTypes.NameIdentifier) ??
                                 User.FindFirst("sub");

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    return Unauthorized("Invalid token - user ID not found in claims");
                }

                var user = await _userService.GetUserById(userId);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return NotFound($"User not found: {ex.Message}");
            }
        }
    }
}