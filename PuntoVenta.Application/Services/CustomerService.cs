using Mapster;
using PuntoVenta.Application.DTOs.Common;
using PuntoVenta.Application.DTOs.Customer;
using PuntoVenta.Application.Interfaces.Repositories;
using PuntoVenta.Application.Interfaces.Services;
using PuntoVenta.Domain.Entities;

namespace PuntoVenta.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllAsync()
    {
        var customers = await _customerRepository.GetAllAsync();
        return customers.Adapt<IEnumerable<CustomerDto>>();
    }

    public async Task<IEnumerable<CustomerDto>> SearchByLastNameAsync(string lastName)
    {
        var matchingCustomers = await _customerRepository.SearchByLastNameAsync(lastName);
        return matchingCustomers.Adapt<IEnumerable<CustomerDto>>();
    }

    public async Task<CustomerDto?> GetByIdAsync(int customerId)
    {
        var existingCustomer = await _customerRepository.GetByIdAsync(customerId);
        return existingCustomer?.Adapt<CustomerDto>();
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto createCustomerDto)
    {
        var newCustomer = Customer.Create(
            createCustomerDto.DocumentNumber,
            createCustomerDto.FirstName,
            createCustomerDto.LastName,
            createCustomerDto.Phone,
            createCustomerDto.Address,
            createCustomerDto.City,
            createCustomerDto.Email);

        var savedCustomer = await _customerRepository.AddAsync(newCustomer);
        return savedCustomer.Adapt<CustomerDto>();
    }

    public async Task<bool> UpdateAsync(int customerId, UpdateCustomerDto dto, string? modifiedBy = null)
    {
        var existingCustomer = await _customerRepository.GetByIdAsync(customerId);
        if (existingCustomer == null)
            return false;

        existingCustomer.Update(
            dto.DocumentNumber,
            dto.FirstName,
            dto.LastName,
            dto.Phone,
            dto.Address,
            dto.City,
            dto.Email,
            modifiedBy);

        return await _customerRepository.UpdateAsync(existingCustomer);
    }

    public async Task<bool> ActivateAsync(int customerId)
        => await _customerRepository.ActivateAsync(customerId);

    public async Task<bool> DeactivateAsync(int customerId)
        => await _customerRepository.DeactivateAsync(customerId);

    public async Task<PagedResult<CustomerDto>> SearchPagedAsync(
        int?    customerId,
        string? documentNumber,
        string? lastName,
        int     page,
        int     pageSize,
        bool    onlyActive = false)
    {
        var (items, totalCount) = await _customerRepository.SearchPagedAsync(customerId, documentNumber, lastName, page, pageSize, onlyActive);
        var customerDtos        = items.Adapt<IEnumerable<CustomerDto>>();
        return PagedResult<CustomerDto>.Create(customerDtos, totalCount, page, pageSize);
    }

    public async Task<DeleteResultDto> DeleteAsync(int customerId)
    {
        var existingCustomer = await _customerRepository.GetByIdAsync(customerId);
        if (existingCustomer == null)
        {
            return new DeleteResultDto
            {
                Success = false,
                Message = "Cliente no encontrado."
            };
        }

        var hasSales = await _customerRepository.HasSalesAsync(customerId);
        if (hasSales)
        {
            var deactivated = await _customerRepository.DeactivateAsync(customerId);
            return new DeleteResultDto
            {
                Success = deactivated,
                IsLogicalDelete = true,
                Message = $"El cliente '{existingCustomer.FullName}' tiene historial de ventas asociado. Se ha marcado como inactivo (eliminación lógica)."
            };
        }
        else
        {
            var deleted = await _customerRepository.PhysicalDeleteAsync(customerId);
            return new DeleteResultDto
            {
                Success = deleted,
                IsLogicalDelete = false,
                Message = $"El cliente '{existingCustomer.FullName}' fue eliminado físicamente con éxito del sistema."
            };
        }
    }
}