using System.ComponentModel.DataAnnotations;

namespace MESSystem.Models;

public class Client
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty; // 거래처명
    
    [StringLength(200)]
    public string? Address { get; set; } // 주소
    
    [StringLength(20)]
    public string? Phone { get; set; } // 전화번호
    
    [StringLength(20)]
    public string? Mobile { get; set; } // 휴대폰
    
    [StringLength(100)]
    public string? Email { get; set; } // 이메일
    
    [StringLength(200)]
    public string? Note { get; set; } // 비고
    
    // 세금계산서 발행용 추가 정보
    [StringLength(12)]
    public string? BusinessNumber { get; set; } // 사업자등록번호
    
    [StringLength(50)]
    public string? CeoName { get; set; } // 대표자명
    
    [StringLength(50)]
    public string? BusinessType { get; set; } // 업태
    
    [StringLength(50)]
    public string? BusinessItem { get; set; } // 종목
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    // Navigation properties
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<SalesClosing> SalesClosings { get; set; } = new List<SalesClosing>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
