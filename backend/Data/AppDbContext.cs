using MealPlannerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPlannerApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Ingredient> Ingredients { get; set; }
    }
} 