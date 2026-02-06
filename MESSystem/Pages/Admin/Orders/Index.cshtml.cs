using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MESSystem.Data;
using MESSystem.Models;
using System.Linq;

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
        public string? SearchTerm { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? ShippingMethod { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? DateFilter { get; set; } // 오늘/이번주/이번달/전체
        
        [BindProperty(SupportsGet = true)]
        public string? SortBy { get; set; } // 최신순/출고일순/우선순위순

        public async Task OnGetAsync()
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Cards)
                .AsQueryable();

            // 검색어 필터 (주문번호, 거래처명)
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var searchLower = SearchTerm.ToLower();
                query = query.Where(o => 
                    o.OrderNumber.ToLower().Contains(searchLower) || 
                    o.ClientName.ToLower().Contains(searchLower));
            }
            
            // 출고방법 필터
            if (!string.IsNullOrEmpty(ShippingMethod))
            {
                query = query.Where(o => o.ShippingMethod == ShippingMethod);
            }
            
            // 날짜 범위 필터
            if (!string.IsNullOrEmpty(DateFilter))
            {
                var today = DateTime.Today;
                switch (DateFilter)
                {
                    case "오늘":
                        query = query.Where(o => o.ShippingDate.Date == today);
                        break;
                    case "이번주":
                        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                        var endOfWeek = startOfWeek.AddDays(7);
                        query = query.Where(o => o.ShippingDate >= startOfWeek && o.ShippingDate < endOfWeek);
                        break;
                    case "이번달":
                        var startOfMonth = new DateTime(today.Year, today.Month, 1);
                        var endOfMonth = startOfMonth.AddMonths(1);
                        query = query.Where(o => o.ShippingDate >= startOfMonth && o.ShippingDate < endOfMonth);
                        break;
                    // "전체"는 필터 없음
                }
            }

            // 정렬
            query = query.OrderBy(o => o.IsDeleted); // 삭제되지 않은 것 우선
            
            if (!string.IsNullOrEmpty(SortBy))
            {
                query = SortBy switch
                {
                    "출고일순" => query.ThenBy(o => o.ShippingDate).ThenBy(o => o.ShippingTime),
                    "우선순위순" => query.ThenBy(o => o.Priority).ThenBy(o => o.ShippingDate),
                    _ => query.ThenByDescending(o => o.CreatedAt) // 최신순 (기본값)
                };
            }
            else
            {
                query = query.ThenByDescending(o => o.CreatedAt); // 기본값: 최신순
            }

            Orders = await query.ToListAsync();
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
