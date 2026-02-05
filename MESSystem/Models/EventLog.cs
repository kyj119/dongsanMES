using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MESSystem.Models;

public class EventLog
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(20)]
    public string EventType { get; set; } = string.Empty; // 작업대기/작업시작/작업완료/출력/스캔
    
    public int? CardId { get; set; }
    
    [Required]
    [StringLength(30)]
    public string CardNumber { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? CollectorId { get; set; }
    
    public DateTime Timestamp { get; set; }
    
    public string? RawJson { get; set; } // 원문 JSON 저장
    
    public bool IsProcessed { get; set; } = false;
    
    public DateTime? ProcessedAt { get; set; }
    
    [StringLength(500)]
    public string? ErrorMessage { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    // Navigation properties
    public Card? Card { get; set; }
    
    // 메타데이터 조회 헬퍼
    public string GetMetadata(string key)
    {
        if (string.IsNullOrEmpty(RawJson))
            return string.Empty;
            
        try
        {
            var json = JsonDocument.Parse(RawJson);
            if (json.RootElement.TryGetProperty("metadata", out var metadata))
            {
                if (metadata.TryGetProperty(key, out var value))
                {
                    return value.GetString() ?? string.Empty;
                }
            }
        }
        catch
        {
            // 파싱 실패 시 빈 문자열 반환
        }
        
        return string.Empty;
    }
}
