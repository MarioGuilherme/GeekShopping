using Microsoft.EntityFrameworkCore;

namespace GeekShopping.OrderAPI.Model.Context;

public class MySQLContext : DbContext {
	public MySQLContext(DbContextOptions<MySQLContext> options) : base(options) { }

	public DbSet<OrderHeader> Headers { get; set; }
	public DbSet<OrderDetail> Details { get; set; }
}