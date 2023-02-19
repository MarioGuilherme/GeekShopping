using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Repository;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace GeekShopping.CartAPI.Repository;

public class CouponRepository : ICouponRepository {
    private readonly HttpClient _client;

    public CouponRepository(HttpClient client) {
        this._client = client;
    }

    public async Task<CouponVO> GetCouponByCouponCode(string couponCode, string token) {
        // api/v1/coupon
        this._client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage response = await this._client.GetAsync($"https://localhost:4450/api/v1/coupon/{couponCode}");
        string content = await response.Content.ReadAsStringAsync();
        if (response.StatusCode != HttpStatusCode.OK)
            return new CouponVO();

        return JsonSerializer.Deserialize<CouponVO>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}