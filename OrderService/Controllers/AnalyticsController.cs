using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Services;

namespace OrderService.Controllers;



[ApiController]
[Route("api/analytics")]
[Authorize(Roles = "admin")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }


    [HttpGet("sales-report")]
    public async Task<IActionResult> GetSalesReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var report = await _analyticsService.GetSalesReportAsync(startDate, endDate);
        return Ok(report);
    }


    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts(
        [FromQuery] int count = 10,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var products = await _analyticsService.GetTopProductsAsync(count, startDate, endDate);
        return Ok(products);
    }


    [HttpGet("revenue-by-date")]
    public async Task<IActionResult> GetRevenueByDate([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var revenue = await _analyticsService.GetRevenueByDateAsync(startDate, endDate);
        return Ok(revenue);
    }


    [HttpGet("payment-methods")]
    public async Task<IActionResult> GetPaymentMethodStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var stats = await _analyticsService.GetPaymentMethodStatsAsync(startDate, endDate);
        return Ok(stats);
    }


    [HttpGet("promotion-effectiveness")]
    public async Task<IActionResult> GetPromotionEffectiveness(
        [FromQuery] int? promoId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var effectiveness = await _analyticsService.GetPromotionEffectivenessAsync(promoId, startDate, endDate);
        return Ok(effectiveness);
    }
}