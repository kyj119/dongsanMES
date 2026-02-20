using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MESSystem.Data;
using MESSystem.Models;

namespace MESSystem.Pages.Admin;

public class DataCheckModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DataCheckModel(ApplicationDbContext context)
    {
        _context = context;
    }

    // 거래처 정보
    public List<Client> Clients { get; set; } = new();

    // ERP 테이블 상태
    public bool SalesClosingsTableExists { get; set; }
    public bool TaxInvoicesTableExists { get; set; }
    public bool PaymentsTableExists { get; set; }
    public int SalesClosingsCount { get; set; }
    public int TaxInvoicesCount { get; set; }
    public int PaymentsCount { get; set; }

    // 주문서 상태
    public int CompletedOrdersCount { get; set; }
    public int SalesClosedOrdersCount { get; set; }
    public int InProgressOrdersCount { get; set; }
    public List<Order> CompletedOrders { get; set; } = new();

    public async Task OnGetAsync()
    {
        // 거래처 정보 조회
        Clients = await _context.Clients
            .OrderBy(c => c.Id)
            .ToListAsync();

        // ERP 테이블 존재 여부 및 데이터 수 확인
        try
        {
            SalesClosingsCount = await _context.SalesClosings.CountAsync();
            SalesClosingsTableExists = true;
        }
        catch
        {
            SalesClosingsTableExists = false;
            SalesClosingsCount = 0;
        }

        try
        {
            TaxInvoicesCount = await _context.TaxInvoices.CountAsync();
            TaxInvoicesTableExists = true;
        }
        catch
        {
            TaxInvoicesTableExists = false;
            TaxInvoicesCount = 0;
        }

        try
        {
            PaymentsCount = await _context.Payments.CountAsync();
            PaymentsTableExists = true;
        }
        catch
        {
            PaymentsTableExists = false;
            PaymentsCount = 0;
        }

        // 주문서 상태별 카운트
        CompletedOrdersCount = await _context.Orders
            .Where(o => !o.IsDeleted && o.Status == "완료" && !o.IsSalesClosed)
            .CountAsync();

        SalesClosedOrdersCount = await _context.Orders
            .Where(o => !o.IsDeleted && o.IsSalesClosed)
            .CountAsync();

        InProgressOrdersCount = await _context.Orders
            .Where(o => !o.IsDeleted && o.Status == "진행중")
            .CountAsync();

        // 완료된 주문서 목록
        CompletedOrders = await _context.Orders
            .Where(o => !o.IsDeleted && o.Status == "완료")
            .OrderByDescending(o => o.UpdatedAt)
            .Take(20)
            .ToListAsync();
    }
}
