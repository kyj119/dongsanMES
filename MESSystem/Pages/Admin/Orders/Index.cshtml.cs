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
    }
}
