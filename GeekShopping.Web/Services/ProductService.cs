using GeekShopping.Web.Models;
using GeekShopping.Web.Services.IServices;
using GeekShopping.Web.Utils;
using System.Net.Http.Headers;

namespace GeekShopping.Web.Services;

public class ProductService : IProductService {
    private readonly HttpClient _client;
    public const string BasePath = "api/v1/product";

    public ProductService(HttpClient client) {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<IEnumerable<ProductViewModel>?> FindAllProducts(string token) {
        this._client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage? response = await this._client.GetAsync(BasePath);
        return await response.ReadContentAs<List<ProductViewModel>>(); 
    }

    public async Task<ProductViewModel?> FindProductById(long id, string token) {
        this._client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage? response = await this._client.GetAsync($"{BasePath}/{id}");
        return await response.ReadContentAs<ProductViewModel>();
    }

    public async Task<ProductViewModel> CreateProduct(ProductViewModel model, string token) {
        this._client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage? response = await this._client.PostAsJson(BasePath, model);
        if (response.IsSuccessStatusCode)
            return await response.ReadContentAs<ProductViewModel>();
        throw new Exception("Something went wrong when calling API");
    }

    public async Task<ProductViewModel> UpdateProduct(ProductViewModel model, string token) {
        this._client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage? response = await this._client.PutAsJson(BasePath, model);
        if (response.IsSuccessStatusCode)
            return await response.ReadContentAs<ProductViewModel>();
        throw new Exception("Something went wrong when calling API");
    }

    public async Task<bool> DeleteProductById(long id, string token) {
        this._client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage? response = await this._client.DeleteAsync($"{BasePath}/{id}");
        if (response.IsSuccessStatusCode)
            return await response.ReadContentAs<bool>();
        throw new Exception("Something went wrong when calling API");
    }
}