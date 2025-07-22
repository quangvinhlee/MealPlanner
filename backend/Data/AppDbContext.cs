using MealPlannerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPlannerApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<FridgeItems> FridgeItems { get; set; }
        public DbSet<Recipes> Recipes { get; set; }
        public DbSet<ShoppingListItems> ShoppingListItems { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        public DbSet<MealPlan> MealPlans { get; set; }
        public DbSet<MealPlanDay> MealPlanDays { get; set; }
        public DbSet<MealPlanMeal> MealPlanMeals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<RecipeIngredient>()
                .HasKey(ri => new { ri.RecipeId, ri.IngredientId });

            modelBuilder.Entity<RecipeIngredient>()
                .HasOne(ri => ri.Recipe)
                .WithMany(r => r.RecipeIngredients)
                .HasForeignKey(ri => ri.RecipeId);

            modelBuilder.Entity<RecipeIngredient>()
                .HasOne(ri => ri.Ingredient)
                .WithMany(i => i.RecipeIngredients)
                .HasForeignKey(ri => ri.IngredientId);

            modelBuilder.Entity<MealPlan>()
                .HasOne(mp => mp.User)
                .WithMany(u => u.MealPlans)
                .HasForeignKey(mp => mp.UserId);

            modelBuilder.Entity<MealPlanDay>()
                .HasOne(d => d.MealPlan)
                .WithMany(mp => mp.Days)
                .HasForeignKey(d => d.MealPlanId);

            modelBuilder.Entity<MealPlanMeal>()
                .HasOne(m => m.MealPlanDay)
                .WithMany(d => d.Meals)
                .HasForeignKey(m => m.MealPlanDayId);
        }
    }
}