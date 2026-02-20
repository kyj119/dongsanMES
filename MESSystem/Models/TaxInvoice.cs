using System.ComponentModel.DataAnnotations;

namespace MESSystem.Models;

/// <summary>
/// 세금계산서 (홈택스 연동)
/// </summary>
public class TaxInvoice
{
    public int Id { get; set; }
    
    /// <summary>
    /// 매출 마감 ID
    /// </summary>
    public int SalesClosingId { get; set; }
    
    /// <summary>
    /// 승인번호 (국세청 발급)
    /// </summary>
    [StringLength(24)]
    public string? ApprovalNumber { get; set; }
    
    /// <summary>
    /// 작성일자
    /// </summary>
    public DateTime IssueDate { get; set; }
    
    /// <summary>
    /// 발행 유형: 정발행/역발행
    /// </summary>
    [Required]
    [StringLength(10)]
    public string IssueType { get; set; } = "정발행";
    
    /// <summary>
    /// 영수/청구 구분
    /// </summary>
    [Required]
    [StringLength(10)]
    public string PurposeType { get; set; } = "영수";
    
    /// <summary>
    /// 공급가액
    /// </summary>
    public decimal SupplyAmount { get; set; }
    
    /// <summary>
    /// 세액 (부가세)
    /// </summary>
    public decimal TaxAmount { get; set; }
    
    /// <summary>
    /// 합계 금액
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// 공급자 사업자등록번호
    /// </summary>
    [Required]
    [StringLength(12)]
    public string SupplierBusinessNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// 공급자 상호
    /// </summary>
    [Required]
    [StringLength(100)]
    public string SupplierName { get; set; } = string.Empty;
    
    /// <summary>
    /// 공급자 대표자명
    /// </summary>
    [Required]
    [StringLength(50)]
    public string SupplierCeoName { get; set; } = string.Empty;
    
    /// <summary>
    /// 공급자 주소
    /// </summary>
    [StringLength(200)]
    public string? SupplierAddress { get; set; }
    
    /// <summary>
    /// 공급자 업태
    /// </summary>
    [StringLength(50)]
    public string? SupplierBusinessType { get; set; }
    
    /// <summary>
    /// 공급자 종목
    /// </summary>
    [StringLength(50)]
    public string? SupplierBusinessItem { get; set; }
    
    /// <summary>
    /// 공급자 이메일
    /// </summary>
    [StringLength(100)]
    public string? SupplierEmail { get; set; }
    
    /// <summary>
    /// 공급받는자 사업자등록번호
    /// </summary>
    [Required]
    [StringLength(12)]
    public string BuyerBusinessNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// 공급받는자 상호
    /// </summary>
    [Required]
    [StringLength(100)]
    public string BuyerName { get; set; } = string.Empty;
    
    /// <summary>
    /// 공급받는자 대표자명
    /// </summary>
    [Required]
    [StringLength(50)]
    public string BuyerCeoName { get; set; } = string.Empty;
    
    /// <summary>
    /// 공급받는자 주소
    /// </summary>
    [StringLength(200)]
    public string? BuyerAddress { get; set; }
    
    /// <summary>
    /// 공급받는자 업태
    /// </summary>
    [StringLength(50)]
    public string? BuyerBusinessType { get; set; }
    
    /// <summary>
    /// 공급받는자 종목
    /// </summary>
    [StringLength(50)]
    public string? BuyerBusinessItem { get; set; }
    
    /// <summary>
    /// 공급받는자 이메일
    /// </summary>
    [StringLength(100)]
    public string? BuyerEmail { get; set; }
    
    /// <summary>
    /// 비고
    /// </summary>
    [StringLength(500)]
    public string? Memo { get; set; }
    
    /// <summary>
    /// 상태: 작성중/발행대기/발행완료/전송완료/취소
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "작성중";
    
    /// <summary>
    /// 발행일시
    /// </summary>
    public DateTime? IssuedAt { get; set; }
    
    /// <summary>
    /// 전송일시 (이메일)
    /// </summary>
    public DateTime? SentAt { get; set; }
    
    /// <summary>
    /// 홈택스 전송 여부
    /// </summary>
    public bool IsSubmittedToHometax { get; set; }
    
    /// <summary>
    /// 홈택스 전송일시
    /// </summary>
    public DateTime? SubmittedToHometaxAt { get; set; }
    
    /// <summary>
    /// 홈택스 응답 메시지
    /// </summary>
    [StringLength(500)]
    public string? HometaxResponse { get; set; }
    
    /// <summary>
    /// XML 파일 경로 (전자문서)
    /// </summary>
    [StringLength(500)]
    public string? XmlFilePath { get; set; }
    
    /// <summary>
    /// PDF 파일 경로
    /// </summary>
    [StringLength(500)]
    public string? PdfFilePath { get; set; }
    
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
    
    // Navigation properties
    public SalesClosing SalesClosing { get; set; } = null!;
    public ICollection<TaxInvoiceItem> Items { get; set; } = new List<TaxInvoiceItem>();
}
