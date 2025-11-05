using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using ProductService.DTOs;
using ProductService.Services;

namespace ProductService.Controllers;


[ApiController]
[Route("api/products")]
public class ProductsController: ControllerBase
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
}
