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
        private readonly FridgeItemsService _fridgeItemsService;
        public FridgeItemsController(FridgeItemsService fridgeItemsService)
        {
            _fridgeItemsService = fridgeItemsService;
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
            var response = await _fridgeItemsService.AddFridgeItem(dto, userId.Value);
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

            var response = await _fridgeItemsService.GetFridgeItemResponsesByUserId(userId.Value);
            return Ok(response);
        }

        /// <summary>
        /// Update a fridge item
        /// </summary>
        [HttpPut]
        public async Task<ActionResult<FridgeItemResponseDto>> UpdateFridgeItem([FromBody] FridgeItemUpdateDto dto)
        {
            var userId = this.GetUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _fridgeItemsService.UpdateFridgeItem(userId.Value, dto);
            if (result == null)
                return NotFound();

            return Ok(result);
        }
    }
}