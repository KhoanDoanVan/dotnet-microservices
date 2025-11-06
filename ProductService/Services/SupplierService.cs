using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;
using ProductService.DTOs;

namespace ProductService.Services;


public interface ISupplierService
{
    Task<List<SupplierDto>> GetAllSuppliersAsync();
    Task<SupplierDto?> GetSupplierByIdAsync(int id);
    Task<SupplierDto> CreateSupplierAsync(CreateSupplierRequest request);
    Task<SupplierDto?> UpdateSupplierAsync(int id, UpdateSupplierRequest request);
    Task<bool> DeleteSupplierAsync(int id);
}


public class SupplierService: ISupplierService
{
    private readonly ProductDbContext _context;

    public SupplierService(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<List<SupplierDto>> GetAllSuppliersAsync()
    {
        var suppliers = await _context.Suppliers.ToListAsync();
        return suppliers.Select(MapToDto).ToList();
    }

    public async Task<SupplierDto?> GetSupplierByIdAsync(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        return supplier != null ? MapToDto(supplier) : null;
    }

    public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierRequest request)
    {
        var supplier = new Supplier
        {
            Name = request.Name,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address
        };

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        return MapToDto(supplier);
    }

    public async Task<SupplierDto?> UpdateSupplierAsync(int id, UpdateSupplierRequest request)
    {
        var supplier = await _context.Suppliers.FindAsync(id);

        if (supplier == null)
        {
            return null;
        }

        supplier.Name = request.Name;
        supplier.Phone = request.Phone;
        supplier.Email = request.Email;
        supplier.Address = request.Address;

        await _context.SaveChangesAsync();

        return MapToDto(supplier);
    }


    public async Task<bool> DeleteSupplierAsync(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);

        if (supplier == null)
        {
            return false;
        }

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync();

        return true;
    }


    private SupplierDto MapToDto(Supplier supplier)
    {
        return new SupplierDto
        {
            SupplierId = supplier.SupplierId,
            Name = supplier.Name,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Address = supplier.Address
        };
    }
}