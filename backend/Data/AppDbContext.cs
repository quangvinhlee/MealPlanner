using MealPlannerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPlannerApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<FridgeItems> FridgeItems { get; set; }
        public DbSet<Recipes> Recipes { get; set; }
        public DbSet<ShoppingListItems> ShoppingListItems { get; set; }
        public DbSet<User> Users { get; set; }
    }
}