using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MESSystem.Data;
using MESSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace MESSystem.Pages.Admin.Clients
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

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            Input = new InputModel
            {
                Name = client.Name,
                Address = client.Address,
                Phone = client.Phone,
                Mobile = client.Mobile,
                Email = client.Email,
                Note = client.Note,
                IsActive = client.IsActive
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            client.Name = Input.Name;
            client.Address = Input.Address;
            client.Phone = Input.Phone;
            client.Mobile = Input.Mobile;
            client.Email = Input.Email;
            client.Note = Input.Note;
            client.IsActive = Input.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "거래처가 수정되었습니다.";
            return RedirectToPage("Index");
        }
    }
}
