using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MESSystem.Models;

public class CardItem
{
    public int Id { get; set; }
    
    [Required]
    public int CardId { get; set; }
    
    [Required]
    public int OrderItemId { get; set; } // 원본 추적용
    
    [Required]
    [StringLength(50)]
    public string ProductCode { get; set; } = string.Empty; // 스냅샷
    
    [Required]
    [StringLength(100)]
    public string ProductName { get; set; } = string.Empty; // 스냅샷
    
    [StringLength(100)]
    public string? Spec { get; set; }
    
    [StringLength(200)]
    public string? Description { get; set; }
    
    [Required]
    public int Quantity { get; set; }
    
    [Column(TypeName = "decimal(18, 2)")]
    public decimal? UnitPrice { get; set; }
    
    [StringLength(200)]
    public string? DesignFileName { get; set; }
    
    [StringLength(200)]
    public string? Remark { get; set; }
    
    [Required]
    public int LineNumber { get; set; }
    
    // Navigation properties
    public Card Card { get; set; } = null!;
    public OrderItem OrderItem { get; set; } = null!;
}
