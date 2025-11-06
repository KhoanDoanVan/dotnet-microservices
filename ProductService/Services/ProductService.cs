using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;
using ProductService.DTOs;


namespace ProductService.Services;


public interface IProductService
{
    Task<List<ProductDto>> GetAllProductsAsync();
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<ProductDto> CreateProductAsync(CreateProductRequest request);
    Task<ProductDto?> UpdateProductAsync(int id, UpdateProductRequest request);
    Task<bool> DeleteProductAsync(int id);
    

    // Extend
    Task<List<ProductDto>> GetProductsByCategoryAsync(int categoryId);
    Task<List<ProductDto>> GetProductsBySupplierAsync(int supplierId);
    Task<List<ProductDto>> SearchProductsAsync(string searchTerm);
    Task<List<ProductDto>> GetLowStockProductsAsync(int threshold);
    Task<ProductWithInventoryDto?> GetProductWithInventoryAsync(int id);
    Task<ProductStatsDto> GetProductStatsAsync();
}



public class ProductService: IProductService
{
    private readonly ProductDbContext _context;

    public ProductService(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> GetAllProductsAsync()
    {
        var products = await _context.Products.ToListAsync();
        return products.Select(MapToDto).ToList();
    }


    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        return product != null ? MapToDto(product) : null;
    }


    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request)
    {
        var product = new Product
        {
            CategoryId = request.CategoryId,
            SupplierId = request.SupplierId,
            ProductName = request.ProductName,
            Barcode = request.Barcode,
            Price = request.Price,
            Unit = request.Unit,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        return MapToDto(product);
    }


    public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductRequest request)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return null;
        }

        product.CategoryId = request.CategoryId;
        product.SupplierId = request.SupplierId;
        product.ProductName = request.ProductName;
        product.Barcode = request.Barcode;
        product.Price = request.Price;
        product.Unit = request.Unit;

        await _context.SaveChangesAsync();

        return MapToDto(product);
    }


    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return false;
        }

        _context.Products.Remove(product);

        await _context.SaveChangesAsync();

        return true;
    }


    // Extend
    public async Task<List<ProductDto>> GetProductsByCategoryAsync(int categoryId)
    {
        var products = await _context.Products
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();

        return products.Select(MapToDto).ToList();
    }


    public async Task<List<ProductDto>> GetProductsBySupplierAsync(int supplierId)
    {
        var products = await _context.Products
            .Where(p => p.SupplierId == supplierId)
            .ToListAsync();

        return products.Select(MapToDto).ToList();
    }


    public async Task<List<ProductDto>> SearchProductsAsync(string searchTerm)
    {
        var products = await _context.Products
            .Where(p => p.ProductName.Contains(searchTerm) ||
                       (p.Barcode != null && p.Barcode.Contains(searchTerm)))
            .ToListAsync();

        return products.Select(MapToDto).ToList();
    }


    public async Task<List<ProductDto>> GetLowStockProductsAsync(int threshold)
    {
        var products = await _context.Products
            .Join(_context.Inventories,
                p => p.ProductId,
                i => i.ProductId,
                (p, i) => new { Product = p, Inventory = i })
            .Where(pi => pi.Inventory.Quantity <= threshold)
            .Select(pi => pi.Product)
            .ToListAsync();

        return products.Select(MapToDto).ToList();
    }


    public async Task<ProductWithInventoryDto?> GetProductWithInventoryAsync(int id)
    {
        var product = await _context.Products
            .Where(p => p.ProductId == id)
            .Select(p => new ProductWithInventoryDto
            {
                ProductId = p.ProductId,
                CategoryId = p.CategoryId,
                SupplierId = p.SupplierId,
                ProductName = p.ProductName,
                Barcode = p.Barcode,
                Price = p.Price,
                Unit = p.Unit,
                CreatedAt = p.CreatedAt,
                StockQuantity = _context.Inventories
                    .Where(i => i.ProductId == p.ProductId)
                    .Select(i => (int?)i.Quantity)
                    .FirstOrDefault() ?? 0
            })
            .FirstOrDefaultAsync();

        return product;
    }


    public async Task<ProductStatsDto> GetProductStatsAsync()
    {
        var totalProducts = await _context.Products.CountAsync();
        var totalValue = await _context.Products
            .Join(_context.Inventories,
                p => p.ProductId,
                i => i.ProductId,
                (p, i) => p.Price * i.Quantity)
            .SumAsync();

        var lowStockCount = await _context.Inventories
            .Where(i => i.Quantity <= 10)
            .CountAsync();

        var outOfStockCount = await _context.Inventories
            .Where(i => i.Quantity == 0)
            .CountAsync();

        return new ProductStatsDto
        {
            TotalProducts = totalProducts,
            TotalInventoryValue = totalValue,
            LowStockProducts = lowStockCount,
            OutOfStockProducts = outOfStockCount
        };
    }


    private ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            ProductId = product.ProductId,
            CategoryId = product.CategoryId,
            SupplierId = product.SupplierId,
            ProductName = product.ProductName,
            Barcode = product.Barcode,
            Price = product.Price,
            Unit = product.Unit,
            CreatedAt = product.CreatedAt
        };
    }
}