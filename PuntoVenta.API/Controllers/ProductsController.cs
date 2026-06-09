using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoVenta.Application.Constants;
using PuntoVenta.Application.DTOs.Product;
using PuntoVenta.Application.Interfaces.Services;

namespace PuntoVenta.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Seller}")]
public class ProductsController : ControllerBase
{
    private readonly IProductService              _productService;
    private readonly IExcelExportService          _excelExportService;
    private readonly IValidator<CreateProductDto> _createProductValidator;

    public ProductsController(
        IProductService              productService,
        IExcelExportService          excelExportService,
        IValidator<CreateProductDto> createProductValidator)
    {
        _productService         = productService;
        _excelExportService     = excelExportService;
        _createProductValidator = createProductValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var allProducts = await _productService.GetAllAsync();
        return Ok(allProducts);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchByName([FromQuery] string name)
    {
        var matchingProducts = await _productService.SearchByNameAsync(name);
        return Ok(matchingProducts);
    }

    /// <summary>
    /// Returns a paginated list of products.
    /// When productId is provided it takes precedence over name.
    /// </summary>
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int?    productId   = null,
        [FromQuery] string? name        = null,
        [FromQuery] int     page        = 1,
        [FromQuery] int     pageSize    = 10,
        [FromQuery] bool    onlyInStock = false,
        [FromQuery] bool    onlyActive  = false)
    {
        if (page < 1)     page     = 1;
        if (pageSize < 1) pageSize = 10;

        var pagedResult = await _productService.SearchPagedAsync(productId, name, page, pageSize, onlyInStock, onlyActive);
        return Ok(pagedResult);
    }

    [HttpGet("export/excel")]
    public async Task<IActionResult> ExportExcel(
        [FromQuery] string? name        = null,
        [FromQuery] bool    onlyInStock = false,
        [FromQuery] bool    onlyActive  = false)
    {
        var pagedResult = await _productService.SearchPagedAsync(
            productId: null, name, page: 1, pageSize: 10_000, onlyInStock, onlyActive);

        var excelBytes = _excelExportService.ExportProducts(pagedResult.Items);
        var fileName   = $"Productos_{DateTime.UtcNow:yyyyMMdd_HHmm}.xlsx";

        return File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    [HttpGet("{productId:int}")]
    public async Task<IActionResult> GetById(int productId)
    {
        var existingProduct = await _productService.GetByIdAsync(productId);

        if (existingProduct is null)
            return NotFound($"Product with id {productId} not found.");

        return Ok(existingProduct);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateProductDto createProductDto)
    {
        var validationResult = await _createProductValidator.ValidateAsync(createProductDto);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        var savedProduct = await _productService.CreateAsync(createProductDto);
        return CreatedAtAction(nameof(GetById), new { productId = savedProduct.ProductId }, savedProduct);
    }

    [HttpPut("{productId:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Update(int productId, [FromBody] UpdateProductDto updateProductDto)
    {
        // FluentValidation via inject or manual, since we added UpdateProductValidator we could inject it, but for brevity we can validate directly or use the CreateProductValidator if same.
        // Or let ASP.NET Core handle it if FluentValidation.AspNetCore is configured to auto-validate.
        // Assuming we need to validate manually:
        var validator = new PuntoVenta.Application.Validators.UpdateProductValidator();
        var validationResult = await validator.ValidateAsync(updateProductDto);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        try
        {
            var modifiedBy = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? User.Identity?.Name;
            var updatedProduct = await _productService.UpdateAsync(productId, updateProductDto, modifiedBy);
            return Ok(updatedProduct);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("{productId:int}/activate")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Activate(int productId)
    {
        var success = await _productService.ActivateAsync(productId);
        return success ? NoContent() : NotFound($"Product with id {productId} not found.");
    }

    [HttpDelete("{productId:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(int productId)
    {
        var result = await _productService.DeleteAsync(productId);
        if (!result.Success)
            return NotFound(result.Message);

        return Ok(result);
    }
}