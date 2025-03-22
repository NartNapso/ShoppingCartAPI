using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCartAPI.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

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
            try
            {
                var cart = await GetGroupedCartAsync();
                Log.Information("Retrieved cart with {ItemCount} items", cart.Sum(g => g.Value.Count));
                return Ok(cart);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve cart.");
                return StatusCode(500, "An error occurred while retrieving the cart.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] CartItem item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.Category))
            {
                Log.Warning("Invalid item submission.");
                return BadRequest(new { error = "Invalid item: Name and Category are required." });
            }

            try
            {
                var newItem = new CartItem
                {
                    Name = item.Name,
                    Quantity = item.Quantity,
                    Category = item.Category
                };

                _context.CartItems.Add(newItem);
                await _context.SaveChangesAsync();

                Log.Information("Added item to cart: {Item}", newItem.Name);
                return Ok(await _context.CartItems.ToListAsync());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to add item to cart.");
                return StatusCode(500, "An error occurred while adding the item.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            try
            {
                var item = await _context.CartItems.FindAsync(id);
                if (item == null)
                {
                    Log.Warning("Tried to delete non-existing item with ID {Id}", id);
                    return NotFound();
                }

                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();

                Log.Information("Removed item with ID {Id}", id);
                return Ok(await GetGroupedCartAsync());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to remove item from cart.");
                return StatusCode(500, "An error occurred while removing the item.");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                _context.CartItems.RemoveRange(_context.CartItems);
                await _context.SaveChangesAsync();
                Log.Information("Cleared the cart.");
                return Ok(new { message = "Cart cleared successfully!" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to clear cart.");
                return StatusCode(500, "An error occurred while clearing the cart.");
            }
        }
    }
}
