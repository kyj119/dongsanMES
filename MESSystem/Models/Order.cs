using System.ComponentModel.DataAnnotations;

namespace MESSystem.Models;

public class Order
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(20)]
    public string OrderNumber { get; set; } = string.Empty; // YYYYMMDD-XX
    
    [Required]
    [StringLength(100)]
    public string ClientName { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? ClientAddress { get; set; }
    
    [StringLength(20)]
    public string? ClientPhone { get; set; }
    
    [StringLength(20)]
    public string? ClientMobile { get; set; }
    
    [Required]
    [StringLength(50)]
    public string ShippingMethod { get; set; } = string.Empty; // 대신택배/대신화물/한진택배/퀵/용차/방문수령/직접배송
    
    [StringLength(20)]
    public string? PaymentMethod { get; set; } // 착불/선불
    
    public DateTime ShippingDate { get; set; }
    
    public TimeSpan? ShippingTime { get; set; }
    
    [StringLength(500)]
    public string? FilePath { get; set; } // 공유폴더 경로
    
    public int Version { get; set; } = 1; // 낙관적 락
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    [StringLength(50)]
    public string? CreatedBy { get; set; }
    
    // 논리 삭제
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    
    [StringLength(50)]
    public string? DeletedBy { get; set; }
    
    [StringLength(200)]
    public string? DeleteReason { get; set; }
    
    // 재주문 관계
    public int? ParentOrderId { get; set; }
    
    [StringLength(20)]
    public string? OrderType { get; set; } // 신규/재주문/재출력/부분취소
    
    [StringLength(200)]
    public string? ParentReason { get; set; }
    
    // Navigation properties
    public Order? ParentOrder { get; set; }
    public ICollection<Order> ChildOrders { get; set; } = new List<Order>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Card> Cards { get; set; } = new List<Card>();
}
