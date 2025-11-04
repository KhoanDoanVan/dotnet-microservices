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