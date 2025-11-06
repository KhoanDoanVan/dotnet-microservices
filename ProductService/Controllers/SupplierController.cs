
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Services;

namespace ProductService.Controllers;



[ApiController]
[Route("api/suppliers")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAllSuppliers()
    {
        var suppliers = await _supplierService.GetAllSuppliersAsync();
        return Ok(suppliers);
    }


    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSupplierById(int id)
    {
        var supplier = await _supplierService.GetSupplierByIdAsync(id);

        if (supplier == null)
        {
            return NotFound(new { message = "Supplier not found" });
        }

        return Ok(supplier);
    }


    [Authorize(Roles = "admin,staff")]
    [HttpPost]
    public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierRequest request)
    {
        var supplier = await _supplierService.CreateSupplierAsync(request);
        return CreatedAtAction(nameof(GetSupplierById), new { id = supplier.SupplierId }, supplier);
    }


    [Authorize(Roles = "admin,staff")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSupplier(int id, [FromBody] UpdateSupplierRequest request)
    {
        var supplier = await _supplierService.UpdateSupplierAsync(id, request);

        if (supplier == null)
        {
            return NotFound(new { message = "Supplier not found" });
        }

        return Ok(supplier);
    }


    [Authorize(Roles = "admin,staff")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
        var result = await _supplierService.DeleteSupplierAsync(id);

        if (!result)
        {
            return NotFound(new { message = "Supplier not found" });
        }

        return NoContent();
    }

}