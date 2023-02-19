using AutoMapper;
using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Model;
using GeekShopping.CartAPI.Model.Context;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.CartAPI.Repository;

public class CartRepository : ICartRepository {
    private readonly MySQLContext _context;
    private readonly IMapper _mapper;

    public CartRepository(MySQLContext context, IMapper mapper) {
        this._context = context;
        this._mapper = mapper;
    }

    public async Task<bool> ApplyCoupon(string userId, string couponCode) {
        CartHeader header = await this._context.CartHeaders.FirstOrDefaultAsync(c => c.UserId == userId);
        if (header is not null) {
            header.CouponCode = couponCode;
            this._context.CartHeaders.Update(header);
            await this._context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<bool> ClearCart(string userId) {
        CartHeader cartHeader = await this._context.CartHeaders.FirstOrDefaultAsync(c => c.UserId == userId);
        if (cartHeader != null) {
            this._context.CartDetails.RemoveRange(this._context.CartDetails.Where(c => c.CartHeaderId == cartHeader.Id));
            this._context.CartHeaders.Remove(cartHeader);
            await this._context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<CartVO> FindCartByUserId(string userId) {
        Cart cart = new() {
            CartHeader = await this._context.CartHeaders.FirstOrDefaultAsync(c => c.UserId == userId) ?? new()
        };
        cart.CartDetails = this._context.CartDetails
            .Where(c => c.CartHeaderId == cart.CartHeader.Id)
            .Include(c => c.Product);
        return this._mapper.Map<CartVO>(cart);
    }

    public async Task<bool> RemoveCoupon(string userId) {
        CartHeader header = await this._context.CartHeaders.FirstOrDefaultAsync(c => c.UserId == userId);
        if (header is not null) {
            header.CouponCode = "";
            this._context.CartHeaders.Update(header);
            await this._context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<bool> RemoveFromCart(long cartDetaildsId) {
        try {
            CartDetail cartDetail = await this._context.CartDetails.FirstOrDefaultAsync(c => c.Id == cartDetaildsId);
            int total = this._context.CartDetails.Where(c => c.CartHeaderId == cartDetail.CartHeaderId).Count();
            this._context.CartDetails.Remove(cartDetail);
            if (total == 1) {
                CartHeader cartHeaderToRemove = await this._context.CartHeaders.FirstOrDefaultAsync(c => c.Id == cartDetail.CartHeaderId);
                this._context.CartHeaders.Remove(cartHeaderToRemove);
                await this._context.SaveChangesAsync();
            }
            return true;
        } catch (Exception) {
            return false;
        }
    }

    public async Task<CartVO> SaveOrUpdateCart(CartVO cart) {
        Cart cartMapped = this._mapper.Map<Cart>(cart);
        Product product = await this._context.Products.FirstOrDefaultAsync(
            p => p.Id == cart.CartDetails.FirstOrDefault()!.ProductId
        );

        if (product == null) {
            await this._context.Products.AddAsync(cartMapped.CartDetails.FirstOrDefault().Product);
            await this._context.SaveChangesAsync();
        }

        CartHeader cartHeader = await this._context.CartHeaders.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == cartMapped.CartHeader.UserId);

        if (cartHeader == null) {
            await this._context.CartHeaders.AddAsync(cartMapped.CartHeader);
            await this._context.SaveChangesAsync();
            cartMapped.CartDetails.FirstOrDefault().CartHeaderId = cartMapped.CartHeader.Id;
            cartMapped.CartDetails.FirstOrDefault().Product = null;
            await this._context.CartDetails.AddAsync(cartMapped.CartDetails.FirstOrDefault());
            await this._context.SaveChangesAsync();
        } else {
            CartDetail cartDetail = await this._context.CartDetails.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == cartMapped.CartDetails.FirstOrDefault().ProductId && p.CartHeaderId == cartHeader.Id);

            if (cartDetail == null) {
                cartMapped.CartDetails.FirstOrDefault().CartHeaderId = cartHeader.Id;
                cartMapped.CartDetails.FirstOrDefault().Product = null;
                await this._context.CartDetails.AddAsync(cartMapped.CartDetails.FirstOrDefault());
                await this._context.SaveChangesAsync();
            } else {
                cartMapped.CartDetails.FirstOrDefault().Product = null;
                cartMapped.CartDetails.FirstOrDefault().Count += cartDetail.Count;
                cartMapped.CartDetails.FirstOrDefault().Id = cartDetail.Id;
                cartMapped.CartDetails.FirstOrDefault().CartHeaderId = cartDetail.CartHeaderId;
                this._context.CartDetails.Update(cartMapped.CartDetails.FirstOrDefault());
                await this._context.SaveChangesAsync();
            }
        }
        return this._mapper.Map<CartVO>(cartMapped);
    }
}