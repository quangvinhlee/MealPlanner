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

namespace MealPlannerApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FridgeItemsController : AppControllerBase
    {
        private readonly FridgeItemsService _fridgeItemsService;
        public FridgeItemsController(FridgeItemsService fridgeItemsService)
        {
            _fridgeItemsService = fridgeItemsService;
        }

        [HttpPost]
        public async Task<ActionResult<FridgeItemResponseDto>> AddFridgeItem([FromBody] FridgeItemCreateDto dto)
        {
            if (UserId == null)
            {
                return Unauthorized();
            }
            var response = await _fridgeItemsService.AddFridgeItem(dto, UserId.Value);
            return Ok(response);
        }


        [HttpGet]
        public async Task<ActionResult<List<FridgeItemResponseDto>>> GetFridgeItems()
        {
            if (UserId == null)
            {
                return Unauthorized();
            }

            var response = await _fridgeItemsService.GetFridgeItemResponsesByUserId(UserId.Value);
            return Ok(response);
        }
    }
}