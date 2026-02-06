namespace MESSystem.Services;

public static class WorkProgressHelper
{
    /// <summary>
    /// 카드의 작업 진행률 계산
    /// </summary>
    public static double CalculateProgress(string status)
    {
        return status switch
        {
            "대기" => 0.0,
            "작업중" => 0.5,
            "완료" => 1.0,
            "보류" => 0.25,
            _ => 0.0
        };
    }
    
    /// <summary>
    /// 남은 시간 계산 (분 단위)
    /// </summary>
    public static double GetRemainingMinutes(DateTime shippingDate, TimeSpan? shippingTime)
    {
        var deadline = shippingDate.Date;
        if (shippingTime.HasValue)
        {
            deadline = deadline.Add(shippingTime.Value);
        }
        else
        {
            // 시간이 없으면 당일 23:59로 설정
            deadline = deadline.AddHours(23).AddMinutes(59);
        }
        
        return (deadline - DateTime.Now).TotalMinutes;
    }
    
    /// <summary>
    /// 예상 작업 시간 계산 (분 단위)
    /// 작업량 기반 (1개당 평균 10분 가정)
    /// </summary>
    public static double GetEstimatedWorkMinutes(int totalQuantity, double progressRate)
    {
        const double minutesPerItem = 10.0; // 1개당 평균 10분
        var remainingQuantity = totalQuantity * (1.0 - progressRate);
        return remainingQuantity * minutesPerItem;
    }
    
    /// <summary>
    /// 작업 상태 레벨 반환
    /// </summary>
    public static WorkStatusLevel GetWorkStatusLevel(double remainingMinutes, double estimatedWorkMinutes)
    {
        if (remainingMinutes < 0)
        {
            return WorkStatusLevel.Overdue; // 지연
        }
        else if (remainingMinutes < estimatedWorkMinutes)
        {
            return WorkStatusLevel.Critical; // 긴급 (예상 시간보다 적음)
        }
        else if (remainingMinutes < estimatedWorkMinutes * 1.5)
        {
            return WorkStatusLevel.Warning; // 주의 (여유 50% 미만)
        }
        else
        {
            return WorkStatusLevel.Normal; // 여유
        }
    }
    
    /// <summary>
    /// 작업 상태 텍스트 반환
    /// </summary>
    public static string GetStatusText(WorkStatusLevel level)
    {
        return level switch
        {
            WorkStatusLevel.Overdue => "⚠️ 지연",
            WorkStatusLevel.Critical => "🔴 긴급",
            WorkStatusLevel.Warning => "🟡 주의",
            WorkStatusLevel.Normal => "🟢 여유",
            _ => "🟢 여유"
        };
    }
    
    /// <summary>
    /// 작업 상태 배지 클래스 반환
    /// </summary>
    public static string GetStatusBadgeClass(WorkStatusLevel level)
    {
        return level switch
        {
            WorkStatusLevel.Overdue => "bg-dark text-white",
            WorkStatusLevel.Critical => "bg-danger text-white",
            WorkStatusLevel.Warning => "bg-warning text-dark",
            WorkStatusLevel.Normal => "bg-success text-white",
            _ => "bg-secondary text-white"
        };
    }
    
    /// <summary>
    /// 남은 시간 포맷팅
    /// </summary>
    public static string FormatRemainingTime(double minutes)
    {
        if (minutes < 0)
        {
            var overdue = TimeSpan.FromMinutes(-minutes);
            if (overdue.TotalDays >= 1)
                return $"{(int)overdue.TotalDays}일 {overdue.Hours}시간 지연";
            else if (overdue.TotalHours >= 1)
                return $"{(int)overdue.TotalHours}시간 {overdue.Minutes}분 지연";
            else
                return $"{(int)overdue.TotalMinutes}분 지연";
        }
        
        var remaining = TimeSpan.FromMinutes(minutes);
        if (remaining.TotalDays >= 1)
            return $"{(int)remaining.TotalDays}일 {remaining.Hours}시간 남음";
        else if (remaining.TotalHours >= 1)
            return $"{(int)remaining.TotalHours}시간 {remaining.Minutes}분 남음";
        else
            return $"{(int)remaining.TotalMinutes}분 남음";
    }
}

/// <summary>
/// 작업 상태 레벨
/// </summary>
public enum WorkStatusLevel
{
    Normal = 0,    // 여유
    Warning = 1,   // 주의
    Critical = 2,  // 긴급
    Overdue = 3    // 지연
}
