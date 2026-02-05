using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MESSystem.Data;
using MESSystem.Models;

namespace MESSystem.Pages.Admin.Clients
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Client> Clients { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? IsActive { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Clients.AsQueryable();

            // 검색
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(c => c.Name.Contains(SearchTerm));
            }

            // 활성/비활성 필터
            if (IsActive.HasValue)
            {
                query = query.Where(c => c.IsActive == IsActive.Value);
            }

            Clients = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            client.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "거래처가 비활성화되었습니다.";
            return RedirectToPage();
        }
    }
}
