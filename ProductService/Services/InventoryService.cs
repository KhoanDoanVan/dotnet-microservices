using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;
using ProductService.DTOs;

namespace ProductService.Services;


public interface IInventoryService
{
    Task<List<InventoryDto>> GetAllInventoriesAsync();
    Task<InventoryDto?> GetInventoryByProductIdAsync(int productId);
    Task<InventoryDto> CreateOrUpdateInventoryAsync(int productId, UpdateInventoryRequest request);
    Task<InventoryDto?> AdjustInventoryAsync(int productId, AdjustInventoryRequest request);
    Task<bool> DeleteInventoryAsync(int productId);
}


public class InventoryService: IInventoryService
{
    private readonly ProductDbContext _context;

    public InventoryService(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<List<InventoryDto>> GetAllInventoriesAsync()
    {
        var inventories = await _context.Inventories.ToListAsync();
        return inventories.Select(MapToDto).ToList();
    }


    public async Task<InventoryDto?> GetInventoryByProductIdAsync(int productId)
    {
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == productId);

        return inventory != null ? MapToDto(inventory) : null;
    }


    public async Task<InventoryDto> CreateOrUpdateInventoryAsync(int productId, UpdateInventoryRequest request)
    {
        // Check if product exists
        var productExists = await _context.Products.AnyAsync(p => p.ProductId == productId);

        if (!productExists)
        {
            throw new Exception($"Product with ID {productId} not found");
        }

        var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);

        if (inventory == null)
        {
            // Create new inventory
            inventory = new Inventory
            {
                ProductId = productId,
                Quantity = request.Quantity,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Inventories.Add(inventory);
        }
        else
        {
            // Update existing inventory
            inventory.Quantity = request.Quantity;
            inventory.UpdatedAt = DateTime.UtcNow;
        }


        await _context.SaveChangesAsync();

        return MapToDto(inventory);
    }


    public async Task<InventoryDto?> AdjustInventoryAsync(int productId, AdjustInventoryRequest request)
    {
        var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);

        if (inventory == null)
        {
            // Check if product exists
            var productExists = await _context.Products.AnyAsync(p => p.ProductId == productId);

            if (!productExists)
            {
                return null;
            }


            // Create new inventory with adjustment
            inventory = new Inventory
            {
                ProductId = productId,
                Quantity = Math.Max(0, request.AdjustmentQuantity),
                UpdatedAt = DateTime.UtcNow
            };

            _context.Inventories.Add(inventory);
        }
        else
        {
            // Adjust existing inventory
            inventory.Quantity += request.AdjustmentQuantity;

            // Ensure quantity doesn't go negative
            if (inventory.Quantity < 0)
            {
                inventory.Quantity = 0;
            }

            inventory.UpdatedAt = DateTime.UtcNow;
        }


        await _context.SaveChangesAsync();

        return MapToDto(inventory);
    }


    public async Task<bool> DeleteInventoryAsync(int productId)
    {
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == productId);

        if (inventory == null)
        {
            return false;
        }

        _context.Inventories.Remove(inventory);
        await _context.SaveChangesAsync();

        return true;
    }


    private InventoryDto MapToDto(Inventory inventory)
    {
        return new InventoryDto
        {
            InventoryId = inventory.InventoryId,
            ProductId = inventory.ProductId,
            Quantity = inventory.Quantity,
            UpdatedAt = inventory.UpdatedAt
        };
    }
}