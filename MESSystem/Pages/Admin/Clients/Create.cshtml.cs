using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MESSystem.Data;
using MESSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace MESSystem.Pages.Admin.Clients
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
            [Required(ErrorMessage = "거래처명을 입력하세요.")]
            [StringLength(100)]
            public string Name { get; set; } = string.Empty;

            [StringLength(200)]
            public string? Address { get; set; }

            [StringLength(20)]
            public string? Phone { get; set; }

            [StringLength(20)]
            public string? Mobile { get; set; }

            [StringLength(100)]
            [EmailAddress(ErrorMessage = "올바른 이메일 형식이 아닙니다.")]
            public string? Email { get; set; }

            [StringLength(200)]
            public string? Note { get; set; }

            public bool IsActive { get; set; } = true;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var client = new Client
            {
                Name = Input.Name,
                Address = Input.Address,
                Phone = Input.Phone,
                Mobile = Input.Mobile,
                Email = Input.Email,
                Note = Input.Note,
                IsActive = Input.IsActive,
                CreatedAt = DateTime.Now
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "거래처가 등록되었습니다.";
            return RedirectToPage("Index");
        }
    }
}
