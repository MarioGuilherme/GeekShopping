using GeekShopping.OrderAPI.Model;
using GeekShopping.OrderAPI.Model.Context;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.OrderAPI.Repository;

public class OrderRepository : IOrderRepository {
    private readonly DbContextOptions<MySQLContext> _context;

    public OrderRepository(DbContextOptions<MySQLContext> context) {
        this._context = context;
    }

    public async Task<bool> AddOrder(OrderHeader orderHeader) {
        if (orderHeader is null) return false;
        await using MySQLContext _db = new (this._context);
        _db.Headers.Add(orderHeader);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task UpdateOrderPaymentStatus(long orderHeaderId, bool status) {
        await using MySQLContext _db = new(this._context);
        OrderHeader header = await _db.Headers.FirstOrDefaultAsync(o => o.Id == orderHeaderId);
        if (header is null) return;
        header.PaymentStatus = status;
        await _db.SaveChangesAsync();
    }
}