using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.DTOs;
using OrderService.Services;

namespace OrderService.Controllers;


[ApiController]
[Route("api/promotions")]
[Authorize]
public class PromotionsController: ControllerBase
{
    private readonly IPromotionService _promotionService;

    public PromotionsController(IPromotionService promotionService)
    {
        _promotionService = promotionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPromotions()
    {
        var promotions = await _promotionService.GetAllPromotionsAsync();
        return Ok(promotions);
    }


    [HttpGet("active")]
    public async Task<IActionResult> GetActivePromotions()
    {
        var promotions = await _promotionService.GetActivePromotionsAsync();
        return Ok(promotions);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetPromotionById(int id)
    {
        var promotion = await _promotionService.GetPromotionByIdAsync(id);

        if (promotion == null)
        {
            return NotFound(new { message = "Promotion not found" });
        }

        return Ok(promotion);
    }

    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetPromotionByCode(string code)
    {
        var promotion = await _promotionService.GetPromotionByCodeAsync(code);

        if (promotion == null)
        {
            return NotFound(new { message = "Promotion not found" });
        }

        return Ok(promotion);
    }


    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> CreatePromotion([FromBody] CreatePromotionRequest request)
    {
        var promotion = await _promotionService.CreatePromotionAsync(request);
        return CreatedAtAction(nameof(GetPromotionById), new { id = promotion.PromoId }, promotion);
    }


    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePromotion(int id, [FromBody] UpdatePromotionRequest request)
    {
        var promotion = await _promotionService.UpdatePromotionAsync(id, request);

        if (promotion == null)
        {
            return NotFound(new { message = "Promotion not found" });
        }

        return Ok(promotion);
    }


    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePromotion(int id)
    {
        var result = await _promotionService.DeletePromotionAsync(id);

        if (!result)
        {
            return NotFound(new { message = "Promotion not found" });
        }

        return Ok(
            new
            {
                message = "Delete Successfully"
            }
        );
    }


    [HttpPost("validate")]
    public async Task<IActionResult> ValidatePromotion([FromBody] ValidatePromotionRequest request)
    {
        var result = await _promotionService.ValidatePromotionAsync(request);
        return Ok(result);
    }
}