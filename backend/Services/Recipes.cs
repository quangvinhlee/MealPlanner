using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MealPlannerApp.Data;

namespace MealPlannerApp.Services
{
    public class Recipes
    {
        private readonly AppDbContext _context;

        public Recipes(AppDbContext context)
        {
            _context = context;
        }




    }
}