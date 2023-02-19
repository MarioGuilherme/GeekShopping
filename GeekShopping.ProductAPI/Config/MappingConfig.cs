using AutoMapper;
using GeekShopping.ProductAPI.Data.ValueObjects;
using GeekShopping.ProductAPI.Model;

namespace GeekShopping.ProductAPI.Config;

public class MappingConfig {
    public static MapperConfiguration RegisterMaps() {
        MapperConfiguration mappingConfig = new(config => {
            config.CreateMap<ProductVO, Product>().ReverseMap();
        });
        return mappingConfig;
    }
}