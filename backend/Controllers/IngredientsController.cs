using MealPlannerApp.Models;
using MealPlannerApp.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace MealPlannerApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IngredientsController : ControllerBase
    {
        private readonly IngredientService _service;
        public IngredientsController(IngredientService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ingredient>>> GetAll()
        {
            var ingredients = await _service.GetAllAsync();
            return Ok(ingredients);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Ingredient>> GetById(Guid id)
        {
            var ingredient = await _service.GetByIdAsync(id);
            if (ingredient == null) return NotFound();
            return Ok(ingredient);
        }

        [HttpPost]
        public async Task<ActionResult<Ingredient>> Create(Ingredient ingredient)
        {
            var created = await _service.AddAsync(ingredient);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, Ingredient ingredient)
        {
            if (id != ingredient.Id) return BadRequest();
            var success = await _service.UpdateAsync(ingredient);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}