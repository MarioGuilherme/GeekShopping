using GeekShopping.Web.Models;
using GeekShopping.Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GeekShopping.Web.Controllers {
    public class HomeController : Controller {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;
        private readonly ICartService _cartService;

        public HomeController(ILogger<HomeController> logger, IProductService productService, ICartService cartService) {
            this._logger = logger;
            this._productService = productService;
            this._cartService = cartService;
        }

        public async Task<IActionResult> Index() {
            IEnumerable<ProductViewModel> products = await this._productService.FindAllProducts("") ?? new List<ProductViewModel>();
            return View(products);
        }

        [Authorize]
        public async Task<IActionResult> Details(int id) {
            string? accessToken = await HttpContext.GetTokenAsync("access_token");
            ProductViewModel? model = await this._productService.FindProductById(id, accessToken ?? "");
            return View(model!);
        }

        [HttpPost]
        [ActionName("Details")]
        [Authorize]
        public async Task<IActionResult> DetailsPost(ProductViewModel model) {
            string? accessToken = await HttpContext.GetTokenAsync("access_token");
            CartViewModel cart = new() {
                CartHeader = new() {
                    UserId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value
                }
            };
            CartDetailViewModel cartDetail = new() {
                Count = model.Count,
                ProductId = model.Id,
                Product = await this._productService.FindProductById(model.Id, accessToken)
            };
            List<CartDetailViewModel> cartDetails = new() { cartDetail };
            cart.CartDetails = cartDetails;
            CartViewModel? response = await this._cartService.AddItemToCart(cart, accessToken);
            if (response != null)
                RedirectToAction(nameof(Index));
            return View(model);
        }

        public IActionResult Privacy() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        public async Task<IActionResult> Login() {
            string? accessToken = await HttpContext.GetTokenAsync("access_token");
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Logout() => SignOut("Cookies", "oidc");
    }
}