namespace ShoppingCartAPI.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string Category { get; set; }
    }
}
