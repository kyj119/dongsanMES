using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MESSystem.Data;
using MESSystem.Models;

namespace MESSystem.Pages.Admin.Products
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public async Task OnGetAsync()
        {
            // 활성 분류 목록 조회
            Categories = await _context.Categories
                .Where(c => c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.CardOrder)
                .ToListAsync();

            // 품목 조회 쿼리 시작
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            // 분류 필터
            if (CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == CategoryId.Value);
            }

            // 검색어 필터
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var searchLower = SearchTerm.ToLower();
                query = query.Where(p => 
                    p.Code.ToLower().Contains(searchLower) || 
                    p.Name.ToLower().Contains(searchLower));
            }

            // 정렬: 삭제되지 않은 것 우선, 분류 순서, 품목코드 순
            Products = await query
                .OrderBy(p => p.IsDeleted)
                .ThenBy(p => p.Category!.CardOrder)
                .ThenBy(p => p.Code)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // 논리 삭제
            product.IsDeleted = true;
            product.IsActive = false;
            product.DeletedAt = DateTime.Now;
            product.DeletedBy = User.Identity?.Name ?? "System";

            await _context.SaveChangesAsync();

            TempData["Message"] = $"'{product.Name}' 품목이 삭제되었습니다.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRestoreAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // 복구
            product.IsDeleted = false;
            product.IsActive = true;
            product.DeletedAt = null;
            product.DeletedBy = null;

            await _context.SaveChangesAsync();

            TempData["Message"] = $"'{product.Name}' 품목이 복구되었습니다.";
            return RedirectToPage();
        }
    }
}
