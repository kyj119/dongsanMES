using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MESSystem.Data;
using MESSystem.Models;

namespace MESSystem.Pages.Cards
{
    public class DetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailModel(ApplicationDbContext context)
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
                .Include(c => c.EventLogs)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (Card == null || Card.Order.IsDeleted)
            {
                return NotFound();
            }

            return Page();
        }

        // 작업 시작
        public async Task<IActionResult> OnPostStartWorkAsync(int cardId)
        {
            var card = await _context.Cards.FindAsync(cardId);
            if (card == null) return NotFound();

            card.Status = "작업중";
            card.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = cardId });
        }

        // 작업 완료
        public async Task<IActionResult> OnPostCompleteWorkAsync(int cardId)
        {
            var card = await _context.Cards.FindAsync(cardId);
            if (card == null) return NotFound();

            card.Status = "완료";
            card.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = cardId });
        }

        // 보류
        public async Task<IActionResult> OnPostPauseWorkAsync(int cardId)
        {
            var card = await _context.Cards.FindAsync(cardId);
            if (card == null) return NotFound();

            card.Status = "보류";
            card.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = cardId });
        }

        // 재개
        public async Task<IActionResult> OnPostResumeWorkAsync(int cardId)
        {
            var card = await _context.Cards.FindAsync(cardId);
            if (card == null) return NotFound();

            card.Status = "작업중";
            card.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = cardId });
        }

        // 대기로 변경
        public async Task<IActionResult> OnPostResetWorkAsync(int cardId)
        {
            var card = await _context.Cards.FindAsync(cardId);
            if (card == null) return NotFound();

            card.Status = "대기";
            card.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = cardId });
        }
    }
}
