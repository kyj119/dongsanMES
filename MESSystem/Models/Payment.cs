using System.ComponentModel.DataAnnotations;

namespace MESSystem.Models;

/// <summary>
/// 입금 관리
/// </summary>
public class Payment
{
    public int Id { get; set; }
    
    /// <summary>
    /// 입금번호 (예: PAY-202602-001)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string PaymentNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// 매출 마감 ID (null 가능 - 선입금)
    /// </summary>
    public int? SalesClosingId { get; set; }
    
    /// <summary>
    /// 거래처 ID
    /// </summary>
    public int ClientId { get; set; }
    
    /// <summary>
    /// 입금일자
    /// </summary>
    public DateTime PaymentDate { get; set; }
    
    /// <summary>
    /// 입금액
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// 입금 방법: 계좌이체/현금/수표/카드
    /// </summary>
    [Required]
    [StringLength(20)]
    public string PaymentMethod { get; set; } = "계좌이체";
    
    /// <summary>
    /// 입금 계좌 (우리 회사 계좌)
    /// </summary>
    [StringLength(50)]
    public string? BankAccount { get; set; }
    
    /// <summary>
    /// 입금자명
    /// </summary>
    [StringLength(50)]
    public string? DepositorName { get; set; }
    
    /// <summary>
    /// 비고
    /// </summary>
    [StringLength(500)]
    public string? Memo { get; set; }
    
    /// <summary>
    /// 통장 거래 ID (자동 연동시)
    /// </summary>
    public int? BankTransactionId { get; set; }
    
    /// <summary>
    /// 매칭 여부
    /// </summary>
    public bool IsMatched { get; set; }
    
    /// <summary>
    /// 매칭 일시
    /// </summary>
    public DateTime? MatchedAt { get; set; }
    
    /// <summary>
    /// 매칭자
    /// </summary>
    [StringLength(50)]
    public string? MatchedBy { get; set; }
    
    /// <summary>
    /// 생성일시
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// 생성자
    /// </summary>
    [StringLength(50)]
    public string? CreatedBy { get; set; }
    
    // Navigation properties
    public SalesClosing? SalesClosing { get; set; }
    public Client Client { get; set; } = null!;
    public BankTransaction? BankTransaction { get; set; }
}
