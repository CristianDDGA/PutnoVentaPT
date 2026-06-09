using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoVenta.Application.Constants;
using PuntoVenta.Application.DTOs.Sale;
using PuntoVenta.Application.Interfaces.Services;
using System.Security.Claims;

namespace PuntoVenta.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Seller}")]
public class SalesController : ControllerBase
{
    private readonly ISaleService             _saleService;
    private readonly IPdfService              _pdfService;
    private readonly IExcelExportService      _excelExportService;
    private readonly IValidator<CreateSaleDto> _createSaleValidator;

    public SalesController(
        ISaleService              saleService,
        IPdfService               pdfService,
        IExcelExportService       excelExportService,
        IValidator<CreateSaleDto> createSaleValidator)
    {
        _saleService         = saleService;
        _pdfService          = pdfService;
        _excelExportService  = excelExportService;
        _createSaleValidator = createSaleValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var allSales = await _saleService.GetAllAsync();
        return Ok(allSales);
    }

    /// <summary>
    /// Returns the next correlative invoice number (last SaleId + 1).
    /// </summary>
    [HttpGet("next-number")]
    public async Task<IActionResult> GetNextInvoiceNumber()
    {
        var nextNumber = await _saleService.GetNextInvoiceNumberAsync();
        return Ok(nextNumber);
    }

    /// <summary>
    /// Returns a paginated list of sales.
    /// When saleId is provided it takes precedence over customerName.
    /// </summary>
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int?    saleId       = null,
        [FromQuery] string? customerName = null,
        [FromQuery] int     page         = 1,
        [FromQuery] int     pageSize     = 15,
        [FromQuery] bool    excludeVoided = false)
    {
        if (page < 1)     page     = 1;
        if (pageSize < 1) pageSize = 15;

        int? sellerId = null;
        if (User.IsInRole(AppRoles.Seller))
        {
            sellerId = GetCurrentUserId();
        }

        var pagedResult = await _saleService.SearchPagedAsync(saleId, customerName, page, pageSize, excludeVoided, sellerId);
        return Ok(pagedResult);
    }

    [HttpGet("{saleId:int}")]
    public async Task<IActionResult> GetById(int saleId)
    {
        var existingSale = await _saleService.GetByIdAsync(saleId);

        if (existingSale is null)
            return NotFound($"Sale with id {saleId} not found.");

        return Ok(existingSale);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSaleDto createSaleDto)
    {
        var validationResult = await _createSaleValidator.ValidateAsync(createSaleDto);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        var userId = GetCurrentUserId();
        var savedSale = await _saleService.CreateAsync(createSaleDto, userId);
        return CreatedAtAction(nameof(GetById), new { saleId = savedSale.SaleId }, savedSale);
    }

    [HttpPost("draft")]
    public async Task<IActionResult> SaveDraft([FromQuery] int? saleId, [FromBody] CreateSaleDto createSaleDto)
    {
        var validationResult = await _createSaleValidator.ValidateAsync(createSaleDto);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        var userId = GetCurrentUserId();
        var savedSale = await _saleService.SaveDraftAsync(saleId, createSaleDto, userId);
        return Ok(savedSale);
    }

    [HttpDelete("draft/{saleId:int}")]
    public async Task<IActionResult> DeleteDraft(int saleId)
    {
        try
        {
            var success = await _saleService.DeleteDraftAsync(saleId);
            if (!success) return NotFound($"Borrador con id {saleId} no encontrado.");
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("export/excel")]
    public async Task<IActionResult> ExportExcel(
        [FromQuery] int?    saleId       = null,
        [FromQuery] string? customerName = null,
        [FromQuery] bool    excludeVoided = false)
    {
        int? sellerId = null;
        if (User.IsInRole(AppRoles.Seller))
            sellerId = GetCurrentUserId();

        var pagedResult = await _saleService.SearchPagedAsync(
            saleId, customerName, page: 1, pageSize: 10_000, excludeVoided, sellerId);

        var excelBytes = _excelExportService.ExportSales(pagedResult.Items);
        var fileName   = $"Ventas_{DateTime.UtcNow:yyyyMMdd_HHmm}.xlsx";

        return File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    [HttpGet("{saleId:int}/pdf")]
    public async Task<IActionResult> GeneratePdf(int saleId)
    {
        var existingSale = await _saleService.GetByIdAsync(saleId);

        if (existingSale is null)
            return NotFound($"Sale with id {saleId} not found.");

        var pdfBytes = _pdfService.GenerateInvoice(existingSale);
        return File(pdfBytes, "application/pdf", $"Factura_{saleId}.pdf");
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPut("{saleId:int}/void")]
    public async Task<IActionResult> VoidSale(int saleId)
    {
        try
        {
            var success = await _saleService.CancelSaleAsync(saleId, GetCurrentUserId());
            if (!success) return NotFound($"Sale with id {saleId} not found.");
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Seller}")]
    [HttpPut("{saleId:int}/pay")]
    public async Task<IActionResult> MarkAsPaid(int saleId)
    {
        try
        {
            var success = await _saleService.ConfirmSaleAsync(saleId, GetCurrentUserId());
            if (!success) return NotFound($"Sale with id {saleId} not found.");
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    private int? GetCurrentUserId()
        => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : null;
}