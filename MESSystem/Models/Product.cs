using System.ComponentModel.DataAnnotations;

namespace MESSystem.Models;

public class Product
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty; // 품목코드 (UNIQUE)
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public int CategoryId { get; set; }
    
    [StringLength(100)]
    public string? DefaultSpec { get; set; } // 기본 규격
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    // 논리 삭제
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    
    // Navigation properties
    public Category Category { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
