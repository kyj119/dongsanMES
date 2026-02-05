using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MESSystem.Data;

namespace MESSystem.Pages.Account;

public class LoginModel : PageModel
{
    private readonly ApplicationDbContext _db;
    
    public LoginModel(ApplicationDbContext db)
    {
        _db = db;
    }
    
    [BindProperty]
    public string Username { get; set; } = string.Empty;
    
    [BindProperty]
    public string Password { get; set; } = string.Empty;
    
    public string? ErrorMessage { get; set; }
    
    public void OnGet()
    {
        // 이미 로그인된 경우 리다이렉트
        var userId = HttpContext.Session.GetString("UserId");
        if (!string.IsNullOrEmpty(userId))
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role == "관리자")
            {
                Response.Redirect("/Admin/Orders/Index");
            }
            else
            {
                Response.Redirect("/Cards/Index");
            }
        }
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            ErrorMessage = "아이디와 비밀번호를 입력하세요.";
            return Page();
        }
        
        // 사용자 조회 (TODO: 실제 환경에서는 비밀번호 해시 비교)
        var user = await _db.Users
            .Where(u => u.Username == Username && u.Password == Password && u.IsActive)
            .FirstOrDefaultAsync();
        
        if (user == null)
        {
            ErrorMessage = "아이디 또는 비밀번호가 올바르지 않습니다.";
            return Page();
        }
        
        // 세션에 사용자 정보 저장
        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserRole", user.Role);
        HttpContext.Session.SetString("UserName", user.FullName);
        
        // 대시보드로 리다이렉트
        return RedirectToPage("/Index");
    }
}
