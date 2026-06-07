using Mapster;
using PuntoVenta.Application.DTOs.Common;
using PuntoVenta.Application.DTOs.Product;
using PuntoVenta.Application.Interfaces.Repositories;
using PuntoVenta.Application.Interfaces.Services;
using PuntoVenta.Domain.Entities;

namespace PuntoVenta.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Adapt<IEnumerable<ProductDto>>();
    }

    public async Task<IEnumerable<ProductDto>> SearchByNameAsync(string name)
    {
        var matchingProducts = await _productRepository.SearchByNameAsync(name);
        return matchingProducts.Adapt<IEnumerable<ProductDto>>();
    }

    public async Task<ProductDto?> GetByIdAsync(int productId)
    {
        var existingProduct = await _productRepository.GetByIdAsync(productId);
        return existingProduct?.Adapt<ProductDto>();
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto createProductDto)
    {
        var newProduct = Product.Create(
            createProductDto.Name,
            createProductDto.Price,
            createProductDto.Stock);

        var savedProduct = await _productRepository.AddAsync(newProduct);
        return savedProduct.Adapt<ProductDto>();
    }

    public async Task<ProductDto> UpdateAsync(int productId, UpdateProductDto updateProductDto, string? modifiedBy = null)
    {
        var existingProduct = await _productRepository.GetByIdTrackedAsync(productId);
        if (existingProduct == null)
            throw new KeyNotFoundException($"Producto {productId} no encontrado.");

        existingProduct.Update(
            updateProductDto.Name,
            updateProductDto.Price,
            updateProductDto.Stock,
            modifiedBy);

        await _productRepository.UpdateAsync(existingProduct);
        return existingProduct.Adapt<ProductDto>();
    }

    public async Task<bool> ActivateAsync(int productId)
        => await _productRepository.ActivateAsync(productId);

    public async Task<bool> DeactivateAsync(int productId)
        => await _productRepository.DeactivateAsync(productId);

    public async Task<PagedResult<ProductDto>> SearchPagedAsync(
        int?    productId,
        string? name,
        int     page,
        int     pageSize,
        bool    onlyInStock = false,
        bool    onlyActive = false)
    {
        var (items, totalCount) = await _productRepository.SearchPagedAsync(productId, name, page, pageSize, onlyInStock, onlyActive);
        var productDtos         = items.Adapt<IEnumerable<ProductDto>>();
        return PagedResult<ProductDto>.Create(productDtos, totalCount, page, pageSize);
    }

    public async Task<DeleteResultDto> DeleteAsync(int productId)
    {
        var existingProduct = await _productRepository.GetByIdAsync(productId);
        if (existingProduct == null)
        {
            return new DeleteResultDto
            {
                Success = false,
                Message = "Producto no encontrado."
            };
        }

        var hasSales = await _productRepository.HasSalesAsync(productId);
        if (hasSales)
        {
            var deactivated = await _productRepository.DeactivateAsync(productId);
            return new DeleteResultDto
            {
                Success = deactivated,
                IsLogicalDelete = true,
                Message = $"El producto '{existingProduct.Name}' tiene historial de ventas asociado. Se ha marcado como inactivo (eliminación lógica)."
            };
        }
        else
        {
            var deleted = await _productRepository.PhysicalDeleteAsync(productId);
            return new DeleteResultDto
            {
                Success = deleted,
                IsLogicalDelete = false,
                Message = $"El producto '{existingProduct.Name}' fue eliminado físicamente con éxito del sistema."
            };
        }
    }
}