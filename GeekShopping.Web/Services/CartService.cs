using GeekShopping.Web.Models;
using GeekShopping.Web.Services.IServices;
using GeekShopping.Web.Utils;
using System.Net.Http.Headers;

namespace GeekShopping.Web.Services;

public class CartService : ICartService {
    private readonly HttpClient _client;
    public const string BasePath = "api/v1/cart";

    public CartService(HttpClient client) {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<CartViewModel> FindCartByUserId(string userId, string token) {
        this._client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage? response = await this._client.GetAsync($"{BasePath}/find-cart/{userId}");
        return await response.ReadContentAs<CartViewModel>();
    }

    public async Task<CartViewModel> AddItemToCart(CartViewModel cart, string token) {
        this._client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage? response = await this._client.PostAsJson($"{BasePath}/add-cart", cart);
        if (response.IsSuccessStatusCode)
            return await response.ReadContentAs<CartViewModel>();
        throw new Exception("Something went wrong when calling API");
    }

    public async Task<CartViewModel> UpdateCart(CartViewModel cart, string token) {
        this._client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage? response = await this._client.PutAsJson($"{BasePath}/update-cart", cart);
        if (response.IsSuccessStatusCode)
            return await response.ReadContentAs<CartViewModel>();
        throw new Exception("Something went wrong when calling API");
    }

    public async Task<bool> RemoveCoupon(string userId, string token) {
        this._client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage? response = await this._client.DeleteAsync($"{BasePath}/remove-coupon/{userId}");
        if (response.IsSuccessStatusCode)
            return await response.ReadContentAs<bool>();
        throw new Exception("Something went wrong when calling API");
    }

    public async Task<bool> ApplyCoupon(CartViewModel cart, string token) {
        this._client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage? response = await this._client.PostAsJson($"{BasePath}/apply-coupon", cart);
        if (response.IsSuccessStatusCode)
            return await response.ReadContentAs<bool>();
        throw new Exception("Something went wrong when calling API");
    }

    public async Task<object> Checkout(CartHeaderViewModel cartheader, string token) {
        this._client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage? response = await this._client.PostAsJson($"{BasePath}/checkout", cartheader);
        if (response.IsSuccessStatusCode)
            return await response.ReadContentAs<CartHeaderViewModel>();
        else if (response.StatusCode.ToString().Equals("PreconditionFailed"))
            return "Coupon Price has changed, please confirm again!";
        throw new Exception("Something went wrong when calling API");
    }

    public async Task<bool> ClearCart(string userId, string token) {
        throw new NotImplementedException();
    }

    public async Task<bool> RemoveFromCart(long cartId, string token) {
        this._client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage? response = await this._client.DeleteAsync($"{BasePath}/remove-cart/{cartId}");
        if (response.IsSuccessStatusCode)
            return await response.ReadContentAs<bool>();
        throw new Exception("Something went wrong when calling API");
    }
}