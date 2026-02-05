using Microsoft.EntityFrameworkCore;
using MESSystem.Data;

namespace MESSystem.Services;

public class OrderNumberService
{
    private readonly ApplicationDbContext _context;
    private static readonly object _lock = new object();

    public OrderNumberService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 새로운 주문번호 생성 (동시성 안전)
    /// 형식: YYYYMMDD-XX (예: 20260204-01, 20260204-02)
    /// </summary>
    public string GenerateOrderNumber()
    {
        lock (_lock)
        {
            var today = DateTime.Now.Date;
            var datePrefix = today.ToString("yyyyMMdd"); // 20260204

            // 오늘 날짜의 마지막 주문번호 조회
            var lastOrder = _context.Orders
                .Where(o => o.OrderNumber.StartsWith(datePrefix))
                .OrderByDescending(o => o.OrderNumber)
                .FirstOrDefault();

            int sequence = 1;
            if (lastOrder != null)
            {
                // 주문번호에서 순번 추출 (예: 20260204-05 → 5)
                var lastSequence = lastOrder.OrderNumber.Substring(datePrefix.Length + 1);
                if (int.TryParse(lastSequence, out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }

            // 새 주문번호 생성 (예: 20260204-01)
            var orderNumber = $"{datePrefix}-{sequence:D2}";
            return orderNumber;
        }
    }

    /// <summary>
    /// 카드번호 생성
    /// 형식: {주문번호}-{분류순서} (예: 20260204-01-1, 20260204-01-2)
    /// </summary>
    public string GenerateCardNumber(string orderNumber, int cardOrder)
    {
        return $"{orderNumber}-{cardOrder}";
    }

    /// <summary>
    /// 주문번호 유효성 검사
    /// </summary>
    public bool IsValidOrderNumber(string orderNumber)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            return false;

        // 형식: YYYYMMDD-XX (11자리)
        if (orderNumber.Length != 11)
            return false;

        var parts = orderNumber.Split('-');
        if (parts.Length != 2)
            return false;

        // 날짜 부분 (8자리)
        if (parts[0].Length != 8 || !int.TryParse(parts[0], out _))
            return false;

        // 순번 부분 (2자리)
        if (parts[1].Length != 2 || !int.TryParse(parts[1], out _))
            return false;

        return true;
    }

    /// <summary>
    /// 카드번호 유효성 검사
    /// </summary>
    public bool IsValidCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
            return false;

        // 형식: YYYYMMDD-XX-Y (최소 13자리)
        var parts = cardNumber.Split('-');
        if (parts.Length != 3)
            return false;

        // 주문번호 부분 검증
        var orderNumber = $"{parts[0]}-{parts[1]}";
        if (!IsValidOrderNumber(orderNumber))
            return false;

        // 카드순서 부분 (1자리 이상)
        if (!int.TryParse(parts[2], out _))
            return false;

        return true;
    }
}
