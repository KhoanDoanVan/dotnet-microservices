using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using ProductService.DTOs;
using ProductService.Services;

namespace ProductService.Controllers;


[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productServices;

    public ProductsController(IProductService productService)
    {
        _productServices = productService;
    }


    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {
        var products = await _productServices.GetAllProductsAsync();
        return Ok(products);
    }


    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
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

        return Ok(product);
    }


    [Authorize(Roles = "admin,staff")]
    [HttpPost]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductRequest request
    )
    {
        var product = await _productServices.CreateProductAsync(request);
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
        var product = await _productServices.GetProductWithInventoryAsync(id);

        if (product == null)
        {
            return NotFound(new { message = "Product not found" });
        }

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
        var products = await _productServices.SearchProductsAsync(searchTerm);
        return Ok(products);
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
        var stats = await _productServices.GetProductStatsAsync();
        return Ok(stats);
    }
}
