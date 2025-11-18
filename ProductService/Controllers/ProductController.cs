using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using ProductService.DTOs;
using ProductService.Services;
using Shared.Services;

namespace ProductService.Controllers;


[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productServices;
    // Extend services
    private readonly IRedisCacheService _cache;
    private readonly IElasticsearchService _elasticsearch;
    private readonly IMessageBusService _messageBus;
    private readonly IDistributedLockService _lockService;

    public ProductsController(
        IProductService productService,
        IRedisCacheService cache,
        IElasticsearchService elasticsearch,
        IMessageBusService messageBus,
        IDistributedLockService lockService
    )
    {
        _productServices = productService;
        _cache = cache;
        _elasticsearch = elasticsearch;
        _messageBus = messageBus;
        _lockService = lockService;
    }


    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {

        var cacheKey = "products:all";

        // Try cache first
        var cached = await _cache.GetAsync<List<ProductDto>>(cacheKey);
        if (cached != null)
        {
            Response.Headers["X-Cache"] = "HIT";
            return Ok(cached);
        }
        
        // Cache miss
        var products = await _productServices.GetAllProductsAsync();
        await _cache.SetAsync(cacheKey, products, TimeSpan.FromMinutes(5));

        Response.Headers["X-Cache"] = "MISS";
        return Ok(products);
    }


    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {

        var cacheKey = $"product:{id}";

        var cached = await _cache.GetAsync<ProductDto>(cacheKey);
        
        if (cached != null)
        {
            Response.Headers["X-Cache"] = "HIT";
            return Ok(cached);
        }


        var product = await _productServices.GetProductByIdAsync(id);

        if (product == null)
        {
            return NotFound(
                new
                {
                    message = "Product not found"
                }
            );
        }

        await _cache.SetAsync(cacheKey, product, TimeSpan.FromMinutes(10));
        Response.Headers["X-Cache"] = "MISS";

        return Ok(product);
    }


    [Authorize(Roles = "admin,staff")]
    [HttpPost]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductRequest request
    )
    {

        // Use distributed lock to prevent race conditions
        using var lockHandle = await _lockService.AcquireLockAsync( // the using holding the dispose for 30s
            $"product:create:{request.Barcode}",
            TimeSpan.FromSeconds(30)
        );

        if (lockHandle == null)
        {
            return Conflict(
                new
                {
                    message = "Another operation is in progress"
                }
            );
        }

        var product = await _productServices.CreateProductAsync(request);


        // index to elasticsearch
        await _elasticsearch.IndexProductAsync(new Models.Product
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            Barcode = product.Barcode,
            CategoryId = product.CategoryId,
            SupplierId = product.SupplierId,
            Price = product.Price,
            Unit = product.Unit,
            CreatedAt = product.CreatedAt
        });

        // Invalidate cache
        await _cache.RemoveAsync("products:all");

        return CreatedAtAction(nameof(GetProductById), new { id = product.ProductId }, product);
    }


    [Authorize(Roles = "admin,staff")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(
        int id,
        [FromBody] UpdateProductRequest request
    )
    {
        var product = await _productServices.UpdateProductAsync(id, request);

        if (product == null)
        {
            return NotFound(
                new
                {
                    message = "Product not found"
                }
            );
        }

        // Update elasticsearch
        await _elasticsearch.IndexProductAsync(new Models.Product
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            Barcode = product.Barcode,
            CategoryId = product.CategoryId,
            SupplierId = product.SupplierId,
            Price = product.Price,
            Unit = product.Unit,
            CreatedAt = product.CreatedAt
        });

        // Invalidate cache
        await _cache.RemoveAsync($"product:{id}");
        await _cache.RemoveAsync($"product:inventory:{id}");
        await _cache.RemoveAsync($"products:all");

        return Ok(product);
    }


    [Authorize(Roles = "admin,staff")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var result = await _productServices.DeleteProductAsync(id);

        if (!result)
        {
            return NotFound(
                new
                {
                    message = "Delete product unsuccessfully"
                }
            );
        }

        // Remove elasticsearch
        await _elasticsearch.DeleteProductAsync(id);

        // Invalidate cache
        await _cache.RemoveAsync($"product:{id}");
        await _cache.RemoveAsync("products:all");

        return Ok(
            new
            {
                message = "Delete Successfully"
            }
        );
    }


    // Extend
    [Authorize]
    [HttpGet("{id}/with-inventory")]
    public async Task<IActionResult> GetProductWithInventory(int id)
    {

        var cacheKey = $"product:inventory:{id}";

        var cached = await _cache.GetAsync<ProductWithInventoryDto>(cacheKey);

        if(cached != null)
        {
            return Ok(cached);
        }

        var product = await _productServices.GetProductWithInventoryAsync(id);

        if (product == null)
        {
            return NotFound(new { message = "Product not found" });
        }

        await _cache.SetAsync(cacheKey, product, TimeSpan.FromMinutes(2));

        return Ok(product);
    }


    [Authorize]
    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetProductsByCategory(int categoryId)
    {
        var products = await _productServices.GetProductsByCategoryAsync(categoryId);
        return Ok(products);
    }


    [Authorize]
    [HttpGet("supplier/{supplierId}")]
    public async Task<IActionResult> GetProductsBySupplier(int supplierId)
    {
        var products = await _productServices.GetProductsBySupplierAsync(supplierId);
        return Ok(products);
    }


    [Authorize]
    [HttpGet("search/{searchTerm}")]
    public async Task<IActionResult> SearchProducts(string searchTerm)
    {
        var products = await _elasticsearch.SearchProductsAsync(searchTerm); // elasticsearch for better search performance
        return Ok(products);
    }

    [Authorize]
    [HttpGet("search-elastic/{query}")]
    public async Task<IActionResult> SearchProductsElastic(string query)
    {
        var products = await _elasticsearch.SearchProductsAsync(query);
        return Ok(products);
    }

    [Authorize]
    [HttpGet("suggest/{query}")]
    public async Task<IActionResult> SuggestProducts(string query)
    {
        var suggestions = await _elasticsearch.SuggestProductsAsync(query);
        return Ok(suggestions);
    }


    [Authorize]
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockProducts(
        [FromQuery] int threshold = 10
    )
    {
        var products = await _productServices.GetLowStockProductsAsync(threshold);
        return Ok(products);
    }


    [Authorize]
    [HttpGet("stats")]
    public async Task<IActionResult> GetProductStats()
    {

        var cacheKey = "products:stats";

        var cached = await _cache.GetAsync<ProductStatsDto>(cacheKey);

        if (cached != null)
        {
            return Ok(cached);
        }

        var stats = await _productServices.GetProductStatsAsync();

        await _cache.SetAsync(cacheKey, stats, TimeSpan.FromMinutes(5));

        return Ok(stats);
    }
}
