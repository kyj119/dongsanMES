using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MESSystem.Data;
using MESSystem.Models;

namespace MESSystem.Pages.Cards
{
    public class PrintModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PrintModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Card? Card { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Card = await _context.Cards
                .Include(c => c.Category)
                .Include(c => c.Order)
                .Include(c => c.CardItems)
                    .ThenInclude(ci => ci.OrderItem)
                        .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (Card == null || Card.Order.IsDeleted)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
