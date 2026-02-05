using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using MESSystem.Data;
using MESSystem.Models;

namespace MESSystem.Pages.Admin.Categories
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "분류명은 필수입니다.")]
            [StringLength(100, ErrorMessage = "분류명은 최대 100자까지 입력 가능합니다.")]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "카드 순서는 필수입니다.")]
            [Range(1, 999, ErrorMessage = "카드 순서는 1~999 사이의 숫자여야 합니다.")]
            public int CardOrder { get; set; } = 1;

            public bool IsActive { get; set; } = true;
        }

        public void OnGet()
        {
            // 다음 카드 순서 자동 설정
            var maxOrder = _context.Categories.Max(c => (int?)c.CardOrder) ?? 0;
            Input.CardOrder = maxOrder + 1;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var category = new Category
            {
                Name = Input.Name,
                CardOrder = Input.CardOrder,
                IsActive = Input.IsActive,
                CreatedAt = DateTime.Now
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"'{category.Name}' 분류가 등록되었습니다.";
            return RedirectToPage("Index");
        }
    }
}
