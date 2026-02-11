using System.ComponentModel.DataAnnotations;

namespace MESSystem.Models;

/// <summary>
/// 매출 마감 항목 (주문서 단위)
/// </summary>
public class SalesClosingItem
{
    public int Id { get; set; }
    
    /// <summary>
    /// 매출 마감 ID
    /// </summary>
    public int SalesClosingId { get; set; }
    
    /// <summary>
    /// 주문서 ID
    /// </summary>
    public int OrderId { get; set; }
    
    /// <summary>
    /// 주문번호 (스냅샷)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string OrderNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// 주문 일자 (스냅샷)
    /// </summary>
    public DateTime OrderDate { get; set; }
    
    /// <summary>
    /// 공급가액
    /// </summary>
    public decimal SupplyAmount { get; set; }
    
    /// <summary>
    /// 항목별 할인 (선택사항)
    /// </summary>
    public decimal DiscountAmount { get; set; }
    
    /// <summary>
    /// 비고
    /// </summary>
    [StringLength(200)]
    public string? Memo { get; set; }
    
    // Navigation properties
    public SalesClosing SalesClosing { get; set; } = null!;
    public Order Order { get; set; } = null!;
}
