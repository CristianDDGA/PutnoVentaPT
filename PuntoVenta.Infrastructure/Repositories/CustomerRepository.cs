using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.Interfaces.Repositories;
using PuntoVenta.Domain.Entities;
using PuntoVenta.Infrastructure.Persistence;

namespace PuntoVenta.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _appDbContext;

    public CustomerRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    public async Task<int> GetTotalCustomersCountAsync()
    => await _appDbContext.Customers.AsNoTracking().CountAsync();
    public async Task<IEnumerable<Customer>> GetAllAsync()
        => await _appDbContext.Customers
            .AsNoTracking()
            .ToListAsync();

    public async Task<IEnumerable<Customer>> SearchByLastNameAsync(string lastName)
        => await _appDbContext.Customers
            .AsNoTracking()
            .Where(customer => customer.LastName.Contains(lastName))
            .ToListAsync();

    public async Task<Customer?> GetByIdAsync(int customerId)
        => await _appDbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(customer => customer.CustomerId == customerId);

    public async Task<Customer> AddAsync(Customer newCustomer)
    {
        await _appDbContext.Customers.AddAsync(newCustomer);
        await _appDbContext.SaveChangesAsync();
        return newCustomer;
    }

    public async Task<bool> UpdateAsync(Customer customer)
    {
        _appDbContext.Customers.Update(customer);
        var affectedRows = await _appDbContext.SaveChangesAsync();
        return affectedRows > 0;
    }

    public async Task<bool> ActivateAsync(int customerId)
        => await UpdateIsActiveAsync(customerId, true);

    public async Task<bool> DeactivateAsync(int customerId)
        => await UpdateIsActiveAsync(customerId, false);

    private async Task<bool> UpdateIsActiveAsync(int customerId, bool isActive)
    {
        var affectedRows = await _appDbContext.Customers
            .Where(customer => customer.CustomerId == customerId)
            .ExecuteUpdateAsync(update => update.SetProperty(customer => customer.IsActive, isActive));

        return affectedRows > 0;
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<Customer> Items, int TotalCount)> SearchPagedAsync(
        int?    customerId,
        string? documentNumber,
        string? lastName,
        int     page,
        int     pageSize,
        bool    onlyActive = false)
    {
        // Start from the full set (AsNoTracking for read-only queries)
        var query = _appDbContext.Customers.AsNoTracking();

        if (onlyActive)
            query = query.Where(customer => customer.IsActive);

        // Exact-match by primary key takes priority when supplied
        if (customerId.HasValue)
        {
            query = query.Where(customer => customer.CustomerId == customerId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(documentNumber))
        {
            query = query.Where(customer => customer.DocumentNumber.Contains(documentNumber));
        }
        else if (!string.IsNullOrWhiteSpace(lastName))
        {
            // LIKE search — EF Core translates Contains to SQL LIKE '%value%'.
            query = query.Where(customer => customer.LastName.Contains(lastName));
        }

        try
        {
            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(customer => customer.LastName)
                .ThenBy(customer  => customer.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
        catch (Exception)
        {
            // En entornos de desarrollo, si la base de datos no está disponible,
            // devolvemos un resultado vacío para que el front no se rompa.
            return (Enumerable.Empty<Customer>(), 0);
        }
    }

    public async Task<bool> HasSalesAsync(int customerId)
        => await _appDbContext.Sales.AnyAsync(s => s.CustomerId == customerId);

    public async Task<bool> PhysicalDeleteAsync(int customerId)
    {
        var customer = await _appDbContext.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId);
        if (customer == null) return false;

        _appDbContext.Customers.Remove(customer);
        var affected = await _appDbContext.SaveChangesAsync();
        return affected > 0;
    }
}
