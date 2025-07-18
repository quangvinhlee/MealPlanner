
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
        public async Task<ActionResult> GoogleLogin([FromBody] GoogleLoginDto dto, [FromServices] IConfiguration config)
        {
            var user = await _userService.LoginOrRegisterGoogleUser(dto.GoogleId, dto.Name, dto.Email, dto.AvatarUrl);

            // Generate JWT
            var token = _userService.GenerateJwtToken(user, config);

            // Set JWT as HTTP-only cookie
            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Set to true in production (requires HTTPS)
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            // Return token in response as well (optional)
            return Ok(new { token });
        }


    }
}