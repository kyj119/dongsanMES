using System.ComponentModel.DataAnnotations;

namespace MESSystem.Models;

public class Card
{
    public int Id { get; set; }
    
    [Required]
    public int OrderId { get; set; }
    
    [Required]
    public int CategoryId { get; set; }
    
    [Required]
    [StringLength(30)]
    public string CardNumber { get; set; } = string.Empty; // 20260204-01-1
    
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "대기"; // 대기/작업중/완료
    
    public bool IsModified { get; set; } = false; // 주문서 수정 알림
    
    public DateTime? ModifiedAt { get; set; }
    
    public DateTime? ModifiedAcknowledgedAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime? UpdatedAt { get; set; }  // 상태 변경 시간
    
    // Navigation properties
    public Order Order { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ICollection<CardItem> CardItems { get; set; } = new List<CardItem>();
    public ICollection<EventLog> EventLogs { get; set; } = new List<EventLog>();
}
