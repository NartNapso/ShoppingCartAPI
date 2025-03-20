using Microsoft.EntityFrameworkCore;

namespace ShoppingCartAPI.Data
{
    public class ShoppingCartDbContext : DbContext
    {
        public ShoppingCartDbContext(DbContextOptions<ShoppingCartDbContext> options) : base(options) { }

        public DbSet<CartItem> CartItems { get; set; }
    }

    public class CartItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string Category { get; set; }
    }
}
