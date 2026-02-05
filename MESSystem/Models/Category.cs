using System.ComponentModel.DataAnnotations;

namespace MESSystem.Models;

public class Category
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public int CardOrder { get; set; } // 카드 고정 순서 (1=태극기, 2=현수막, 3=간판)
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    // Navigation properties
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Card> Cards { get; set; } = new List<Card>();
}
