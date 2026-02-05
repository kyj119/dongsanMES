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
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    // Navigation properties
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
