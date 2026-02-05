using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MESSystem.Data;
using MESSystem.Models;

namespace MESSystem.Pages.Admin.Categories
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Category> Categories { get; set; } = new();

        public async Task OnGetAsync()
        {
            // 모든 분류 조회 (삭제된 것 포함, CardOrder 순)
            Categories = await _context.Categories
                .OrderBy(c => c.CardOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // 논리 삭제
            category.IsDeleted = true;
            category.IsActive = false;
            category.DeletedAt = DateTime.Now;
            category.DeletedBy = User.Identity?.Name ?? "System";

            await _context.SaveChangesAsync();

            TempData["Message"] = $"'{category.Name}' 분류가 삭제되었습니다.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRestoreAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // 복구
            category.IsDeleted = false;
            category.IsActive = true;
            category.DeletedAt = null;
            category.DeletedBy = null;

            await _context.SaveChangesAsync();

            TempData["Message"] = $"'{category.Name}' 분류가 복구되었습니다.";
            return RedirectToPage();
        }
    }
}
