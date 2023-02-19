using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Messages;
using GeekShopping.CartAPI.RabbitMQSender;
using GeekShopping.CartAPI.Repository;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.CartAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CartController : ControllerBase {
    private ICartRepository _cartRepository;
    private ICouponRepository _couponRepository;
    private IRabbitMQMessageSender _rabbitMQMessageSender;

    public CartController(ICartRepository cartRepository, ICouponRepository couponRepository, IRabbitMQMessageSender rabbitMQMessageSender) {
        this._cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        this._couponRepository = couponRepository ?? throw new ArgumentNullException(nameof(couponRepository));
        this._rabbitMQMessageSender = rabbitMQMessageSender ?? throw new ArgumentNullException(nameof(rabbitMQMessageSender));
    }

    [HttpGet("find-cart/{id}")]
    public async Task<ActionResult<CartVO>> FindById(string id) {
        CartVO cart = await this._cartRepository.FindCartByUserId(id);
        if (cart == null) return NotFound();
        return Ok(cart);
    }

    [HttpPost("add-cart/")]
    public async Task<ActionResult<CartVO>> AddCart(CartVO cart) {
        CartVO cartMapped = await this._cartRepository.SaveOrUpdateCart(cart);
        if (cartMapped == null) return NotFound();
        return Ok(cartMapped);
    }

    [HttpPut("update-cart/")]
    public async Task<ActionResult<CartVO>> UpdateCart(CartVO cart) {
        CartVO cartMapped = await this._cartRepository.SaveOrUpdateCart(cart);
        if (cartMapped == null) return NotFound();
        return Ok(cartMapped);
    }

    [HttpDelete("remove-cart/{id}")]
    public async Task<ActionResult<CartVO>> RemoveCart(int id) {
        bool status = await this._cartRepository.RemoveFromCart(id);
        if (!status) return BadRequest();
        return Ok(status);
    }

    [HttpPost("apply-coupon")]
    public async Task<ActionResult<CartVO>> ApplyCoupon(CartVO cart) {
        bool status = await this._cartRepository.ApplyCoupon(cart.CartHeader.UserId, cart.CartHeader.CouponCode);
        if (status) return Ok(status);
        return NotFound();
    }

    [HttpDelete("remove-coupon/{userId}")]
    public async Task<ActionResult<CartVO>> RemoveCoupon(string userId) {
        bool status = await this._cartRepository.RemoveCoupon(userId);
        if (status) return Ok(status);
        return NotFound();
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutHeaderVO>> Checkout(CheckoutHeaderVO checkoutHeader) {
        if (checkoutHeader.UserId is null) return BadRequest();

        CartVO cart = await this._cartRepository.FindCartByUserId(checkoutHeader.UserId);
        
        if (cart is null) return NotFound();

        string token = Request.Headers["Authorization"];
        if (!string.IsNullOrEmpty(checkoutHeader.CouponCode)) {
            CouponVO coupon = await this._couponRepository.GetCouponByCouponCode(checkoutHeader.CouponCode, token);
            if (checkoutHeader.DiscountAmount != coupon.DiscountAmount) {
                return StatusCode(412);
            }
        }

        checkoutHeader.CartDetails = cart.CartDetails;
        checkoutHeader.DateTime = DateTime.Now;

        // RabbitMQ Logic comes heres
        this._rabbitMQMessageSender.SendMessage(checkoutHeader, "checkoutqueue");

        await this._cartRepository.ClearCart(checkoutHeader.UserId);

        return Ok(checkoutHeader);
    }
}