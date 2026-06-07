using System.Net.Http.Json;
using PuntoVenta.Blazor.Models;

namespace PuntoVenta.Blazor.Services;

public class CustomerApiService
{
    private readonly HttpClient _httpClient;

    public CustomerApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<CustomerModel>> GetAllAsync()
    {
        try
        {
            var customers = await _httpClient.GetFromJsonAsync<List<CustomerModel>>("api/Customers");
            return customers ?? [];
        }
        catch { return []; }
    }

    public async Task<List<CustomerModel>> SearchByLastNameAsync(string lastName)
    {
        try
        {
            var matchingCustomers = await _httpClient.GetFromJsonAsync<List<CustomerModel>>(
                $"api/Customers/search?lastName={Uri.EscapeDataString(lastName)}");
            return matchingCustomers ?? [];
        }
        catch { return []; }
    }

    public async Task<CustomerModel?> GetByIdAsync(int customerId)
    {
        try { return await _httpClient.GetFromJsonAsync<CustomerModel>($"api/Customers/{customerId}"); }
        catch { return null; }
    }

    public async Task<PagedResultModel<CustomerModel>?> SearchPagedAsync(
        int?    customerId     = null,
        string? documentNumber = null,
        string? lastName       = null,
        int     page           = 1,
        int     pageSize       = 10,
        bool    onlyActive     = false)
    {
        try
        {
            var url = BuildPagedUrl("api/Customers/paged", page, pageSize,
                customerId.HasValue ? $"customerId={customerId}" : null,
                !string.IsNullOrWhiteSpace(documentNumber) ? $"documentNumber={Uri.EscapeDataString(documentNumber)}" : null,
                !string.IsNullOrWhiteSpace(lastName) ? $"lastName={Uri.EscapeDataString(lastName)}" : null,
                onlyActive ? "onlyActive=true" : null);

            return await _httpClient.GetFromJsonAsync<PagedResultModel<CustomerModel>>(url);
        }
        catch { return null; }
    }

    public async Task<CustomerModel?> CreateAsync(CreateCustomerModel createCustomerModel)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Customers", createCustomerModel);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<CustomerModel>();
        }
        catch { return null; }
    }

    public async Task<bool> UpdateAsync(int customerId, UpdateCustomerModel updateCustomerModel)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Customers/{customerId}", updateCustomerModel);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> ActivateAsync(int customerId)
    {
        try
        {
            var response = await _httpClient.PutAsync($"api/Customers/{customerId}/activate", null);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<DeleteResultModel?> DeleteAsync(int customerId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/Customers/{customerId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<DeleteResultModel>();
            }
            
            var errMessage = await response.Content.ReadAsStringAsync();
            return new DeleteResultModel { Success = false, Message = errMessage };
        }
        catch (Exception ex)
        {
            return new DeleteResultModel { Success = false, Message = ex.Message };
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static string BuildPagedUrl(string baseUrl, int page, int pageSize, params string?[] filters)
    {
        var queryParts = filters
            .Where(f => f is not null)
            .Concat([$"page={page}", $"pageSize={pageSize}"]);

        return $"{baseUrl}?{string.Join('&', queryParts)}";
    }
}