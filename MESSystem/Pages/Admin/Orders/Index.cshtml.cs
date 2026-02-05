using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MESSystem.Data;
using MESSystem.Models;

namespace MESSystem.Pages.Admin.Orders
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Order> Orders { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Cards)
                .AsQueryable();

            // 상태 필터
            if (!string.IsNullOrEmpty(Status))
            {
                query = query.Where(o => o.Status == Status);
            }

            // 검색어 필터
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var searchLower = SearchTerm.ToLower();
                query = query.Where(o => 
                    o.OrderNumber.ToLower().Contains(searchLower) || 
                    o.ClientName.ToLower().Contains(searchLower));
            }

            // 정렬: 삭제되지 않은 것 우선, 최신순
            Orders = await query
                .OrderBy(o => o.IsDeleted)
                .ThenByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id, string? reason)
        {
            var order = await _context.Orders
                .Include(o => o.Cards)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // 논리 삭제
            order.IsDeleted = true;
            order.DeletedAt = DateTime.Now;
            order.DeletedBy = User.Identity?.Name ?? "System";
            order.DeleteReason = reason ?? "사용자 삭제";
            order.Status = "취소";

            // 연결된 카드도 비활성화
            foreach (var card in order.Cards)
            {
                card.Status = "취소";
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = $"주문서 '{order.OrderNumber}'가 삭제되었습니다.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRestoreAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Cards)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // 복구
            order.IsDeleted = false;
            order.DeletedAt = null;
            order.DeletedBy = null;
            order.DeleteReason = null;
            order.Status = "작성";

            // 연결된 카드도 복구
            foreach (var card in order.Cards)
            {
                card.Status = "대기";
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = $"주문서 '{order.OrderNumber}'가 복구되었습니다.";
            return RedirectToPage();
        }
    }
}
