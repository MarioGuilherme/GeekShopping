using GeekShopping.Web.Models;
using GeekShopping.Web.Services.IServices;
using GeekShopping.Web.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.Web.Controllers;

public class ProductController : Controller {
    private readonly IProductService _productService;

    public ProductController(IProductService productService) {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
    }

    public async Task<IActionResult> ProductIndex() {
        IEnumerable<ProductViewModel> products = await this._productService.FindAllProducts(string.Empty);
        return View(products);
    }

    public IActionResult ProductCreate() => View();

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ProductCreate(ProductViewModel model) {
        if (ModelState.IsValid) {
            string? accessToken = await HttpContext.GetTokenAsync("access_token");
            ProductViewModel? modelCreated = await this._productService.CreateProduct(model, accessToken);
            if (modelCreated != null)
                return RedirectToAction(nameof(ProductIndex));
        }
        return View(model);
    }

    public async Task<IActionResult> ProductUpdate(long id) {
        string? accessToken = await HttpContext.GetTokenAsync("access_token");
        ProductViewModel model = await this._productService.FindProductById(id, accessToken);
        if (model != null)
            return View(model);
        return NotFound();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ProductUpdate(ProductViewModel model) {
        if (ModelState.IsValid) {
            string? accessToken = await HttpContext.GetTokenAsync("access_token");
            ProductViewModel? response = await this._productService.UpdateProduct(model, accessToken);
            if (response != null)
                return RedirectToAction(nameof(ProductIndex));
        }
        return View(model);
    }

    [Authorize]
    public async Task<IActionResult> ProductDelete(long id) {
        string? accessToken = await HttpContext.GetTokenAsync("access_token");
        ProductViewModel model = await this._productService.FindProductById(id, accessToken);
        if (model != null)
            return View(model);
        return NotFound();
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> ProductDelete(ProductViewModel model) {
        string? accessToken = await HttpContext.GetTokenAsync("access_token");
        bool response = await this._productService.DeleteProductById(model.Id, accessToken);
        if (response)
            return RedirectToAction(nameof(ProductIndex));
        return View(model);
    }
}