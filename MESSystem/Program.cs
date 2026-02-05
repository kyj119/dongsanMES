using Microsoft.EntityFrameworkCore;
using MESSystem.Data;
using MESSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

app.Run();

// DTO for API
public record EventDto(
    string EventType,
    string CardNumber,
    string? CollectorId,
    DateTime Timestamp,
    Dictionary<string, string>? Metadata
);
