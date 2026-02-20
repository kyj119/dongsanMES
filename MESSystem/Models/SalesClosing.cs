using System.ComponentModel.DataAnnotations;

namespace MESSystem.Models;

/// <summary>
/// 매출 마감 (이카운트 방식)
/// 거래처별 기간 단위로 주문서를 묶어서 매출 확정
/// </summary>
public class SalesClosing
{
    public int Id { get; set; }
    
    /// <summary>
    /// 마감번호 (예: SC-202602-001)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string ClosingNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// 거래처
    /// </summary>
    public int ClientId { get; set; }
    
    /// <summary>
    /// 마감 시작일
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// 마감 종료일
    /// </summary>
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// 총 공급가액 (주문서 합계)
    /// </summary>
    public decimal SupplyAmount { get; set; }
    
    /// <summary>
    /// 할인 금액
    /// </summary>
    public decimal DiscountAmount { get; set; }
    
    /// <summary>
    /// 추가 금액
    /// </summary>
    public decimal AdditionalAmount { get; set; }
    
    /// <summary>
    /// 조정 후 공급가액
    /// </summary>
    public decimal AdjustedSupplyAmount { get; set; }
    
    /// <summary>
    /// 부가세 (10%)
    /// </summary>
    public decimal VatAmount { get; set; }
    
    /// <summary>
    /// 합계 금액
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// 결제 방법: 세금계산서/카드결제/현금영수증/계좌이체
    /// </summary>
    [Required]
    [StringLength(20)]
    public string PaymentType { get; set; } = "세금계산서";
    
    /// <summary>
    /// 카드 결제 수수료율 (%)
    /// </summary>
    public decimal? CardFeeRate { get; set; }
    
    /// <summary>
    /// 카드 결제 수수료
    /// </summary>
    public decimal? CardFeeAmount { get; set; }
    
    /// <summary>
    /// 비고
    /// </summary>
    [StringLength(500)]
    public string? Memo { get; set; }
    
    /// <summary>
    /// 상태: 임시저장/확정/세금계산서발행/입금완료/취소
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "임시저장";
    
    /// <summary>
    /// 확정일시
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }
    
    /// <summary>
    /// 확정자
    /// </summary>
    [StringLength(50)]
    public string? ConfirmedBy { get; set; }
    
    /// <summary>
    /// 생성일시
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// 생성자
    /// </summary>
    [StringLength(50)]
    public string? CreatedBy { get; set; }
    
    /// <summary>
    /// 수정일시
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// 수정자
    /// </summary>
    [StringLength(50)]
    public string? UpdatedBy { get; set; }
    
    // Navigation properties
    public Client Client { get; set; } = null!;
    public ICollection<SalesClosingItem> Items { get; set; } = new List<SalesClosingItem>();
    public TaxInvoice? TaxInvoice { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
