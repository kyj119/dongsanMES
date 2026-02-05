using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using MESSystem.Data;
using MESSystem.Models;

namespace MESSystem.Pages.Admin.Products
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Category> Categories { get; set; } = new();

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "품목코드는 필수입니다.")]
            [StringLength(50, ErrorMessage = "품목코드는 최대 50자까지 입력 가능합니다.")]
            [RegularExpression(@"^[A-Z0-9\-_]+$", ErrorMessage = "품목코드는 대문자, 숫자, -, _ 만 사용 가능합니다.")]
            public string Code { get; set; } = string.Empty;

            [Required(ErrorMessage = "품목명은 필수입니다.")]
            [StringLength(200, ErrorMessage = "품목명은 최대 200자까지 입력 가능합니다.")]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "분류는 필수입니다.")]
            public int CategoryId { get; set; }

            [StringLength(200, ErrorMessage = "기본규격은 최대 200자까지 입력 가능합니다.")]
            public string? DefaultSpec { get; set; }

            public bool IsActive { get; set; } = true;
        }

        public async Task OnGetAsync()
        {
            Categories = await _context.Categories
                .Where(c => c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.CardOrder)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // 분류 목록 재로드 (검증 실패 시 표시용)
            Categories = await _context.Categories
                .Where(c => c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.CardOrder)
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 품목코드 중복 검사
            var existingProduct = await _context.Products
                .AnyAsync(p => p.Code == Input.Code);

            if (existingProduct)
            {
                ModelState.AddModelError("Input.Code", "이미 등록된 품목코드입니다.");
                return Page();
            }

            var product = new Product
            {
                Code = Input.Code.ToUpper(),
                Name = Input.Name,
                CategoryId = Input.CategoryId,
                DefaultSpec = Input.DefaultSpec,
                IsActive = Input.IsActive,
                CreatedAt = DateTime.Now
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"'{product.Name}' 품목이 등록되었습니다.";
            return RedirectToPage("Index");
        }
    }
}
