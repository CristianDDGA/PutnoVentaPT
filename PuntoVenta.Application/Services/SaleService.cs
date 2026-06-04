using PuntoVenta.Application.Extensions;
using PuntoVenta.Application.DTOs.Common;
using PuntoVenta.Application.DTOs.Sale;
using PuntoVenta.Application.Interfaces.Repositories;
using PuntoVenta.Application.Interfaces.Services;
using PuntoVenta.Domain.Entities;
using PuntoVenta.Domain.Enums;

namespace PuntoVenta.Application.Services;

public class SaleService : ISaleService
{
    private readonly ISaleRepository         _saleRepository;
    private readonly ICustomerRepository     _customerRepository;
    private readonly IProductRepository       _productRepository;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly IUserRepository          _userRepository;
    private readonly IUnitOfWork              _unitOfWork;

    public SaleService(
        ISaleRepository         saleRepository,
        ICustomerRepository     customerRepository,
        IProductRepository      productRepository,
        IStockMovementRepository stockMovementRepository,
        IUserRepository         userRepository,
        IUnitOfWork             unitOfWork)
    {
        _saleRepository         = saleRepository;
        _customerRepository     = customerRepository;
        _productRepository      = productRepository;
        _stockMovementRepository = stockMovementRepository;
        _userRepository         = userRepository;
        _unitOfWork             = unitOfWork;
    }

    public async Task<IEnumerable<SaleDto>> GetAllAsync()
    {
        var allSales = await _saleRepository.GetAllAsync();
        return allSales.Select(MapSaleToDto).ToList();
    }

    public async Task<SaleDto?> GetByIdAsync(int saleId)
    {
        var existingSale = await _saleRepository.GetByIdAsync(saleId);
        if (existingSale == null) return null;

        return MapSaleToDto(existingSale);
    }

    public async Task<int> GetNextInvoiceNumberAsync()
    {
        var lastSaleId = await _saleRepository.GetLastSaleIdAsync();
        return lastSaleId + 1;
    }

    public async Task<SaleDto> CreateAsync(CreateSaleDto createSaleDto, int? userId = null)
    {
        if (!Enum.TryParse<PaymentType>(createSaleDto.PaymentType, out var paymentType))
            throw new ArgumentException($"Invalid payment type: {createSaleDto.PaymentType}");

        var duplicateProductIds = createSaleDto.Details
            .GroupBy(detail => detail.ProductId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateProductIds.Count > 0)
            throw new InvalidOperationException("No se permite repetir el mismo producto en el detalle de la venta.");

        var customer = await _customerRepository.GetByIdAsync(createSaleDto.CustomerId)
            ?? throw new KeyNotFoundException($"Cliente {createSaleDto.CustomerId} no encontrado.");

        if (!customer.IsActive)
            throw new InvalidOperationException("El cliente seleccionado está inactivo.");

        var saleDetails = new List<SaleDetail>();

        foreach (var saleDetailDto in createSaleDto.Details)
        {
            var existingProduct = await _productRepository.GetByIdAsync(saleDetailDto.ProductId)
                ?? throw new KeyNotFoundException($"Producto {saleDetailDto.ProductId} no encontrado.");

            if (!existingProduct.IsActive)
                throw new InvalidOperationException($"El producto {existingProduct.Name} está inactivo.");

            if (existingProduct.Stock < saleDetailDto.Quantity)
                throw new InvalidOperationException($"Stock insuficiente para el producto: {existingProduct.Name}.");

            var newSaleDetail = SaleDetail.Create(
                existingProduct.ProductId,
                existingProduct.Name,
                saleDetailDto.Quantity,
                existingProduct.Price);

            saleDetails.Add(newSaleDetail);
        }

        string? sellerName = null;
        if (userId.HasValue)
        {
            var user = await _userRepository.GetByIdAsync(userId.Value);
            sellerName = user?.FullName;
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var newSale   = Sale.Create(createSaleDto.CustomerId, customer.DocumentNumber, customer.FullName, paymentType, saleDetails, userId, sellerName);
            var savedSale = await _saleRepository.AddAsync(newSale);

            foreach (var detail in savedSale.Details)
            {
                var existingProduct = await _productRepository.GetByIdAsync(detail.ProductId)
                    ?? throw new KeyNotFoundException($"Producto {detail.ProductId} no encontrado.");

                var stockReduced = await _productRepository.ReduceStockAsync(detail.ProductId, detail.Quantity);
                if (!stockReduced)
                    throw new InvalidOperationException($"Stock insuficiente para el producto: {existingProduct.Name}.");

                await _stockMovementRepository.AddAsync(StockMovement.Create(
                    detail.ProductId,
                    detail.Quantity,
                    existingProduct.Stock,
                    existingProduct.Stock - detail.Quantity,
                    StockMovementType.Out,
                    $"Venta #{savedSale.SaleId} cobrada y confirmada",
                    null));
            }

            savedSale.ConfirmSale();
            await _saleRepository.UpdateAsync(savedSale);

            await _unitOfWork.CommitTransactionAsync();

            // Refresh the saved sale from DB to reflect the new Status and Stock modifications
            var refreshedSale = await _saleRepository.GetByIdAsync(savedSale.SaleId) ?? savedSale;

            return MapSaleToDto(refreshedSale);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<SaleDto> SaveDraftAsync(int? saleId, CreateSaleDto createSaleDto, int? userId = null)
    {
        if (!Enum.TryParse<PaymentType>(createSaleDto.PaymentType, out var paymentType))
            throw new ArgumentException($"Invalid payment type: {createSaleDto.PaymentType}");

        var duplicateProductIds = createSaleDto.Details
            .GroupBy(detail => detail.ProductId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateProductIds.Count > 0)
            throw new InvalidOperationException("No se permite repetir el mismo producto en el detalle de la venta.");

        var customer = await _customerRepository.GetByIdAsync(createSaleDto.CustomerId)
            ?? throw new KeyNotFoundException($"Cliente {createSaleDto.CustomerId} no encontrado.");

        if (!customer.IsActive)
            throw new InvalidOperationException("El cliente seleccionado está inactivo.");

        var saleDetails = new List<SaleDetail>();

        foreach (var saleDetailDto in createSaleDto.Details)
        {
            var existingProduct = await _productRepository.GetByIdAsync(saleDetailDto.ProductId)
                ?? throw new KeyNotFoundException($"Producto {saleDetailDto.ProductId} no encontrado.");

            if (!existingProduct.IsActive)
                throw new InvalidOperationException($"El producto {existingProduct.Name} está inactivo.");

            var newSaleDetail = SaleDetail.Create(
                existingProduct.ProductId,
                existingProduct.Name,
                saleDetailDto.Quantity,
                existingProduct.Price);

            saleDetails.Add(newSaleDetail);
        }

        string? sellerName = null;
        if (userId.HasValue)
        {
            var user = await _userRepository.GetByIdAsync(userId.Value);
            sellerName = user?.FullName;
        }

        Sale sale;

        if (saleId.HasValue && saleId > 0)
        {
            sale = await _saleRepository.GetByIdTrackedAsync(saleId.Value)
                   ?? throw new KeyNotFoundException($"Borrador {saleId.Value} no encontrado.");

            sale.UpdateDraft(createSaleDto.CustomerId, customer.DocumentNumber, customer.FullName, paymentType, saleDetails, userId, sellerName);
            await _saleRepository.UpdateAsync(sale);
        }
        else
        {
            sale = Sale.Create(createSaleDto.CustomerId, customer.DocumentNumber, customer.FullName, paymentType, saleDetails, userId, sellerName);
            await _saleRepository.AddAsync(sale);
        }

        // Return DTO mapped correctly
        var refreshedSale = await _saleRepository.GetByIdAsync(sale.SaleId) ?? sale;
        return MapSaleToDto(refreshedSale);
    }

    public async Task<bool> DeleteDraftAsync(int saleId)
    {
        var sale = await _saleRepository.GetByIdTrackedAsync(saleId);
        if (sale == null) return false;

        if (sale.Status != SaleStatus.Draft)
            throw new InvalidOperationException("Solo se pueden eliminar borradores de esta forma.");

        await _saleRepository.DeleteAsync(sale);
        return true;
    }

    public async Task<PagedResult<SaleDto>> SearchPagedAsync(
        int?    saleId,
        string? customerName,
        int     page,
        int     pageSize,
        bool    excludeVoided = false,
        int?    sellerId = null)
    {
        var (items, totalCount) = await _saleRepository.SearchPagedAsync(saleId, customerName, page, pageSize, excludeVoided, sellerId);
        
        var dtos = items.Select(MapSaleToDto).ToList();

        return PagedResult<SaleDto>.Create(dtos, totalCount, page, pageSize);
    }

    public async Task<bool> VoidSaleAsync(int saleId)
    {
        return await CancelSaleAsync(saleId);
    }

    public async Task<bool> MarkAsPaidAsync(int saleId)
    {
        return await ConfirmSaleAsync(saleId);
    }

    public async Task<bool> ConfirmSaleAsync(int saleId, int? userId = null)
    {
        var sale = await _saleRepository.GetByIdTrackedAsync(saleId);
        if (sale == null) return false;

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            if (sale.Status == SaleStatus.Cancelled)
                throw new InvalidOperationException("La factura ya se encuentra cancelada.");

            if (sale.Status == SaleStatus.Confirmed)
                throw new InvalidOperationException("La factura ya se encuentra confirmada.");

            foreach (var detail in sale.Details)
            {
                var existingProduct = await _productRepository.GetByIdAsync(detail.ProductId)
                    ?? throw new KeyNotFoundException($"Producto {detail.ProductId} no encontrado.");

                var stockReduced = await _productRepository.ReduceStockAsync(detail.ProductId, detail.Quantity);
                if (!stockReduced)
                    throw new InvalidOperationException($"Stock insuficiente para el producto: {existingProduct.Name}.");

                await _stockMovementRepository.AddAsync(StockMovement.Create(
                    detail.ProductId,
                    detail.Quantity,
                    existingProduct.Stock,
                    existingProduct.Stock - detail.Quantity,
                    StockMovementType.Out,
                    $"Venta #{sale.SaleId} confirmada",
                    userId));
            }

            sale.ConfirmSale();
            await _saleRepository.UpdateAsync(sale);

            await _unitOfWork.CommitTransactionAsync();
            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<bool> CancelSaleAsync(int saleId, int? userId = null)
    {
        var sale = await _saleRepository.GetByIdTrackedAsync(saleId);
        if (sale == null) return false;

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            if (sale.Status == SaleStatus.Cancelled)
                throw new InvalidOperationException("La factura ya se encuentra cancelada.");

            if (sale.Status == SaleStatus.Confirmed)
            {
                foreach (var detail in sale.Details)
                {
                    var product = await _productRepository.GetByIdAsync(detail.ProductId);
                    if (product != null)
                    {
                        product.AddStock(detail.Quantity);
                        await _productRepository.UpdateStockAsync(product.ProductId, product.Stock);

                        await _stockMovementRepository.AddAsync(StockMovement.Create(
                            detail.ProductId,
                            detail.Quantity,
                            product.Stock - detail.Quantity,
                            product.Stock,
                            StockMovementType.In,
                            $"Venta #{sale.SaleId} cancelada. Reversión de stock",
                            userId));
                    }
                }
            }

            sale.CancelSale();
            await _saleRepository.UpdateAsync(sale);

            await _unitOfWork.CommitTransactionAsync();
            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private static SaleDto MapSaleToDto(Sale sale)
    {
        return new SaleDto
        {
            SaleId           = sale.SaleId,
            CustomerId       = sale.CustomerId,
            CustomerName     = !string.IsNullOrWhiteSpace(sale.CustomerName) && sale.CustomerName != "N/A" ? sale.CustomerName : (sale.Customer?.FullName ?? string.Empty),
            CustomerDocument = !string.IsNullOrWhiteSpace(sale.CustomerDocument) && sale.CustomerDocument != "N/A" ? sale.CustomerDocument : (sale.Customer?.DocumentNumber ?? string.Empty),
            CustomerAddress  = sale.Customer?.Address ?? string.Empty,
            CustomerCity     = sale.Customer?.City ?? string.Empty,
            CustomerPhone    = sale.Customer?.Phone ?? string.Empty,
            CustomerEmail    = sale.Customer?.Email ?? string.Empty,
            SellerName       = !string.IsNullOrWhiteSpace(sale.SellerName) ? sale.SellerName : (sale.User?.FullName ?? string.Empty),
            SaleDate         = sale.SaleDate,
            PaymentType      = sale.PaymentType.ToSpanish(),
            Subtotal         = sale.Subtotal,
            TaxAmount        = sale.TaxAmount,
            Total            = sale.Total,
            Status           = sale.Status.ToSpanish(),
            Details          = sale.Details.Select(detail => new SaleDetailDto
            {
                ProductId   = detail.ProductId,
                ProductName = !string.IsNullOrWhiteSpace(detail.ProductName) ? detail.ProductName : (detail.Product?.Name ?? string.Empty),
                Quantity    = detail.Quantity,
                UnitPrice   = detail.UnitPrice,
                Subtotal    = detail.Subtotal
            }).ToList()
        };
    }
}