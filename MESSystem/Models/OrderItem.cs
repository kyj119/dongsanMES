using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MESSystem.Models;

public class OrderItem
{
    public int Id { get; set; }
    
    [Required]
    public int OrderId { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
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
    
    [StringLength(500)]
    public string? FilePath { get; set; } // 전체 파일 경로
    
    [StringLength(200)]
    public string? Remark { get; set; }
    
    [Required]
    public int LineNumber { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    // Navigation properties
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public ICollection<CardItem> CardItems { get; set; } = new List<CardItem>();
}
