using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MealPlannerApp.Models;
using MealPlannerApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MealPlannerApp.DTOs;
using System.ComponentModel;

namespace MealPlannerApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FridgeItemsController : ControllerBase
    {
        private readonly FridgeItemService _fridgeItemService;

        public FridgeItemsController(FridgeItemService fridgeItemService)
        {
            _fridgeItemService = fridgeItemService;
        }

        /// <summary>
        /// Add a fridge item
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<FridgeItemResponseDto>> AddFridgeItem([FromBody] FridgeItemCreateDto dto)
        {
            var userId = this.GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var response = await _fridgeItemService.CreateFridgeItemAsync(dto, userId.Value);
            return Ok(response);
        }

        /// <summary>
        /// Get all fridge items
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<FridgeItemResponseDto>>> GetFridgeItems()
        {
            var userId = this.GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var response = await _fridgeItemService.GetUserFridgeItemsAsync(userId.Value);
            return Ok(response);
        }

        /// <summary>
        /// Update a fridge item
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<FridgeItemResponseDto>> UpdateFridgeItem(Guid id, [FromBody] FridgeItemUpdateDto dto)
        {
            var userId = this.GetUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _fridgeItemService.UpdateFridgeItemAsync(id, dto, userId.Value);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        /// <summary>
        /// Get a specific fridge item by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<FridgeItemResponseDto>> GetFridgeItem(Guid id)
        {
            var userId = this.GetUserId();
            if (userId == null)
                return Unauthorized();

            var item = await _fridgeItemService.GetFridgeItemByIdAsync(id, userId.Value);
            if (item == null)
                return NotFound();

            return Ok(item);
        }

        /// <summary>
        /// Delete a fridge item
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteFridgeItem(Guid id)
        {
            var userId = this.GetUserId();
            if (userId == null)
                return Unauthorized();

            var deleted = await _fridgeItemService.DeleteFridgeItemAsync(id, userId.Value);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}