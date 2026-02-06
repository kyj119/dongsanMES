namespace MESSystem.Services;

public static class ShippingMethodHelper
{
    /// <summary>
    /// 배송방법별 기본 우선순위 반환
    /// </summary>
    public static int GetDefaultPriority(string shippingMethod)
    {
        return shippingMethod switch
        {
            "퀵" => 1,
            "용차" => 1,
            "대신화물" => 2,
            "대신택배" => 2,
            "한진택배" => 3,
            "방문수령" => 4,
            "직접배송" => 4,
            _ => 2 // 기본값
        };
    }
    
    /// <summary>
    /// 배송방법별 기본 출고 시간 반환 (출고시간 입력이 필요 없는 경우)
    /// </summary>
    public static TimeSpan? GetDefaultShippingTime(string shippingMethod)
    {
        return shippingMethod switch
        {
            "대신화물" => new TimeSpan(16, 0, 0), // 16:00
            "대신택배" => new TimeSpan(16, 0, 0), // 16:00
            "한진택배" => new TimeSpan(18, 0, 0), // 18:00
            _ => null // 사용자가 직접 입력해야 함
        };
    }
    
    /// <summary>
    /// 출고시간 입력이 필요한 배송방법인지 확인
    /// </summary>
    public static bool RequiresShippingTime(string shippingMethod)
    {
        return shippingMethod switch
        {
            "퀵" => true,
            "용차" => true,
            "방문수령" => true,
            "직접배송" => true,
            _ => false
        };
    }
    
    /// <summary>
    /// 결제방법 입력이 필요한 배송방법인지 확인
    /// </summary>
    public static bool RequiresPaymentMethod(string shippingMethod)
    {
        return shippingMethod switch
        {
            "대신택배" => true,
            "대신화물" => true,
            "한진택배" => true,
            "퀵" => true,
            "용차" => true,
            _ => false
        };
    }
    
    /// <summary>
    /// 우선순위 레벨 텍스트 반환
    /// </summary>
    public static string GetPriorityText(int priority)
    {
        return priority switch
        {
            1 => "최우선",
            2 => "보통",
            3 => "낮음",
            4 => "매우 낮음",
            _ => "보통"
        };
    }
    
    /// <summary>
    /// 우선순위 배지 색상 반환
    /// </summary>
    public static string GetPriorityBadgeClass(int priority)
    {
        return priority switch
        {
            1 => "bg-danger",
            2 => "bg-primary",
            3 => "bg-secondary",
            4 => "bg-light text-dark",
            _ => "bg-primary"
        };
    }
}
