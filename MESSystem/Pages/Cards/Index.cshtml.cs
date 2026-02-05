using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MESSystem.Data;
using MESSystem.Models;

namespace MESSystem.Pages.Cards
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Card> Cards { get; set; } = new();
        public List<Category> Categories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public async Task OnGetAsync()
        {
            // 카테고리 목록 로드
            Categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.CardOrder)
                .ToListAsync();

            // 카드 목록 조회
            var query = _context.Cards
                .Include(c => c.Category)
                .Include(c => c.Order)
                .Include(c => c.CardItems)
                    .ThenInclude(ci => ci.OrderItem)
                        .ThenInclude(oi => oi.Product)
                .Where(c => !c.Order.IsDeleted);  // 삭제된 주문의 카드 제외

            // 상태 필터
            if (!string.IsNullOrEmpty(Status))
            {
                query = query.Where(c => c.Status == Status);
            }

            // 분류 필터
            if (CategoryId.HasValue)
            {
                query = query.Where(c => c.CategoryId == CategoryId.Value);
            }

            // 검색어 필터
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(c => c.CardNumber.Contains(SearchTerm) ||
                                        c.CardItems.Any(ci => ci.OrderItem.Product.Name.Contains(SearchTerm)));
            }

            // 정렬: 출고일시 오름차순 (가장 빨리 출고해야 할 것부터)
            Cards = await query
                .OrderBy(c => c.Order.ShippingDate)
                .ThenBy(c => c.Order.ShippingTime)
                .ThenBy(c => c.CardNumber)
                .ToListAsync();
        }
    }
}
