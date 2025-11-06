using Microsoft.EntityFrameworkCore;
using AuthService.Data;
using AuthService.Models;
using AuthService.DTOs;

namespace AuthService.Services;


public interface ICustomerService
{
    Task<List<CustomerDto>> GetAllCustomersAsync();
    Task<CustomerDto?> GetCustomerByIdAsync(int id);
    Task<List<CustomerDto>> SearchCustomersAsync(CustomerSearchRequest request);
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request);
    Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerRequest request);
    Task<bool> DeleteCustomerAsync(int id);
    Task<CustomerDto?> UpdateCustomerStatsAsync(int customerId, decimal orderAmount);
}


public class CustomerService: ICustomerService
{
    private readonly AuthDbContext _context;

    public CustomerService(AuthDbContext context)
    {
        _context = context;
    }


    public async Task<List<CustomerDto>> GetAllCustomersAsync()
    {
        var customers = await _context.Customers.ToListAsync();
        return customers.Select(MapToDto).ToList();
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        return customer != null ? MapToDto(customer) : null;
    }

    public async Task<List<CustomerDto>> SearchCustomersAsync(CustomerSearchRequest request)
    {
        var query = _context.Customers.AsQueryable();

        if (!string.IsNullOrEmpty(request.Name))
        {
            query = query.Where(c => c.Name.Contains(request.Name));
        }

        if (!string.IsNullOrEmpty(request.Phone))
        {
            query = query.Where(c => c.Phone != null && c.Phone.Contains(request.Phone));
        }

        if (!string.IsNullOrEmpty(request.Email))
        {
            query = query.Where(c => c.Email != null && c.Email.Contains(request.Email));
        }

        var customers = await query.ToListAsync();
        return customers.Select(MapToDto).ToList();
    }


    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request)
    {
        var customer = new Customer
        {
            Name = request.Name,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            CreatedAt = DateTime.UtcNow,
            TotalSpent = 0,
            OrderCount = 0,
            Tier = CustomerTier.Bronze
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return MapToDto(customer);
    }


    public async Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerRequest request)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
        {
            return null;
        }

        customer.Name = request.Name;
        customer.Phone = request.Phone;
        customer.Email = request.Email;
        customer.Address = request.Address;

        await _context.SaveChangesAsync();

        return MapToDto(customer);
    }


    public async Task<bool> DeleteCustomerAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
        {
            return false;
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return true;
    }


    public async Task<CustomerDto?> UpdateCustomerStatsAsync(int customerId, decimal orderAmount)
    {
        var customer = await _context.Customers.FindAsync(customerId);

        if (customer == null)
        {
            return null;
        }

        customer.TotalSpent += orderAmount;
        customer.OrderCount++;

        // Update tier based on total spent
        customer.Tier = customer.TotalSpent switch
        {
            >= 10000 => CustomerTier.Platinum,
            >= 5000 => CustomerTier.Gold,
            >= 1000 => CustomerTier.Silver,
            _ => CustomerTier.Bronze
        };

        await _context.SaveChangesAsync();

        return MapToDto(customer);
    }


    private CustomerDto MapToDto(Customer customer)
    {
        return new CustomerDto
        {
            CustomerId = customer.CustomerId,
            Name = customer.Name,
            Phone = customer.Phone,
            Email = customer.Email,
            Address = customer.Address,
            CreatedAt = customer.CreatedAt,
            TotalSpent = customer.TotalSpent,
            OrderCount = customer.OrderCount,
            Tier = customer.Tier.ToString().ToLower()
        };
    }
}