using Microsoft.EntityFrameworkCore;
using MESSystem.Data;
using MESSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add custom services
builder.Services.AddScoped<FileUploadService>();
builder.Services.AddScoped<OrderNumberService>();
builder.Services.AddScoped<ThumbnailService>();

var app = builder.Build();

// 데이터베이스 초기화 및 시드 데이터
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    
    // 데이터베이스 생성
    context.Database.EnsureCreated();
    
    // 기본 사용자가 없으면 추가
    if (!context.Users.Any())
    {
        context.Users.AddRange(
            new MESSystem.Models.User
            {
                Username = "admin",
                Password = "admin123",
                FullName = "관리자",
                Role = "관리자",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new MESSystem.Models.User
            {
                Username = "designer",
                Password = "designer123",
                FullName = "디자이너",
                Role = "디자이너",
                IsActive = true,
                CreatedAt = DateTime.Now
            }
        );
        context.SaveChanges();
    }
    
    // 기본 분류가 없으면 추가
    if (!context.Categories.Any())
    {
        context.Categories.AddRange(
            new MESSystem.Models.Category { Name = "태극기", CardOrder = 1, IsActive = true, CreatedAt = DateTime.Now },
            new MESSystem.Models.Category { Name = "현수막", CardOrder = 2, IsActive = true, CreatedAt = DateTime.Now },
            new MESSystem.Models.Category { Name = "간판", CardOrder = 3, IsActive = true, CreatedAt = DateTime.Now }
        );
        context.SaveChanges();
    }
    
    // 테스트용 거래처 데이터
    if (!context.Clients.Any())
    {
        context.Clients.AddRange(
            new MESSystem.Models.Client
            {
                Name = "서울시청",
                Address = "서울시 중구 세종대로 110",
                Phone = "02-2133-7000",
                Mobile = "010-1234-5678",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new MESSystem.Models.Client
            {
                Name = "부산광역시청",
                Address = "부산시 연제구 중앙대로 1001",
                Phone = "051-888-1234",
                Mobile = "010-2345-6789",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new MESSystem.Models.Client
            {
                Name = "대한상공회의소",
                Address = "서울시 중구 세종대로 39",
                Phone = "02-316-3114",
                Mobile = "010-3456-7890",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new MESSystem.Models.Client
            {
                Name = "국립중앙박물관",
                Address = "서울시 용산구 서빙고로 137",
                Phone = "02-2077-9000",
                Mobile = "010-4567-8901",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new MESSystem.Models.Client
            {
                Name = "한국관광공사",
                Address = "서울시 중구 청계천로 40",
                Phone = "02-729-9600",
                Mobile = "010-5678-9012",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new MESSystem.Models.Client
            {
                Name = "롯데월드타워",
                Address = "서울시 송파구 올림픽로 300",
                Phone = "02-3213-5000",
                Mobile = "010-6789-0123",
                IsActive = true,
                CreatedAt = DateTime.Now
            }
        );
        context.SaveChanges();
    }
    
    // 테스트용 품목 데이터
    if (!context.Products.Any())
    {
        var categories = context.Categories.ToList();
        var category1 = categories.First(c => c.Name == "태극기");
        var category2 = categories.First(c => c.Name == "현수막");
        var category3 = categories.First(c => c.Name == "간판");
        
        context.Products.AddRange(
            // 태극기
            new MESSystem.Models.Product
            {
                Code = "TG-001",
                Name = "태극기 소형",
                CategoryId = category1.Id,
                DefaultSpec = "100x150cm",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new MESSystem.Models.Product
            {
                Code = "TG-002",
                Name = "태극기 중형",
                CategoryId = category1.Id,
                DefaultSpec = "150x225cm",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new MESSystem.Models.Product
            {
                Code = "TG-003",
                Name = "태극기 대형",
                CategoryId = category1.Id,
                DefaultSpec = "200x300cm",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            // 현수막
            new MESSystem.Models.Product
            {
                Code = "HS-001",
                Name = "현수막 일반",
                CategoryId = category2.Id,
                DefaultSpec = "90x120cm",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new MESSystem.Models.Product
            {
                Code = "HS-002",
                Name = "현수막 대형",
                CategoryId = category2.Id,
                DefaultSpec = "180x240cm",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new MESSystem.Models.Product
            {
                Code = "HS-003",
                Name = "현수막 특대",
                CategoryId = category2.Id,
                DefaultSpec = "300x600cm",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            // 간판
            new MESSystem.Models.Product
            {
                Code = "GP-001",
                Name = "아크릴 간판",
                CategoryId = category3.Id,
                DefaultSpec = "50x100cm",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new MESSystem.Models.Product
            {
                Code = "GP-002",
                Name = "LED 간판",
                CategoryId = category3.Id,
                DefaultSpec = "100x200cm",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new MESSystem.Models.Product
            {
                Code = "GP-003",
                Name = "입간판",
                CategoryId = category3.Id,
                DefaultSpec = "60x180cm",
                IsActive = true,
                CreatedAt = DateTime.Now
            }
        );
        context.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Session 사용

app.UseAuthorization();

app.MapRazorPages();

// API Endpoints (Collector용)
app.MapPost("/api/events", async (EventDto eventDto, ApplicationDbContext db, ILogger<Program> logger) =>
{
    try
    {
        // 원문 로그 저장
        var eventLog = new MESSystem.Models.EventLog
        {
            EventType = eventDto.EventType,
            CardNumber = eventDto.CardNumber,
            CollectorId = eventDto.CollectorId,
            Timestamp = eventDto.Timestamp,
            RawJson = System.Text.Json.JsonSerializer.Serialize(eventDto),
            CreatedAt = DateTime.Now
        };
        
        db.EventLogs.Add(eventLog);
        
        // 카드 조회 및 매핑
        var card = await db.Cards
            .Include(c => c.Order)
            .FirstOrDefaultAsync(c => c.CardNumber == eventDto.CardNumber && !c.Order.IsDeleted);
        
        if (card == null)
        {
            eventLog.IsProcessed = false;
            eventLog.ErrorMessage = "카드를 찾을 수 없습니다.";
            await db.SaveChangesAsync();
            
            return Results.BadRequest(new { error = "CARD_NOT_FOUND", message = "카드 번호를 찾을 수 없습니다." });
        }
        
        eventLog.CardId = card.Id;
        
        // 카드 상태 업데이트
        switch (eventDto.EventType)
        {
            case "작업대기":
                // 상태 유지
                break;
            case "작업시작":
                card.Status = "작업중";
                break;
            case "작업완료":
                card.Status = "완료";
                break;
        }
        
        eventLog.IsProcessed = true;
        eventLog.ProcessedAt = DateTime.Now;
        
        await db.SaveChangesAsync();
        
        logger.LogInformation($"이벤트 처리 완료: {eventDto.EventType} - {eventDto.CardNumber}");
        
        return Results.Ok(new
        {
            status = "success",
            event_id = eventLog.Id,
            message = "이벤트가 기록되었습니다."
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "이벤트 처리 중 오류 발생");
        return Results.Problem("서버 오류가 발생했습니다.");
    }
});

// 거래처 검색 API
app.MapGet("/api/clients/search", async (string? q, ApplicationDbContext db) =>
{
    if (string.IsNullOrEmpty(q))
    {
        return Results.Ok(new List<object>());
    }

    var clients = await db.Clients
        .Where(c => c.IsActive && c.Name.Contains(q))
        .Take(10)
        .Select(c => new
        {
            c.Id,
            c.Name,
            c.Address,
            c.Phone,
            c.Mobile
        })
        .ToListAsync();

    return Results.Ok(clients);
});

// 품목 검색 API
app.MapGet("/api/products/search", async (string? q, ApplicationDbContext db) =>
{
    if (string.IsNullOrEmpty(q))
    {
        return Results.Ok(new List<object>());
    }

    var products = await db.Products
        .Include(p => p.Category)
        .Where(p => p.IsActive && (p.Name.Contains(q) || p.Code.Contains(q)))
        .Take(20)
        .Select(p => new
        {
            p.Id,
            p.Code,
            p.Name,
            p.DefaultSpec,
            CategoryName = p.Category.Name
        })
        .ToListAsync();

    return Results.Ok(products);
});

// 썸네일 이미지 제공 API
app.MapGet("/api/thumbnail", async (string? path) =>
{
    if (string.IsNullOrEmpty(path))
    {
        return Results.BadRequest("경로가 제공되지 않았습니다.");
    }

    try
    {
        if (!File.Exists(path))
        {
            // 에러 이미지 반환
            var errorImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "error-thumbnail.jpg");
            if (File.Exists(errorImagePath))
            {
                var errorBytes = await File.ReadAllBytesAsync(errorImagePath);
                return Results.File(errorBytes, "image/jpeg");
            }
            return Results.NotFound("파일을 찾을 수 없습니다.");
        }

        var bytes = await File.ReadAllBytesAsync(path);
        var extension = Path.GetExtension(path).ToLower();
        var contentType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            _ => "application/octet-stream"
        };

        return Results.File(bytes, contentType);
    }
    catch (Exception ex)
    {
        return Results.Problem($"이미지를 불러올 수 없습니다: {ex.Message}");
    }
});

app.Run();

// DTO for API
public record EventDto(
    string EventType,
    string CardNumber,
    string? CollectorId,
    DateTime Timestamp,
    Dictionary<string, string>? Metadata
);
