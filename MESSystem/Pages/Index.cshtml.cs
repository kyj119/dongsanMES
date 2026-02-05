using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MESSystem.Data;
using MESSystem.Models;

namespace MESSystem.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string? UserRole { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCards { get; set; }
        public int WaitingCards { get; set; }
        public int WorkingCards { get; set; }
        public int CompletedCards { get; set; }
        public int HoldCards { get; set; }
        public List<Order> RecentOrders { get; set; } = new();
        public List<Card> UrgentCards { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login");
            }

            UserRole = HttpContext.Session.GetString("UserRole");

            // 통계 데이터 로드
            TotalOrders = await _context.Orders
                .Where(o => !o.IsDeleted)
                .CountAsync();

            TotalCards = await _context.Cards
                .Where(c => !c.Order.IsDeleted)
                .CountAsync();

            WaitingCards = await _context.Cards
                .Where(c => !c.Order.IsDeleted && c.Status == "대기")
                .CountAsync();

            WorkingCards = await _context.Cards
                .Where(c => !c.Order.IsDeleted && c.Status == "작업중")
                .CountAsync();

            CompletedCards = await _context.Cards
                .Where(c => !c.Order.IsDeleted && c.Status == "완료")
                .CountAsync();

            HoldCards = await _context.Cards
                .Where(c => !c.Order.IsDeleted && c.Status == "보류")
                .CountAsync();

            // 최근 주문서 (관리자용)
            if (UserRole == "관리자")
            {
                RecentOrders = await _context.Orders
                    .Include(o => o.Cards)
                    .Where(o => !o.IsDeleted)
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(10)
                    .ToListAsync();
            }

            // 긴급 작업 (오늘 출고 예정)
            var today = DateTime.Today;
            UrgentCards = await _context.Cards
                .Include(c => c.Category)
                .Include(c => c.Order)
                .Include(c => c.CardItems)
                .Where(c => !c.Order.IsDeleted 
                    && c.Order.ShippingDate == today
                    && c.Status != "완료")
                .ToListAsync();
            
            // SQLite는 TimeSpan을 ORDER BY에서 지원하지 않으므로 메모리에서 정렬
            UrgentCards = UrgentCards
                .OrderBy(c => c.Order.ShippingTime)
                .ThenBy(c => c.CardNumber)
                .ToList();

            return Page();
        }
    }
}
