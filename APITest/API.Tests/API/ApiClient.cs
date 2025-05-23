using API.Tests.Models;
using System.Net.Http.Json;

namespace API.Tests.API;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public ApiClient(string baseUrl)
    {
        _baseUrl = baseUrl;
        _httpClient = new HttpClient();
    }

    public async Task<List<Product>?> GetAllProductsAsync()
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/api/products");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Product>>();
    }

    public async Task<HttpResponseMessage> DeleteProductAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/api/deleteproduct?id={id}");
        return response;
    }

    public async Task<HttpResponseMessage> AddProductAsync(Product product)
    {
        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/addproduct", product);
        //response.EnsureSuccessStatusCode();
        //return await response.Content.ReadFromJsonAsync<Product>();
        return response;
    }

    public async Task<HttpResponseMessage> EditProductAsync(Product product)
    {
        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/editproduct", product);
        //response.EnsureSuccessStatusCode();
        //return await response.Content.ReadFromJsonAsync<Product>();
        return response;
    }
}