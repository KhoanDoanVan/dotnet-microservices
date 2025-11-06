using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using OrderService.DTOs;
using ProductService.DTOs;

namespace OrderService.Services;


public interface IPromotionService
{
    Task<List<PromotionDto>> GetAllPromotionsAsync();
    Task<List<PromotionDto>> GetActivePromotionsAsync();
    Task<PromotionDto?> GetPromotionByIdAsync(int id);
    Task<PromotionDto?> GetPromotionByCodeAsync(string code);
    Task<PromotionDto> CreatePromotionAsync(CreatePromotionRequest request);
    Task<PromotionDto?> UpdatePromotionAsync(int id, UpdatePromotionRequest request);
    Task<bool> DeletePromotionAsync(int id);
    Task<PromotionValidationResult> ValidatePromotionAsync(ValidatePromotionRequest request);
}


public class PromotionService: IPromotionService
{
    private readonly OrderDbContext _context;

    public PromotionService(OrderDbContext context)
    {
        _context = context;
    }


    public async Task<List<PromotionDto>> GetAllPromotionsAsync()
    {
        var promotions = await _context.Promotions.ToListAsync();
        return promotions.Select(MapToDto).ToList();
    }


    public async Task<List<PromotionDto>> GetActivePromotionsAsync()
    {
        var now = DateTime.UtcNow.Date;
        var promotions = await _context.Promotions
        .Where(p => p.Status == PromoStatus.Active && p.StartDate <= now && p.EndDate >= now)
        .ToListAsync();

        return promotions.Select(MapToDto).ToList();
    }

    public async Task<PromotionDto?> GetPromotionByIdAsync(int id)
    {
        var promotion = await _context.Promotions.FindAsync(id);
        return promotion != null ? MapToDto(promotion) : null;
    }


    public async Task<PromotionDto?> GetPromotionByCodeAsync(string code)
    {
        var promotion = await _context.Promotions
            .FirstOrDefaultAsync(p => p.PromoCode == code);
        return promotion != null ? MapToDto(promotion) : null;
    }


    public async Task<PromotionDto> CreatePromotionAsync(CreatePromotionRequest request)
    {
        if (!Enum.TryParse<DiscountType>(request.DiscountType, true, out var discountType))
        {
            discountType = DiscountType.Percent;
        }

        var promotion = new Promotion
        {
            PromoCode = request.PromoCode,
            Description = request.Description,
            DiscountType = discountType,
            DiscountValue = request.DiscountValue,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            MinOrderAmount = request.MinOrderAmount,
            UsageLimit = request.UsageLimit,
            UsedCount = 0,
            Status = PromoStatus.Active
        };

        _context.Promotions.Add(promotion);
        await _context.SaveChangesAsync();

        return MapToDto(promotion);
    }


    public async Task<PromotionDto?> UpdatePromotionAsync(int id, UpdatePromotionRequest request)
    {
        var promotion = await _context.Promotions.FindAsync(id);

        if (promotion == null)
        {
            return null;
        }

        if (!Enum.TryParse<DiscountType>(request.DiscountType, true, out var discountType))
        {
            discountType = DiscountType.Percent;
        }

        if (!Enum.TryParse<PromoStatus>(request.Status, true, out var status))
        {
            status = PromoStatus.Active;
        }

        promotion.Description = request.Description;
        promotion.DiscountType = discountType;
        promotion.DiscountValue = request.DiscountValue;
        promotion.StartDate = request.StartDate;
        promotion.EndDate = request.EndDate;
        promotion.MinOrderAmount = request.MinOrderAmount;
        promotion.UsageLimit = request.UsageLimit;
        promotion.Status = status;

        await _context.SaveChangesAsync();

        return MapToDto(promotion);
    }


    public async Task<bool> DeletePromotionAsync(int id)
    {
        var promotion = await _context.Promotions.FindAsync(id);

        if (promotion == null)
        {
            return false;
        }

        _context.Promotions.Remove(promotion);
        await _context.SaveChangesAsync();

        return true;
    }


    public async Task<PromotionValidationResult> ValidatePromotionAsync(ValidatePromotionRequest request)
    {
        var promotion = await _context.Promotions
            .FirstOrDefaultAsync(p => p.PromoCode == request.PromoCode);

        if (promotion == null)
        {
            return new PromotionValidationResult
            {
                IsValid = false,
                Message = "Promotion code not found",
                DiscountAmount = 0
            };
        }

        // Check if promotion is active
        if (promotion.Status != PromoStatus.Active)
        {
            return new PromotionValidationResult
            {
                IsValid = false,
                Message = "Promotion is not active",
                DiscountAmount = 0
            };
        }

        // Check date range
        var now = DateTime.UtcNow.Date;
        if (now < promotion.StartDate || now > promotion.EndDate)
        {
            return new PromotionValidationResult
            {
                IsValid = false,
                Message = "Promotion is not valid for current date",
                DiscountAmount = 0
            };
        }

        // Check minimum order amount
        if (request.OrderAmount < promotion.MinOrderAmount)
        {
            return new PromotionValidationResult
            {
                IsValid = false,
                Message = $"Order amount must be at least {promotion.MinOrderAmount}",
                DiscountAmount = 0
            };
        }

        // Check usage limit
        if (promotion.UsageLimit > 0 && promotion.UsedCount >= promotion.UsageLimit)
        {
            return new PromotionValidationResult
            {
                IsValid = false,
                Message = "Promotion usage limit reached",
                DiscountAmount = 0
            };
        }

        // Calculate discount
        decimal discountAmount = promotion.DiscountType == DiscountType.Percent
            ? request.OrderAmount * (promotion.DiscountValue / 100)
            : promotion.DiscountValue;

        // Ensure discount doesn't exceed order amount
        discountAmount = Math.Min(discountAmount, request.OrderAmount);

        return new PromotionValidationResult
        {
            IsValid = true,
            Message = "Promotion is valid",
            DiscountAmount = discountAmount,
            Promotion = MapToDto(promotion)
        };
    }


    private PromotionDto MapToDto(Promotion promotion)
    {
        return new PromotionDto
        {
            PromoId = promotion.PromoId,
            PromoCode = promotion.PromoCode,
            Description = promotion.Description,
            DiscountType = promotion.DiscountType.ToString().ToLower(),
            DiscountValue = promotion.DiscountValue,
            StartDate = promotion.StartDate,
            EndDate = promotion.EndDate,
            MinOrderAmount = promotion.MinOrderAmount,
            UsageLimit = promotion.UsageLimit,
            UsedCount = promotion.UsedCount,
            Status = promotion.Status.ToString().ToLower()
        };
    }
}
