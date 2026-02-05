using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using MESSystem.Data;
using MESSystem.Models;
using MESSystem.Services;

namespace MESSystem.Pages.Admin.Orders
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly FileUploadService _fileUploadService;

        public EditModel(ApplicationDbContext context, FileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        public Order Order { get; set; } = null!;

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            public int OrderId { get; set; }

            // 품목 정보
            public List<OrderItemInput> Items { get; set; } = new();

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

        public class OrderItemInput
        {
            public int? OrderItemId { get; set; }  // 기존 품목 수정 시
            public int ProductId { get; set; }
            public string? Spec { get; set; }
            public string? Description { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public IFormFile? DesignFile { get; set; }
            public string? Remark { get; set; }
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
                        .ThenInclude(p => p.Category)
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
                Version = order.Version,
                Items = order.OrderItems.OrderBy(i => i.LineNumber).Select(item => new OrderItemInput
                {
                    OrderItemId = item.Id,
                    ProductId = item.ProductId,
                    Spec = item.Spec,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice ?? 0,
                    Remark = item.Remark
                }).ToList()
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

            // 품목 최소 1개 검증
            if (Input.Items == null || Input.Items.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "최소 1개 이상의 품목이 필요합니다.");
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
                // 1. 주문서 정보 업데이트
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

                // 2. 품목 정보 업데이트
                var submittedItemIds = Input.Items
                    .Where(i => i.OrderItemId.HasValue)
                    .Select(i => i.OrderItemId!.Value)
                    .ToHashSet();

                // 2-1. 삭제된 품목 제거 (체크박스 해제)
                var itemsToDelete = order.OrderItems
                    .Where(item => !submittedItemIds.Contains(item.Id))
                    .ToList();

                foreach (var item in itemsToDelete)
                {
                    _context.OrderItems.Remove(item);
                    
                    // 연결된 카드 아이템도 제거
                    foreach (var card in order.Cards)
                    {
                        var cardItemsToRemove = card.CardItems
                            .Where(ci => ci.OrderItemId == item.Id)
                            .ToList();
                        
                        foreach (var ci in cardItemsToRemove)
                        {
                            _context.Remove(ci);
                        }
                    }
                }

                // 2-2. 기존 품목 업데이트
                for (int i = 0; i < Input.Items.Count; i++)
                {
                    var inputItem = Input.Items[i];
                    
                    if (inputItem.OrderItemId.HasValue)
                    {
                        var orderItem = order.OrderItems.FirstOrDefault(oi => oi.Id == inputItem.OrderItemId.Value);
                        if (orderItem != null)
                        {
                            // 기존 품목 정보 업데이트
                            orderItem.Spec = inputItem.Spec;
                            orderItem.Description = inputItem.Description;
                            orderItem.Quantity = inputItem.Quantity;
                            orderItem.UnitPrice = inputItem.UnitPrice;
                            orderItem.Remark = inputItem.Remark;
                            orderItem.LineNumber = i + 1;

                            // 디자인 파일 교체
                            if (inputItem.DesignFile != null && inputItem.DesignFile.Length > 0)
                            {
                                var cardNumber = order.Cards.FirstOrDefault(c => 
                                    c.CardItems.Any(ci => ci.OrderItemId == orderItem.Id))?.CardNumber;
                                
                                if (!string.IsNullOrEmpty(cardNumber))
                                {
                                    var filePath = await _fileUploadService.UploadFileAsync(
                                        inputItem.DesignFile,
                                        order.OrderNumber,
                                        cardNumber);
                                    
                                    orderItem.DesignFileName = Path.GetFileName(filePath);
                                    orderItem.FilePath = filePath;
                                }
                            }
                            
                            // 연결된 카드 아이템 스냅샷도 업데이트
                            foreach (var card in order.Cards)
                            {
                                var cardItem = card.CardItems.FirstOrDefault(ci => ci.OrderItemId == orderItem.Id);
                                if (cardItem != null)
                                {
                                    cardItem.ProductName = orderItem.Product.Name;
                                    cardItem.ProductCode = orderItem.Product.Code;
                                    cardItem.Spec = orderItem.Spec;
                                    cardItem.Quantity = orderItem.Quantity;
                                    cardItem.UnitPrice = orderItem.UnitPrice;
                                }
                            }
                        }
                    }
                }

                // 3. 연결된 모든 카드에 '수정됨' 플래그 설정
                foreach (var card in order.Cards)
                {
                    card.IsModified = true;
                    card.ModifiedAt = DateTime.Now;
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
