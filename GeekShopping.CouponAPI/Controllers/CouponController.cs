using GeekShopping.CouponAPI.Data.ValueObjects;
using GeekShopping.CouponAPI.Repository;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.CartAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CouponController : ControllerBase {
    private ICouponRepository _repository;

    public CouponController(ICouponRepository repository) {
        this._repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    [HttpGet("{couponCode}")]
    public async Task<ActionResult<CouponVO>> GetCouponByCouponCode(string couponCode) {
        CouponVO coupon = await this._repository.GetCouponByCouponCode(couponCode);
        if (coupon is null) return NotFound();
        return Ok(coupon);
    }
}