namespace MESCollector.Models;

public class EventDto
{
    public string EventType { get; set; } = string.Empty; // 작업대기, 작업시작, 작업완료
    public string CardNumber { get; set; } = string.Empty; // YYYYMMDD-XX-Y
    public string? CollectorId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public Dictionary<string, string>? Metadata { get; set; }
}

public class EventResponse
{
    public string Status { get; set; } = string.Empty;
    public int EventId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
