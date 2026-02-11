using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MESSystem.Data;
using MESSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace MESSystem.Pages.Admin.Users;

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

        public string Username { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 6, ErrorMessage = "비밀번호는 최소 6자 이상이어야 합니다.")]
        public string? NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "비밀번호가 일치하지 않습니다.")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "이름은 필수입니다.")]
        [StringLength(50, ErrorMessage = "이름은 최대 50자까지 가능합니다.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "권한을 선택하세요.")]
        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        // 권한 체크: 관리자만 접근 가능
        var currentRole = HttpContext.Session.GetString("Role");
        
        if (currentRole != "관리자")
        {
            TempData["ErrorMessage"] = "관리자만 접근할 수 있습니다.";
            return RedirectToPage("/Index");
        }

        if (id == null)
        {
            return NotFound();
        }

        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

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

        var user = await _context.Users.FindAsync(Input.Id);

        if (user == null)
        {
            return NotFound();
        }

        // admin 계정의 권한과 상태는 변경 불가
        if (user.Username == "admin")
        {
            user.FullName = Input.FullName;
            
            // 비밀번호 변경
            if (!string.IsNullOrEmpty(Input.NewPassword))
            {
                user.Password = Input.NewPassword; // TODO: 해시 필요
            }
        }
        else
        {
            user.FullName = Input.FullName;
            user.Role = Input.Role;
            user.IsActive = Input.IsActive;
            
            // 비밀번호 변경
            if (!string.IsNullOrEmpty(Input.NewPassword))
            {
                user.Password = Input.NewPassword; // TODO: 해시 필요
            }
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"사용자 '{user.Username}'이(가) 수정되었습니다.";
        return RedirectToPage("./Index");
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        // 권한 체크
        var currentRole = HttpContext.Session.GetString("Role");
        var currentUsername = HttpContext.Session.GetString("Username");
        
        if (currentRole != "관리자")
        {
            TempData["ErrorMessage"] = "관리자만 접근할 수 있습니다.";
            return RedirectToPage("/Index");
        }

        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            TempData["ErrorMessage"] = "사용자를 찾을 수 없습니다.";
            return RedirectToPage("./Index");
        }

        // admin 계정은 삭제 불가
        if (user.Username == "admin")
        {
            TempData["ErrorMessage"] = "시스템 관리자 계정은 삭제할 수 없습니다.";
            return RedirectToPage("./Index");
        }

        // 자기 자신 삭제 불가
        if (user.Username == currentUsername)
        {
            TempData["ErrorMessage"] = "현재 로그인한 계정은 삭제할 수 없습니다.";
            return RedirectToPage("./Index");
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"사용자 '{user.Username}'이(가) 삭제되었습니다.";
        return RedirectToPage("./Index");
    }
}
