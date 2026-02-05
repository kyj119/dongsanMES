using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using MESSystem.Data;
using MESSystem.Models;

namespace MESSystem.Pages.Admin.Orders
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Order Order { get; set; } = null!;

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            public int OrderId { get; set; }

            // 거래처 정보
            [Required(ErrorMessage = "거래처명은 필수입니다.")]
            [StringLength(200)]
            public string ClientName { get; set; } = string.Empty;

            [StringLength(50)]
            public string? ClientPhone { get; set; }

            [StringLength(50)]
            public string? ClientMobile { get; set; }

            [StringLength(500)]
            public string? ClientAddress { get; set; }

            // 출고 정보
            [Required(ErrorMessage = "출고방법은 필수입니다.")]
            public string ShippingMethod { get; set; } = string.Empty;

            public string? PaymentMethod { get; set; }

            [Required(ErrorMessage = "출고일은 필수입니다.")]
            public DateTime ShippingDate { get; set; }

            public TimeSpan? ShippingTime { get; set; }

            // 낙관적 락
            public int Version { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.IsDeleted)
            {
                TempData["Error"] = "삭제된 주문서는 수정할 수 없습니다.";
                return RedirectToPage("Detail", new { id });
            }

            Order = order;

            Input = new InputModel
            {
                OrderId = order.Id,
                ClientName = order.ClientName,
                ClientPhone = order.ClientPhone,
                ClientMobile = order.ClientMobile,
                ClientAddress = order.ClientAddress,
                ShippingMethod = order.ShippingMethod,
                PaymentMethod = order.PaymentMethod,
                ShippingDate = order.ShippingDate,
                ShippingTime = order.ShippingTime,
                Version = order.Version
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // 주문서 재로드
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Cards)
                    .ThenInclude(c => c.CardItems)
                .FirstOrDefaultAsync(o => o.Id == Input.OrderId);

            if (order == null)
            {
                return NotFound();
            }

            Order = order;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 비즈니스 검증
            var validationError = ValidateInput();
            if (!string.IsNullOrEmpty(validationError))
            {
                ModelState.AddModelError(string.Empty, validationError);
                return Page();
            }

            // 낙관적 락 검증
            if (order.Version != Input.Version)
            {
                ModelState.AddModelError(string.Empty, "다른 사용자가 먼저 수정했습니다. 페이지를 새로고침해주세요.");
                return Page();
            }

            try
            {
                // 1. 주문서 업데이트
                order.ClientName = Input.ClientName;
                order.ClientPhone = Input.ClientPhone;
                order.ClientMobile = Input.ClientMobile;
                order.ClientAddress = Input.ClientAddress;
                order.ShippingMethod = Input.ShippingMethod;
                order.PaymentMethod = Input.PaymentMethod;
                order.ShippingDate = Input.ShippingDate;
                order.ShippingTime = Input.ShippingTime;
                order.UpdatedAt = DateTime.Now;
                order.Version++; // 버전 증가

                // 2. 연결된 모든 카드에 '수정됨' 플래그 설정
                foreach (var card in order.Cards)
                {
                    card.IsModified = true;
                    
                    // 카드 품목 스냅샷도 업데이트 (최신 정보 반영)
                    foreach (var cardItem in card.CardItems)
                    {
                        var orderItem = order.OrderItems.FirstOrDefault(oi => oi.Id == cardItem.OrderItemId);
                        if (orderItem != null)
                        {
                            // 주문서 품목 정보를 카드 스냅샷에 동기화
                            // (현재 품목 자체는 수정 안 함, 거래처/출고 정보만 수정)
                        }
                    }
                }

                await _context.SaveChangesAsync();

                TempData["Message"] = $"주문서 '{order.OrderNumber}'가 수정되었습니다. {order.Cards.Count}개의 카드에 알림이 표시됩니다.";
                return RedirectToPage("Detail", new { id = order.Id });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await OrderExists(Input.OrderId))
                {
                    return NotFound();
                }
                
                ModelState.AddModelError(string.Empty, "동시성 오류가 발생했습니다. 다시 시도해주세요.");
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"수정 중 오류가 발생했습니다: {ex.Message}");
                return Page();
            }
        }

        private string? ValidateInput()
        {
            // 출고방법별 필수 항목 검증
            var needsPayment = new[] { "대신택배", "대신화물", "한진택배", "퀵", "용차" };
            if (needsPayment.Contains(Input.ShippingMethod) && string.IsNullOrEmpty(Input.PaymentMethod))
            {
                return "선택한 출고방법은 결제방법이 필수입니다.";
            }

            var needsTime = new[] { "퀵", "용차", "방문수령", "직접배송" };
            if (needsTime.Contains(Input.ShippingMethod) && !Input.ShippingTime.HasValue)
            {
                return "선택한 출고방법은 출고시간이 필수입니다.";
            }

            return null;
        }

        private async Task<bool> OrderExists(int id)
        {
            return await _context.Orders.AnyAsync(o => o.Id == id);
        }
    }
}
