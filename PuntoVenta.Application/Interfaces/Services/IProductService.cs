using PuntoVenta.Application.DTOs.Common;
using PuntoVenta.Application.DTOs.Product;

namespace PuntoVenta.Application.Interfaces.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<IEnumerable<ProductDto>> SearchByNameAsync(string name);
    Task<ProductDto?>             GetByIdAsync(int productId);
    Task<ProductDto>              CreateAsync(CreateProductDto createProductDto);
    Task<ProductDto>              UpdateAsync(int productId, UpdateProductDto updateProductDto, string? modifiedBy = null);
    Task<bool>                   ActivateAsync(int productId);
    Task<bool>                   DeactivateAsync(int productId);
    Task<DeleteResultDto>        DeleteAsync(int productId);

    /// <summary>
    /// Returns a paginated subset of products filtered by optional productId (exact) or name (LIKE).
    /// </summary>
    Task<PagedResult<ProductDto>> SearchPagedAsync(
        int?    productId,
        string? name,
        int     page,
        int     pageSize,
        bool    onlyInStock = false,
        bool    onlyActive = false);
}