using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MESSystem.Data;
using MESSystem.Models;

namespace MESSystem.Pages.Admin.Orders
{
    public class DetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Order Order { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Category)
                .Include(o => o.Cards)
                    .ThenInclude(c => c.Category)
                .Include(o => o.Cards)
                    .ThenInclude(c => c.CardItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            Order = order;
            return Page();
        }
    }
}
