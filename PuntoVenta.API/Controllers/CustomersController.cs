using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoVenta.Application.Constants;
using PuntoVenta.Application.DTOs.Customer;
using PuntoVenta.Application.Interfaces.Services;

namespace PuntoVenta.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Seller}")]
[AllowAnonymous]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService            _customerService;
    private readonly IValidator<CreateCustomerDto> _createCustomerValidator;
    private readonly IValidator<UpdateCustomerDto> _updateCustomerValidator;

    public CustomersController(
        ICustomerService             customerService,
        IValidator<CreateCustomerDto> createCustomerValidator,
        IValidator<UpdateCustomerDto> updateCustomerValidator)
    {
        _customerService         = customerService;
        _createCustomerValidator = createCustomerValidator;
        _updateCustomerValidator = updateCustomerValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var allCustomers = await _customerService.GetAllAsync();
        return Ok(allCustomers);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchByLastName([FromQuery] string lastName)
    {
        var matchingCustomers = await _customerService.SearchByLastNameAsync(lastName);
        return Ok(matchingCustomers);
    }

    /// <summary>
    /// Returns a paginated list of customers.
    /// When customerId is provided it takes precedence over lastName.
    /// </summary>
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int?    customerId     = null,
        [FromQuery] string? documentNumber = null,
        [FromQuery] string? lastName       = null,
        [FromQuery] int     page           = 1,
        [FromQuery] int     pageSize       = 10,
        [FromQuery] bool    onlyActive     = false)
    {
        if (page < 1)     page     = 1;
        if (pageSize < 1) pageSize = 10;

        var pagedResult = await _customerService.SearchPagedAsync(customerId, documentNumber, lastName, page, pageSize, onlyActive);
        return Ok(pagedResult);
    }

    [HttpGet("{customerId:int}")]
    public async Task<IActionResult> GetById(int customerId)
    {
        var existingCustomer = await _customerService.GetByIdAsync(customerId);

        if (existingCustomer is null)
            return NotFound($"Customer with id {customerId} not found.");

        return Ok(existingCustomer);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto createCustomerDto)
    {
        var validationResult = await _createCustomerValidator.ValidateAsync(createCustomerDto);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        var savedCustomer = await _customerService.CreateAsync(createCustomerDto);
        return CreatedAtAction(nameof(GetById), new { customerId = savedCustomer.CustomerId }, savedCustomer);
    }

    [HttpPut("{customerId:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Update(int customerId, [FromBody] UpdateCustomerDto updateCustomerDto)
    {
        var validationResult = await _updateCustomerValidator.ValidateAsync(updateCustomerDto);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        var modifiedBy = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? User.Identity?.Name;
        var success = await _customerService.UpdateAsync(customerId, updateCustomerDto, modifiedBy);
        return success ? NoContent() : NotFound($"Customer with id {customerId} not found.");
    }

    [HttpPut("{customerId:int}/activate")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Activate(int customerId)
    {
        var success = await _customerService.ActivateAsync(customerId);
        return success ? NoContent() : NotFound($"Customer with id {customerId} not found.");
    }

    [HttpPut("{customerId:int}/deactivate")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Deactivate(int customerId)
    {
        var success = await _customerService.DeactivateAsync(customerId);
        return success ? NoContent() : NotFound($"Customer with id {customerId} not found.");
    }

    [HttpDelete("{customerId:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(int customerId)
    {
        var result = await _customerService.DeleteAsync(customerId);
        if (!result.Success)
            return NotFound(result.Message);

        return Ok(result);
    }
}