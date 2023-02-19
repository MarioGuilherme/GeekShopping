using AutoMapper;
using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Model;

namespace GeekShopping.CartAPI.Config;

public class MappingConfig {
    public static MapperConfiguration RegisterMaps() {
        MapperConfiguration mappingConfig = new(config => {
            config.CreateMap<Product, ProductVO>().ReverseMap();
            config.CreateMap<CartHeader, CartHeaderVO>().ReverseMap();
            config.CreateMap<CartDetail, CartDetailVO>().ReverseMap();
            config.CreateMap<Cart, CartVO>().ReverseMap();
        });
        return mappingConfig;
    }
}