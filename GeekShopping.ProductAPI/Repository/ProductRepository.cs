using AutoMapper;
using GeekShopping.ProductAPI.Data.ValueObjects;
using GeekShopping.ProductAPI.Model;
using GeekShopping.ProductAPI.Model.Context;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.ProductAPI.Repository;

public class ProductRepository : IProductRepository {
    private readonly MySQLContext _context;
    private readonly IMapper _mapper;

    public ProductRepository(MySQLContext context, IMapper mapper) {
        this._context = context;
        this._mapper = mapper;
    }

    public async Task<IEnumerable<ProductVO>> FindAll() {
        List<Product> products = await this._context.Products.ToListAsync();
        return this._mapper.Map<List<ProductVO>>(products);
    }

    public async Task<ProductVO?> FindById(long id) {
        Product? product = await this._context.Products.FirstOrDefaultAsync(product => product.Id == id);
        return this._mapper.Map<ProductVO?>(product);
    }

    public async Task<ProductVO> Create(ProductVO product) {
        Product productMapped = this._mapper.Map<Product>(product);
        await this._context.AddAsync(productMapped);
        await this._context.SaveChangesAsync();
        return this._mapper.Map<ProductVO>(productMapped);
    }

    public async Task<ProductVO> Update(ProductVO product) {
        Product productMapped = this._mapper.Map<Product>(product);
        this._context.Update(productMapped);
        await this._context.SaveChangesAsync();
        return this._mapper.Map<ProductVO>(productMapped);
    }

    public async Task<bool> Delete(long id) {
        try {
            Product? product = await this._context.Products.FirstOrDefaultAsync(product => product.Id == id);

            if (product == null) return false;

            this._context.Products.Remove(product);
            await this._context.SaveChangesAsync();
            return true;
        } catch (Exception) {
            return false;
        }
    }
}