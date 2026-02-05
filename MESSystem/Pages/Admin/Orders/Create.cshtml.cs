using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using MESSystem.Data;
using MESSystem.Models;
using MESSystem.Services;

namespace MESSystem.Pages.Admin.Orders
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly FileUploadService _fileUploadService;
        private readonly OrderNumberService _orderNumberService;

        public CreateModel(
            ApplicationDbContext context,
            FileUploadService fileUploadService,
            OrderNumberService orderNumberService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
            _orderNumberService = orderNumberService;
        }

        public List<Product> Products { get; set; } = new();

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
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
            public DateTime ShippingDate { get; set; } = DateTime.Today;

            public TimeSpan? ShippingTime { get; set; }

            // 품목 정보
            public List<OrderItemInput> Items { get; set; } = new();
        }

        public class OrderItemInput
        {
            public int? ProductId { get; set; }
            public string? Spec { get; set; }
            public string? Description { get; set; }
            public int? Quantity { get; set; }
            public decimal? UnitPrice { get; set; }
            public IFormFile? DesignFile { get; set; }
            public string? Remark { get; set; }
        }

        public async Task OnGetAsync()
        {
            // 활성 품목 목록 조회 (분류 포함)
            Products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && !p.IsDeleted)
                .OrderBy(p => p.Category.CardOrder)
                .ThenBy(p => p.Code)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // 품목 목록 재로드
            Products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && !p.IsDeleted)
                .OrderBy(p => p.Category.CardOrder)
                .ThenBy(p => p.Code)
                .ToListAsync();

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

            try
            {
                // 1. 주문번호 생성
                var orderNumber = _orderNumberService.GenerateOrderNumber();

                // 2. 주문서 생성
                var order = new Order
                {
                    OrderNumber = orderNumber,
                    ClientName = Input.ClientName,
                    ClientPhone = Input.ClientPhone,
                    ClientMobile = Input.ClientMobile,
                    ClientAddress = Input.ClientAddress,
                    ShippingMethod = Input.ShippingMethod,
                    PaymentMethod = Input.PaymentMethod,
                    ShippingDate = Input.ShippingDate,
                    ShippingTime = Input.ShippingTime,
                    Status = "작성",
                    CreatedAt = DateTime.Now,
                    CreatedBy = User.Identity?.Name ?? "System"
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // Order.Id 생성

                // 3. 품목 라인 저장 (빈 줄 제외)
                var lineNumber = 1;
                var itemsByCategory = new Dictionary<int, List<OrderItem>>();

                foreach (var itemInput in Input.Items)
                {
                    if (!itemInput.ProductId.HasValue || !itemInput.Quantity.HasValue || itemInput.Quantity <= 0)
                        continue; // 빈 줄 건너뛰기

                    var product = await _context.Products
                        .Include(p => p.Category)
                        .FirstOrDefaultAsync(p => p.Id == itemInput.ProductId.Value);

                    if (product == null)
                        continue;

                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = product.Id,
                        Spec = itemInput.Spec,
                        Description = itemInput.Description,
                        Quantity = itemInput.Quantity.Value,
                        UnitPrice = itemInput.UnitPrice ?? 0,
                        Remark = itemInput.Remark,
                        LineNumber = lineNumber++
                    };

                    _context.OrderItems.Add(orderItem);
                    
                    // 분류별로 그룹화
                    if (!itemsByCategory.ContainsKey(product.CategoryId))
                    {
                        itemsByCategory[product.CategoryId] = new List<OrderItem>();
                    }
                    itemsByCategory[product.CategoryId].Add(orderItem);
                }

                await _context.SaveChangesAsync(); // OrderItem.Id 생성

                // 4. 카드 생성 (분류별)
                var categories = await _context.Categories
                    .Where(c => itemsByCategory.Keys.Contains(c.Id))
                    .OrderBy(c => c.CardOrder)
                    .ToListAsync();

                foreach (var category in categories)
                {
                    var cardNumber = _orderNumberService.GenerateCardNumber(orderNumber, category.CardOrder);

                    var card = new Card
                    {
                        OrderId = order.Id,
                        CategoryId = category.Id,
                        CardNumber = cardNumber,
                        Status = "대기",
                        IsModified = false,
                        CreatedAt = DateTime.Now
                    };

                    _context.Cards.Add(card);
                    await _context.SaveChangesAsync(); // Card.Id 생성

                    // 5. 카드 품목 스냅샷 저장
                    foreach (var orderItem in itemsByCategory[category.Id])
                    {
                        var cardItem = new CardItem
                        {
                            CardId = card.Id,
                            OrderItemId = orderItem.Id,
                            ProductCode = orderItem.Product?.Code ?? "",
                            ProductName = orderItem.Product?.Name ?? "",
                            Spec = orderItem.Spec,
                            Description = orderItem.Description,
                            Quantity = orderItem.Quantity,
                            UnitPrice = orderItem.UnitPrice,
                            Remark = orderItem.Remark,
                            LineNumber = orderItem.LineNumber
                        };

                        _context.CardItems.Add(cardItem);
                    }

                    await _context.SaveChangesAsync();
                }

                // 6. 디자인 파일 업로드 및 저장
                await UploadDesignFilesAsync(order, itemsByCategory);

                TempData["Message"] = $"주문서 '{orderNumber}'가 생성되었습니다. {categories.Count}개의 카드가 생성되었습니다.";
                return RedirectToPage("Detail", new { id = order.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"저장 중 오류가 발생했습니다: {ex.Message}");
                return Page();
            }
        }

        private string? ValidateInput()
        {
            // 품목 검증: 최소 1개 이상
            var validItems = Input.Items.Count(i => i.ProductId.HasValue && i.Quantity.HasValue && i.Quantity > 0);
            if (validItems == 0)
            {
                return "최소 1개 이상의 품목을 입력해주세요.";
            }

            // 최대 20개 검증
            if (validItems > 20)
            {
                return "품목은 최대 20개까지만 입력 가능합니다.";
            }

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

        private async Task UploadDesignFilesAsync(Order order, Dictionary<int, List<OrderItem>> itemsByCategory)
        {
            var categories = await _context.Categories
                .Where(c => itemsByCategory.Keys.Contains(c.Id))
                .OrderBy(c => c.CardOrder)
                .ToListAsync();

            var itemIndex = 0;
            foreach (var category in categories)
            {
                var cardNumber = _orderNumberService.GenerateCardNumber(order.OrderNumber, category.CardOrder);

                foreach (var orderItem in itemsByCategory[category.Id])
                {
                    var fileInput = Input.Items[itemIndex].DesignFile;
                    if (fileInput != null && fileInput.Length > 0)
                    {
                        try
                        {
                            var filePath = await _fileUploadService.UploadFileAsync(
                                fileInput, 
                                order.OrderNumber, 
                                cardNumber);

                            orderItem.DesignFileName = Path.GetFileName(filePath);
                            orderItem.FilePath = filePath;
                        }
                        catch (Exception ex)
                        {
                            // 로그 기록 (일단 무시)
                            Console.WriteLine($"파일 업로드 실패: {ex.Message}");
                        }
                    }
                    itemIndex++;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
