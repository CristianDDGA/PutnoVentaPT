using PuntoVenta.Domain.Entities;

namespace PuntoVenta.Application.Interfaces.Repositories;

public interface ICustomerRepository
{
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<IEnumerable<Customer>> SearchByLastNameAsync(string lastName);
    Task<Customer?>             GetByIdAsync(int customerId);
    Task<Customer>              AddAsync(Customer customer);
    Task<bool>                  UpdateAsync(Customer customer);
    Task<bool>                  ActivateAsync(int customerId);
    Task<int> GetTotalCustomersCountAsync();
    Task<bool>                  DeactivateAsync(int customerId);
    Task<bool>                  HasSalesAsync(int customerId);
    Task<bool>                  PhysicalDeleteAsync(int customerId);

    /// <summary>
    /// Returns a paginated and optionally filtered list of customers.
    /// Filtering by customerId takes exact-match priority; lastName uses LIKE (index: inx_apellido).
    /// </summary>
    Task<(IEnumerable<Customer> Items, int TotalCount)> SearchPagedAsync(
        int?    customerId,
        string? documentNumber,
        string? lastName,
        int     page,
        int     pageSize,
        bool    onlyActive = false);
}