using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MESSystem.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        // 로그인 여부 확인
        var userId = HttpContext.Session.GetString("UserId");
        
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Account/Login");
        }
        
        var userRole = HttpContext.Session.GetString("UserRole");
        
        // 역할별 리다이렉트
        if (userRole == "관리자")
        {
            return RedirectToPage("/Admin/Orders/Index");
        }
        else
        {
            return RedirectToPage("/Cards/Index");
        }
    }
}
