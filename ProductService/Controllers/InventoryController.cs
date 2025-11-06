using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Services;

namespace ProductService.Controllers;


[ApiController]
[Route("api/inventory")]
[Authorize]
public class InventoryController: ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllInventories()
    {
        var inventories = await _inventoryService.GetAllInventoriesAsync();
        return Ok(inventories);
    }


    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetInventoryByProductId(int productId)
    {
        var inventory = await _inventoryService.GetInventoryByProductIdAsync(productId);

        if (inventory == null)
        {
            return NotFound(new { message = "Inventory not found for this product" });
        }

        return Ok(inventory);
    }


    [Authorize(Roles = "admin,staff")]
    [HttpPut("product/{productId}")]
    public async Task<IActionResult> UpdateInventory(int productId, [FromBody] UpdateInventoryRequest request)
    {
        try
        {
            var inventory = await _inventoryService.CreateOrUpdateInventoryAsync(productId, request);
            return Ok(inventory);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [Authorize(Roles = "admin,staff")]
    [HttpPost("product/{productId}/adjust")]
    public async Task<IActionResult> AdjustInventory(int productId, [FromBody] AdjustInventoryRequest request)
    {
        var inventory = await _inventoryService.AdjustInventoryAsync(productId, request);

        if (inventory == null)
        {
            return NotFound(new { message = "Product not found" });
        }

        return Ok(inventory);
    }


    [Authorize(Roles = "admin")]
    [HttpDelete("product/{productId}")]
    public async Task<IActionResult> DeleteInventory(int productId)
    {
        var result = await _inventoryService.DeleteInventoryAsync(productId);

        if (!result)
        {
            return NotFound(new { message = "Inventory not found" });
        }

        return NoContent();
    }
}