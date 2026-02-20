using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MESSystem.Data;
using MESSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace MESSystem.Pages.Admin.Users;

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
        [Required(ErrorMessage = "사용자명은 필수입니다.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "사용자명은 3~50자여야 합니다.")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "사용자명은 영문과 숫자만 가능합니다.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "비밀번호는 필수입니다.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "비밀번호는 최소 6자 이상이어야 합니다.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "비밀번호 확인은 필수입니다.")]
        [Compare("Password", ErrorMessage = "비밀번호가 일치하지 않습니다.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "이름은 필수입니다.")]
        [StringLength(50, ErrorMessage = "이름은 최대 50자까지 가능합니다.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "권한을 선택하세요.")]
        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public IActionResult OnGet()
    {
        // 권한 체크: 관리자만 접근 가능
        var currentRole = HttpContext.Session.GetString("Role");
        
        if (currentRole != "관리자")
        {
            TempData["ErrorMessage"] = "관리자만 접근할 수 있습니다.";
            return RedirectToPage("/Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // 권한 체크
        var currentRole = HttpContext.Session.GetString("Role");
        
        if (currentRole != "관리자")
        {
            TempData["ErrorMessage"] = "관리자만 접근할 수 있습니다.";
            return RedirectToPage("/Index");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // 중복 사용자명 체크
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == Input.Username);

        if (existingUser != null)
        {
            ModelState.AddModelError("Input.Username", "이미 사용 중인 사용자명입니다.");
            return Page();
        }

        // 새 사용자 생성
        var user = new User
        {
            Username = Input.Username,
            Password = Input.Password, // TODO: 실제 환경에서는 해시 필요
            FullName = Input.FullName,
            Role = Input.Role,
            IsActive = Input.IsActive,
            CreatedAt = DateTime.Now
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"사용자 '{user.Username}'이(가) 생성되었습니다.";
        return RedirectToPage("./Index");
    }
}
