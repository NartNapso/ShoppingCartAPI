using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace ShoppingCartAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private static List<CartItem> _cart = new List<CartItem>();

        private static Dictionary<string, List<CartItem>> GetGroupedCart()
        {
            return _cart
                .GroupBy(item => item.Category)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        [HttpGet]
        public IActionResult GetCart()
        {
            return Ok(GetGroupedCart());
        }

        [HttpPost]
        public IActionResult AddToCart([FromBody] CartItem item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.Category))
            {
                return BadRequest(new { error = "Invalid item: Name and Category are required." });
            }

            item.Id = _cart.Count > 0 ? _cart.Max(i => i.Id) + 1 : 1;
            _cart.Add(item);

            return Ok(GetGroupedCart());
        }

        [HttpDelete("{id}")]
        public IActionResult RemoveFromCart(int id)
        {
            var item = _cart.FirstOrDefault(i => i.Id == id);
            if (item == null) return NotFound();

            _cart.Remove(item);
            return Ok(GetGroupedCart());
        }

        [HttpDelete]
        public IActionResult ClearCart()
        {
            _cart.Clear();
            return Ok(new { message = "Cart cleared successfully!" });
        }
    }

    public class CartItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string Category { get; set; }
    }
}
