using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MealPlannerApp.Data;

namespace MealPlannerApp.Services
{
    public class RecipesService
    {
        private readonly AppDbContext _context;

        public RecipesService(AppDbContext context)
        {
            _context = context;
        }




    }
}