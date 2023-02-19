using AutoMapper;
using GeekShopping.CouponAPI.Data.ValueObjects;
using GeekShopping.CouponAPI.Model;

namespace GeekShopping.CartAPI.Config;

public class MappingConfig {
    public static MapperConfiguration RegisterMaps() {
        MapperConfiguration mappingConfig = new(config => {
            config.CreateMap<Coupon, CouponVO>().ReverseMap();
        });
        return mappingConfig;
    }
}