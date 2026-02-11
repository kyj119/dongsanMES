using System.ComponentModel.DataAnnotations;

namespace MESSystem.Models;

/// <summary>
/// 통장 거래 내역 (오픈뱅킹 연동)
/// </summary>
public class BankTransaction
{
    public int Id { get; set; }
    
    /// <summary>
    /// 은행 코드 (예: 004 국민은행)
    /// </summary>
    [Required]
    [StringLength(10)]
    public string BankCode { get; set; } = string.Empty;
    
    /// <summary>
    /// 은행명
    /// </summary>
    [StringLength(50)]
    public string? BankName { get; set; }
    
    /// <summary>
    /// 계좌번호 (마스킹)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string AccountNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// 거래일시
    /// </summary>
    public DateTime TransactionDateTime { get; set; }
    
    /// <summary>
    /// 거래 유형: 입금/출금
    /// </summary>
    [Required]
    [StringLength(10)]
    public string TransactionType { get; set; } = string.Empty;
    
    /// <summary>
    /// 금액
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// 거래 후 잔액
    /// </summary>
    public decimal Balance { get; set; }
    
    /// <summary>
    /// 적요 (거래 내용)
    /// </summary>
    [StringLength(200)]
    public string? Description { get; set; }
    
    /// <summary>
    /// 입금자명 (또는 출금처)
    /// </summary>
    [StringLength(50)]
    public string? CounterpartyName { get; set; }
    
    /// <summary>
    /// 오픈뱅킹 거래고유번호
    /// </summary>
    [StringLength(50)]
    public string? ExternalTransactionId { get; set; }
    
    /// <summary>
    /// 처리 여부
    /// </summary>
    public bool IsProcessed { get; set; }
    
    /// <summary>
    /// 처리 일시
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
    
    /// <summary>
    /// 생성일시 (API 조회 시각)
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    // Navigation properties
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
