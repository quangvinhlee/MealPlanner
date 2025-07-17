using MealPlannerApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MealPlannerApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DbCheckController : ControllerBase
    {
        private readonly AppDbContext _context;
        public DbCheckController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    await conn.OpenAsync();
                    await conn.CloseAsync();
                }
                return Ok(new { status = "Database connection successful" });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { status = "Database connection failed", error = ex.Message });
            }
        }
    }
} 