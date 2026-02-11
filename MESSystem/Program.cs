using Microsoft.EntityFrameworkCore;
using MESSystem.Data;
using MESSystem.Services;

// 마이그레이션 모드 체크
if (args.Length > 0 && args[0] == "migrate")
{
    DatabaseMigrator.RunMigration(args);
    return;
}

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

app.Run();

// DTO for API
public record EventDto(
    string EventType,
    string CardNumber,
    string? CollectorId,
    DateTime Timestamp,
    Dictionary<string, string>? Metadata
);
