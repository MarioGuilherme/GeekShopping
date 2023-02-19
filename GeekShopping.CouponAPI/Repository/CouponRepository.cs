using AutoMapper;
using GeekShopping.CouponAPI.Data.ValueObjects;
using GeekShopping.CouponAPI.Model;
using GeekShopping.CouponAPI.Model.Context;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.CouponAPI.Repository;

public class CouponRepository : ICouponRepository {
    private readonly MySQLContext _context;
    private readonly IMapper _mapper;

    public CouponRepository(MySQLContext context, IMapper mapper) {
        this._context = context;
        this._mapper = mapper;
    }

    public async Task<CouponVO> GetCouponByCouponCode(string couponCode) {
        Coupon coupon = await this._context.Coupons.FirstOrDefaultAsync(c => c.CouponCode == couponCode);
        return this._mapper.Map<CouponVO>(coupon);
    }
}