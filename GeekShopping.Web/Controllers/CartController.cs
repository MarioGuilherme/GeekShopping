using GeekShopping.Web.Models;
using GeekShopping.Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.Web.Controllers;

public class CartController : Controller {
    private readonly ILogger<HomeController> _logger;
    private readonly IProductService _productService;
    private readonly ICartService _cartService;
    private readonly ICouponService _couponService;

    public CartController(
        IProductService productService,
        ICartService cartService,
        ICouponService couponService
    ) {
        this._productService = productService;
        this._cartService = cartService;
        this._couponService = couponService;
    }

    [Authorize]
    public async Task<IActionResult> CartIndex() {
        return View(await this.FindUserCart());
    }

    [HttpPost]
    [ActionName("ApplyCoupon")]
    public async Task<IActionResult> ApplyCoupon(CartViewModel model) {
        string? accessToken = await HttpContext.GetTokenAsync("access_token");
        string? userId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value;
        bool response = await this._cartService.ApplyCoupon(model, accessToken);
        if (response)
            return RedirectToAction(nameof(CartIndex));
        return View();
    }

    [HttpPost]
    [ActionName("RemoveCoupon")]
    public async Task<IActionResult> RemoveCoupon() {
        string? accessToken = await HttpContext.GetTokenAsync("access_token");
        string? userId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value;
        bool response = await this._cartService.RemoveCoupon(userId, accessToken);
        if (response)
            return RedirectToAction(nameof(CartIndex));
        return View();
    }

    public async Task<IActionResult> Remove(int id) {
        string? accessToken = await HttpContext.GetTokenAsync("access_token");
        string? userId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value;
        bool response = await this._cartService.RemoveFromCart(id, accessToken);
        if (response) return RedirectToAction(nameof(CartIndex));
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Checkout() {
        return View(await this.FindUserCart());
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation() {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Checkout(CartViewModel model) {
        string? accessToken = await HttpContext.GetTokenAsync("access_token");
        object response = await this._cartService.Checkout(model.CartHeader, accessToken);
        if (response is not null && response.GetType() == typeof(string)) {
            TempData["Error"] = response;
            return RedirectToAction(nameof(Checkout));
        } else if (response is not null)
            return RedirectToAction(nameof(Confirmation));
        return View(model);
    }

    private async Task<CartViewModel> FindUserCart() {
        string? accessToken = await HttpContext.GetTokenAsync("access_token");
        string? userId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value;
        CartViewModel? response = await this._cartService.FindCartByUserId(userId, accessToken);
        if (response?.CartDetails != null) {
            if (!string.IsNullOrEmpty(response.CartHeader.CouponCode)) {
                var coupon = await this._couponService.GetCoupon(response.CartHeader.CouponCode, accessToken);
                if (coupon?.CouponCode is not null)
                    response.CartHeader.DiscountAmount = coupon.DiscountAmount;
            }
            foreach (CartDetailViewModel? detail in response.CartDetails)
                response.CartHeader.PurchaseAmount += (detail.Product.Price * detail.Count);
            response.CartHeader.PurchaseAmount -= response.CartHeader.DiscountAmount;
        }
        return response!;
    }
}