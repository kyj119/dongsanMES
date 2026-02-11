using System.ComponentModel.DataAnnotations;

namespace MESSystem.Models;

/// <summary>
/// 세금계산서 품목 (공급 내역)
/// </summary>
public class TaxInvoiceItem
{
    public int Id { get; set; }
    
    /// <summary>
    /// 세금계산서 ID
    /// </summary>
    public int TaxInvoiceId { get; set; }
    
    /// <summary>
    /// 월/일
    /// </summary>
    [StringLength(10)]
    public string? ItemDate { get; set; }
    
    /// <summary>
    /// 품목명
    /// </summary>
    [Required]
    [StringLength(100)]
    public string ItemName { get; set; } = string.Empty;
    
    /// <summary>
    /// 규격
    /// </summary>
    [StringLength(50)]
    public string? Specification { get; set; }
    
    /// <summary>
    /// 수량
    /// </summary>
    public decimal Quantity { get; set; }
    
    /// <summary>
    /// 단가
    /// </summary>
    public decimal UnitPrice { get; set; }
    
    /// <summary>
    /// 공급가액
    /// </summary>
    public decimal SupplyAmount { get; set; }
    
    /// <summary>
    /// 세액
    /// </summary>
    public decimal TaxAmount { get; set; }
    
    /// <summary>
    /// 비고
    /// </summary>
    [StringLength(200)]
    public string? Memo { get; set; }
    
    // Navigation properties
    public TaxInvoice TaxInvoice { get; set; } = null!;
}
