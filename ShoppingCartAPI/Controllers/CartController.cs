using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCartAPI.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCartAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ShoppingCartDbContext _context;

        public CartController(ShoppingCartDbContext context)
        {
            _context = context;
        }

        private async Task<Dictionary<string, List<CartItem>>> GetGroupedCartAsync()
        {
            var items = await _context.CartItems.ToListAsync();
            return items.GroupBy(item => item.Category)
                        .ToDictionary(g => g.Key, g => g.ToList());
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var cart = await GetGroupedCartAsync();
            return Ok(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] CartItem item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.Category))
            {
                return BadRequest(new { error = "Invalid item: Name and Category are required." });
            }

            var newItem = new CartItem
            {
                Name = item.Name,
                Quantity = item.Quantity,
                Category = item.Category
            };

            _context.CartItems.Add(newItem);
            await _context.SaveChangesAsync();

            return Ok(await _context.CartItems.ToListAsync());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var item = await _context.CartItems.FindAsync(id);
            if (item == null) return NotFound();

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(await GetGroupedCartAsync());
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            _context.CartItems.RemoveRange(_context.CartItems);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cart cleared successfully!" });
        }
    }
}
