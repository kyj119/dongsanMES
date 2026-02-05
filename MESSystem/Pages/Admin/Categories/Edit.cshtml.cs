using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using MESSystem.Data;
using MESSystem.Models;

namespace MESSystem.Pages.Admin.Categories
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "분류명은 필수입니다.")]
            [StringLength(100, ErrorMessage = "분류명은 최대 100자까지 입력 가능합니다.")]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "카드 순서는 필수입니다.")]
            [Range(1, 999, ErrorMessage = "카드 순서는 1~999 사이의 숫자여야 합니다.")]
            public int CardOrder { get; set; } = 1;

            public bool IsActive { get; set; } = true;

            public DateTime CreatedAt { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            Input = new InputModel
            {
                Id = category.Id,
                Name = category.Name,
                CardOrder = category.CardOrder,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var category = await _context.Categories.FindAsync(Input.Id);
            if (category == null)
            {
                return NotFound();
            }

            category.Name = Input.Name;
            category.CardOrder = Input.CardOrder;
            category.IsActive = Input.IsActive;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CategoryExists(Input.Id))
                {
                    return NotFound();
                }
                throw;
            }

            TempData["Message"] = $"'{category.Name}' 분류가 수정되었습니다.";
            return RedirectToPage("Index");
        }

        private async Task<bool> CategoryExists(int id)
        {
            return await _context.Categories.AnyAsync(c => c.Id == id);
        }
    }
}
