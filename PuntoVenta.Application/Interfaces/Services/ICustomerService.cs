using PuntoVenta.Application.DTOs.Common;
using PuntoVenta.Application.DTOs.Customer;

namespace PuntoVenta.Application.Interfaces.Services;

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllAsync();
    Task<IEnumerable<CustomerDto>> SearchByLastNameAsync(string lastName);
    Task<CustomerDto?>             GetByIdAsync(int customerId);
    Task<CustomerDto>              CreateAsync(CreateCustomerDto createCustomerDto);
    Task<bool>                     UpdateAsync(int customerId, UpdateCustomerDto dto, string? modifiedBy = null);
    Task<bool>                     ActivateAsync(int customerId);
    Task<bool>                     DeactivateAsync(int customerId);
    Task<DeleteResultDto>          DeleteAsync(int customerId);

    /// <summary>
    /// Returns a paginated subset of customers filtered by optional customerId (exact) or lastName (LIKE).
    /// </summary>
    Task<PagedResult<CustomerDto>> SearchPagedAsync(
        int?    customerId,
        string? documentNumber,
        string? lastName,
        int     page,
        int     pageSize,
        bool    onlyActive = false);
}