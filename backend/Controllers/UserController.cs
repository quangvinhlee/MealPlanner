
using MealPlannerApp.DTOs;
using MealPlannerApp.Models;
using MealPlannerApp.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

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
        public async Task<ActionResult<User>> GoogleLogin([FromBody] GoogleLoginDto dto)
        {
            // In production, verify the Google token here!
            var user = await _userService.LoginOrRegisterGoogleUser(dto.GoogleId, dto.Name, dto.Email, dto.AvatarUrl);
            return Ok(user);
        }

        [HttpGet("google-client-id")]
        public ActionResult<string> GetGoogleClientId([FromServices] IConfiguration config)
        {
            var clientId = config["Authentication:Google:ClientId"];
            Console.WriteLine($"[UserController] Google ClientId from config: {clientId}");
            return Ok(clientId);
        }
    }
}