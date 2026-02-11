using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MESSystem.Data;
using MESSystem.Models;

namespace MESSystem.Pages.Admin.Users;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<User> Users { get; set; } = new();
    
    [BindProperty(SupportsGet = true)]
    public string? SearchUsername { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public string? SearchFullName { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public string? SearchRole { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public string? SearchStatus { get; set; }
    
    // 통계
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int AdminUsers { get; set; }
    public int RegularUsers { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // 권한 체크: 관리자만 접근 가능
        var currentUser = HttpContext.Session.GetString("Username");
        var currentRole = HttpContext.Session.GetString("Role");
        
        if (string.IsNullOrEmpty(currentUser) || currentRole != "관리자")
        {
            TempData["ErrorMessage"] = "관리자만 접근할 수 있습니다.";
            return RedirectToPage("/Index");
        }

        // 기본 쿼리
        var query = _context.Users.AsQueryable();

        // 검색 필터 적용
        if (!string.IsNullOrEmpty(SearchUsername))
        {
            query = query.Where(u => u.Username.Contains(SearchUsername));
        }

        if (!string.IsNullOrEmpty(SearchFullName))
        {
            query = query.Where(u => u.FullName.Contains(SearchFullName));
        }

        if (!string.IsNullOrEmpty(SearchRole))
        {
            query = query.Where(u => u.Role == SearchRole);
        }

        if (!string.IsNullOrEmpty(SearchStatus))
        {
            bool isActive = SearchStatus == "active";
            query = query.Where(u => u.IsActive == isActive);
        }

        // 정렬 (ID 순)
        Users = await query.OrderBy(u => u.Id).ToListAsync();

        // 통계 계산
        TotalUsers = await _context.Users.CountAsync();
        ActiveUsers = await _context.Users.CountAsync(u => u.IsActive);
        AdminUsers = await _context.Users.CountAsync(u => u.Role == "관리자");
        RegularUsers = await _context.Users.CountAsync(u => u.Role == "사용자");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        // 권한 체크
        var currentUser = HttpContext.Session.GetString("Username");
        var currentRole = HttpContext.Session.GetString("Role");
        
        if (string.IsNullOrEmpty(currentUser) || currentRole != "관리자")
        {
            TempData["ErrorMessage"] = "관리자만 접근할 수 있습니다.";
            return RedirectToPage("/Index");
        }

        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            TempData["ErrorMessage"] = "사용자를 찾을 수 없습니다.";
            return RedirectToPage();
        }

        // admin 계정은 삭제 불가
        if (user.Username == "admin")
        {
            TempData["ErrorMessage"] = "시스템 관리자 계정은 삭제할 수 없습니다.";
            return RedirectToPage();
        }

        // 자기 자신 삭제 불가
        if (user.Username == currentUser)
        {
            TempData["ErrorMessage"] = "현재 로그인한 계정은 삭제할 수 없습니다.";
            return RedirectToPage();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"사용자 '{user.Username}'이(가) 삭제되었습니다.";
        return RedirectToPage();
    }
}
