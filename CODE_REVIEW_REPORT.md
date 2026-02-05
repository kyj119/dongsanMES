# 📊 MES 시스템 코드 리뷰 보고서

**작성일**: 2026-02-05  
**리뷰 대상**: MES System (Manufacturing Execution System)  
**프로젝트 버전**: Day 1 완료 (기본 구조 및 인증)

---

## 📋 목차
1. [프로젝트 개요](#프로젝트-개요)
2. [아키텍처 분석](#아키텍처-분석)
3. [코드 품질 평가](#코드-품질-평가)
4. [보안 분석](#보안-분석)
5. [성능 고려사항](#성능-고려사항)
6. [개선 권장사항](#개선-권장사항)
7. [우수 사례](#우수-사례)
8. [다음 단계 제안](#다음-단계-제안)

---

## 1. 프로젝트 개요

### 1.1 프로젝트 정보
- **이름**: MES System (생산 현장 관리 시스템)
- **목적**: 사내 생산현장용 주문서 관리 및 카드 기반 작업 추적
- **기술 스택**: 
  - **백엔드**: ASP.NET Core 8.0 Razor Pages
  - **데이터베이스**: SQLite (개발) / SQL Server (프로덕션)
  - **인증**: 세션 기반 인증
  - **로깅**: Serilog (Collector)
  - **패턴**: MVC, Repository Pattern (부분)

### 1.2 프로젝트 통계
```
구성요소:
├── MESSystem (웹 애플리케이션)
│   ├── C# 파일: 36개
│   ├── Razor Pages: 26개
│   ├── 모델: 9개
│   └── 서비스: 2개
│
└── MESCollector (데이터 수집기)
    ├── C# 파일: 9개
    ├── 서비스: 3개
    └── 모델: 2개

총 코드 라인: ~2,000줄
문서: 5개 (50페이지)
```

### 1.3 핵심 기능
- ✅ **사용자 인증**: 관리자/현장 사용자 로그인
- ✅ **주문서 관리**: CRUD 및 논리 삭제
- ✅ **카드 시스템**: 작업 추적용 카드 생성 및 관리
- ✅ **이벤트 로깅**: Collector에서 실시간 이벤트 수집
- ✅ **거래처/품목 관리**: 마스터 데이터 관리
- ✅ **API 엔드포인트**: RESTful API (Collector 연동)

---

## 2. 아키텍처 분석

### 2.1 시스템 아키텍처

```
┌─────────────────────────────────────────────────────┐
│                   사용자 계층                         │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐          │
│  │ 관리자   │  │ 디자이너 │  │ 현장직원 │          │
│  │ (Admin)  │  │(Designer)│  │ (Field) │          │
│  └──────────┘  └──────────┘  └──────────┘          │
└─────────────────────────────────────────────────────┘
           │              │              │
           └──────────────┼──────────────┘
                          │
           ┌──────────────▼──────────────┐
           │   ASP.NET Core Web App      │
           │   (Razor Pages)             │
           │  ┌──────────────────────┐   │
           │  │ Session Auth         │   │
           │  │ - 로그인/로그아웃    │   │
           │  │ - 권한 분리          │   │
           │  └──────────────────────┘   │
           │  ┌──────────────────────┐   │
           │  │ Business Logic       │   │
           │  │ - Order Management   │   │
           │  │ - Card System        │   │
           │  │ - File Upload        │   │
           │  └──────────────────────┘   │
           │  ┌──────────────────────┐   │
           │  │ API Endpoints        │   │
           │  │ - POST /api/events   │   │
           │  │ - GET /api/clients   │   │
           │  │ - GET /api/products  │   │
           │  └──────────────────────┘   │
           └──────────────┬──────────────┘
                          │
           ┌──────────────▼──────────────┐
           │  Entity Framework Core      │
           │  (ORM)                       │
           └──────────────┬──────────────┘
                          │
           ┌──────────────▼──────────────┐
           │  SQLite / SQL Server        │
           │  (Database)                  │
           └──────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│              MES Collector (독립 프로세스)           │
│  ┌──────────────────────────────────────────────┐  │
│  │ FileSystemWatcher (3개)                      │  │
│  │  - Preview 폴더 (.bmp, .tsc)                 │  │
│  │  - PrintLog 폴더 (.log)                      │  │
│  │  - Job 폴더 (.job)                           │  │
│  └────────────────┬─────────────────────────────┘  │
│                   │                                 │
│  ┌────────────────▼─────────────────────────────┐  │
│  │ FileParserService                            │  │
│  │  - 카드번호 추출                             │  │
│  │  - LOG/JOB 파일 파싱                        │  │
│  └────────────────┬─────────────────────────────┘  │
│                   │                                 │
│  ┌────────────────▼─────────────────────────────┐  │
│  │ ApiService (HttpClient)                      │  │
│  │  - 이벤트 전송 (POST /api/events)           │  │
│  │  - 재시도 로직 (향후)                       │  │
│  └──────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────┘
```

### 2.2 데이터 모델 분석

#### 핵심 엔티티 (9개)

**1. User (사용자)**
```csharp
- Id, Username, Password, FullName, Role
- 역할: 관리자, 디자이너, 사용자
- 상태: IsActive
```

**2. Order (주문서)**
```csharp
- OrderNumber: YYYYMMDD-XX 형식
- Client 정보 (참조 + 스냅샷)
- ShippingMethod, PaymentMethod
- Status: 작성/진행중/완료/취소
- 논리 삭제: IsDeleted
- 재주문 관계: ParentOrderId
```

**3. OrderItem (주문 품목)**
```csharp
- Product 참조 + 스냅샷 (가격, 사양)
- Quantity, UnitPrice
```

**4. Card (작업 카드)**
```csharp
- CardNumber: YYYYMMDD-XX-Y 형식
- Status: 대기/작업중/완료
- IsModified: 주문서 수정 알림
- Category 참조 (태극기/현수막/간판)
```

**5. CardItem (카드 품목 스냅샷)**
```csharp
- OrderItem 참조
- 카드 생성 시점 스냅샷
```

**6. EventLog (이벤트 로그)**
```csharp
- EventType: 작업대기/작업시작/작업완료
- CardNumber, CollectorId
- RawJson: 원문 저장
- IsProcessed, ErrorMessage
```

**7. Product (품목 마스터)**
```csharp
- Code (고유 코드)
- Name, DefaultSpec
- Category 참조
```

**8. Category (분류)**
```csharp
- Name: 태극기, 현수막, 간판
- CardOrder: 카드 정렬 순서
```

**9. Client (거래처)**
```csharp
- Name, Address, Phone, Mobile
- 검색 최적화 (Index)
```

#### 관계도
```
Client ──1:N──> Order ──1:N──> OrderItem ──N:1──> Product ──N:1──> Category
                 │                                                      │
                 └──1:N──> Card ──1:N──> CardItem                      │
                            │                                           │
                            ├────────────────────────────────N:1────────┘
                            │
                            └──1:N──> EventLog
                            
Order ──재주문관계──> Order (ParentOrderId)
```

### 2.3 계층 구조

```
┌─────────────────────────────────────────┐
│        Presentation Layer               │
│  (Razor Pages - *.cshtml + *.cshtml.cs) │
│  - Account/ (로그인/로그아웃)           │
│  - Admin/ (관리자 화면)                 │
│  - Cards/ (현장 화면 - 예정)           │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│         Business Logic Layer            │
│  (PageModel Classes + Services)         │
│  - FileUploadService                    │
│  - OrderNumberService                   │
│  - [향후] CardService, OrderService     │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│          Data Access Layer              │
│  (Entity Framework Core)                │
│  - ApplicationDbContext                 │
│  - DbSet<T> for each entity             │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│           Database Layer                │
│  (SQLite / SQL Server)                  │
│  - 9 Tables with Indexes                │
│  - Foreign Key Constraints              │
└─────────────────────────────────────────┘
```

---

## 3. 코드 품질 평가

### 3.1 우수한 점 ✅

#### 1. **명확한 프로젝트 구조**
```csharp
MESSystem/
├── Data/                    // 데이터 컨텍스트
├── Models/                  // 엔티티 모델
├── Pages/                   // Razor Pages (UI + 로직)
│   ├── Account/            // 인증
│   └── Admin/              // 관리 기능
├── Services/               // 비즈니스 로직
└── wwwroot/                // 정적 파일
```
**평가**: 표준 ASP.NET Core 구조를 잘 따르고 있음.

#### 2. **Entity Framework 최적화**
```csharp
// ApplicationDbContext.cs - 인덱스 최적화
entity.HasIndex(e => e.CardNumber).IsUnique();
entity.HasIndex(e => e.Status);
entity.HasIndex(e => e.Timestamp).IsDescending();
```
**평가**: 검색 성능을 고려한 인덱스 설계.

#### 3. **논리 삭제 구현**
```csharp
public bool IsDeleted { get; set; } = false;
public DateTime? DeletedAt { get; set; }
public string? DeletedBy { get; set; }
public string? DeleteReason { get; set; }
```
**평가**: 데이터 보존 및 감사 추적 가능.

#### 4. **스냅샷 패턴**
```csharp
// OrderItem: 주문 시점의 품목 정보 복사
public string ProductName { get; set; }
public decimal UnitPrice { get; set; }
```
**평가**: 과거 데이터 무결성 보장.

#### 5. **낙관적 락**
```csharp
public int Version { get; set; } = 1;  // 동시성 제어
```
**평가**: 다중 사용자 환경 고려.

#### 6. **Collector 설계**
```csharp
// FileSystemWatcher를 사용한 실시간 모니터링
// - Preview → 작업대기
// - PrintLog → 작업시작
// - Job → 작업완료
```
**평가**: 이벤트 기반 아키텍처로 확장성 우수.

#### 7. **API 설계**
```csharp
app.MapPost("/api/events", async (EventDto eventDto, ...) => 
{
    // 원문 보존 + 파싱 + 상태 업데이트
    var eventLog = new EventLog
    {
        RawJson = JsonSerializer.Serialize(eventDto)  // 원본 보존
    };
    // ...
});
```
**평가**: 데이터 손실 방지를 위한 원문 저장.

### 3.2 개선이 필요한 부분 ⚠️

#### 1. **비밀번호 보안 (중요!)**
```csharp
// ❌ 현재: 평문 저장
Password = "admin123"

// ✅ 권장: BCrypt 해싱
Password = BCrypt.HashPassword("admin123")
```
**위험도**: 🔴 높음  
**영향**: 사용자 계정 탈취 가능  
**해결책**:
```csharp
// NuGet: BCrypt.Net-Next
using BCrypt.Net;

// 저장 시
user.Password = BCrypt.HashPassword(plainPassword);

// 검증 시
bool isValid = BCrypt.Verify(plainPassword, user.Password);
```

#### 2. **CSRF 보호 미흡**
```csharp
// ❌ 현재: POST 폼에 CSRF 토큰 없음
<form method="post">
    <!-- ... -->
</form>

// ✅ 권장: AntiForgeryToken 추가
<form method="post">
    @Html.AntiForgeryToken()
    <!-- ... -->
</form>
```
**위험도**: 🟡 중간  
**해결책**: ASP.NET Core의 내장 CSRF 보호 활성화

#### 3. **권한 체크 중복**
```csharp
// ❌ 매 PageModel마다 반복
if (HttpContext.Session.GetString("Role") != "관리자")
{
    return RedirectToPage("/Account/Login");
}

// ✅ 권장: 커스텀 Authorize Attribute
[AdminOnly]
public class OrdersModel : PageModel { }
```
**위험도**: 🟢 낮음  
**영향**: 코드 중복, 유지보수 어려움

#### 4. **에러 핸들링 부족**
```csharp
// ❌ try-catch 없음
var client = await _context.Clients.FindAsync(id);
client.Name = form.Name;  // NullReferenceException 가능

// ✅ 권장: 방어 코드
var client = await _context.Clients.FindAsync(id);
if (client == null)
{
    return NotFound();
}
```

#### 5. **검색 쿼리 최적화 필요**
```csharp
// ⚠️ 현재: Contains 검색 (인덱스 미활용)
.Where(c => c.Name.Contains(query))

// ✅ 권장: Full-Text Search 또는 StartsWith
.Where(c => c.Name.StartsWith(query))  // 인덱스 활용
```

#### 6. **데이터베이스 마이그레이션 미사용**
```csharp
// ❌ 현재: EnsureCreated() 사용
context.Database.EnsureCreated();

// ✅ 권장: Migrations 사용
// dotnet ef migrations add InitialCreate
// dotnet ef database update
```
**문제**: 프로덕션 환경에서 스키마 변경 시 데이터 손실 위험

#### 7. **Collector 재시도 로직 없음**
```csharp
// ❌ 현재: 전송 실패 시 로그만 출력
catch (HttpRequestException ex)
{
    _logger.LogError(ex, "API 호출 실패");
}

// ✅ 권장: 재전송 큐 구현
// - 실패한 이벤트를 로컬 큐에 저장
// - 주기적으로 재전송 시도
```

---

## 4. 보안 분석

### 4.1 보안 취약점

| 항목 | 위험도 | 현재 상태 | 권장 조치 |
|------|--------|----------|----------|
| **비밀번호 저장** | 🔴 높음 | 평문 저장 | BCrypt 해싱 적용 |
| **CSRF 보호** | 🟡 중간 | 미적용 | AntiForgeryToken 추가 |
| **SQL Injection** | 🟢 낮음 | EF Core 사용 (안전) | 유지 |
| **인증 타임아웃** | 🟢 낮음 | 2시간 설정 | 적절함 |
| **HTTPS** | 🟡 중간 | HTTP 리디렉션 있음 | 인증서 설치 필요 |
| **API 인증** | 🟡 중간 | 없음 | API Key 또는 JWT 추가 |
| **파일 업로드** | 🟡 중간 | 검증 부족 | 파일 타입/크기 체크 |
| **로그 노출** | 🟢 낮음 | 민감 정보 없음 | 유지 |

### 4.2 보안 강화 체크리스트

#### 즉시 수정 필요 (Week 1)
- [ ] BCrypt 비밀번호 해싱 구현
- [ ] CSRF 토큰 추가
- [ ] 파일 업로드 검증 강화

#### 단기 개선 (Week 2-3)
- [ ] API 인증 (API Key)
- [ ] HTTPS 인증서 적용
- [ ] 권한 체크 통일 (Attribute)

#### 장기 개선 (Month 1)
- [ ] 2FA (이중 인증) 고려
- [ ] 감사 로그 강화
- [ ] 정기 보안 감사

---

## 5. 성능 고려사항

### 5.1 데이터베이스 성능

#### 좋은 점 ✅
```csharp
// 1. 인덱스 최적화
entity.HasIndex(e => e.CardNumber).IsUnique();
entity.HasIndex(e => e.Status);
entity.HasIndex(e => e.CreatedAt);

// 2. Eager Loading 사용
.Include(c => c.Order)
.Include(c => c.Category)

// 3. 페이징 준비 (Select-Take 패턴)
.Take(10)
.Take(20)
```

#### 개선 가능 ⚠️
```csharp
// ❌ N+1 쿼리 가능성
foreach (var order in orders)
{
    var items = order.OrderItems.ToList();  // 각 루프마다 쿼리
}

// ✅ 권장: Include 사용
var orders = await _context.Orders
    .Include(o => o.OrderItems)
    .ThenInclude(oi => oi.Product)
    .ToListAsync();
```

### 5.2 확장성

#### 현재 용량 추정
```
예상 데이터량 (연간):
- 주문서: ~5,000건
- 카드: ~20,000건
- 이벤트 로그: ~60,000건
총: ~85,000 레코드

예상 DB 크기: ~100MB (1년)
→ SQLite로 충분히 처리 가능
```

#### 병목 지점
1. **파일 업로드**: 공유 폴더 네트워크 속도 의존
2. **검색**: Full-Text Search 부재
3. **동시 접속**: 세션 메모리 사용

---

## 6. 개선 권장사항

### 6.1 우선순위별 개선 계획

#### 🔴 긴급 (Day 2-3)
1. **비밀번호 해싱**
   ```bash
   dotnet add package BCrypt.Net-Next
   ```
   
2. **CSRF 보호**
   ```csharp
   builder.Services.AddAntiforgery();
   ```

3. **에러 페이지**
   ```csharp
   app.UseStatusCodePagesWithReExecute("/Error/{0}");
   ```

#### 🟡 중요 (Week 2)
1. **권한 체크 통일**
   ```csharp
   public class AdminOnlyAttribute : Attribute, IPageFilter { }
   ```

2. **API 인증**
   ```csharp
   app.MapPost("/api/events", [ApiKeyAuth] async (...) => { });
   ```

3. **Collector 재시도 로직**
   ```csharp
   // 로컬 SQLite 큐 + 재전송 스케줄러
   ```

#### 🟢 개선 (Week 3-4)
1. **로깅 강화**
   ```csharp
   builder.Services.AddSerilog();
   ```

2. **캐싱 추가**
   ```csharp
   builder.Services.AddMemoryCache();
   ```

3. **Swagger API 문서**
   ```csharp
   builder.Services.AddEndpointsApiExplorer();
   ```

### 6.2 리팩토링 제안

#### 1. Service Layer 분리
```csharp
// 현재: PageModel에 비즈니스 로직
// 권장: Service 클래스로 분리

public interface IOrderService
{
    Task<Order> CreateOrderAsync(OrderDto dto);
    Task<bool> CancelOrderAsync(int id, string reason);
}

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    // ...
}
```

#### 2. DTO 사용
```csharp
// 현재: Entity 직접 노출
// 권장: DTO 변환

public record OrderDto(
    string ClientName,
    string ShippingMethod,
    List<OrderItemDto> Items
);

public record OrderItemDto(
    int ProductId,
    int Quantity
);
```

#### 3. Validator 추가
```csharp
// FluentValidation 사용
public class OrderValidator : AbstractValidator<OrderDto>
{
    public OrderValidator()
    {
        RuleFor(x => x.ClientName).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
    }
}
```

---

## 7. 우수 사례

### 7.1 베스트 프랙티스

#### 1. **파일명 자동 관리 전략**
```
원본: "태극기_디자인.ai"
   ↓ 서버 자동 리네이밍
결과: "20260204-01-1_태극기.ai"
   ↓ 공유폴더 저장
경로: \\192.168.0.122\Designs\2026\02\20260204-01\

장점:
✅ 카드번호 자동 추출 가능
✅ 파일 충돌 방지
✅ 시간순 정렬 용이
```

#### 2. **이벤트 소싱 패턴**
```csharp
// EventLog에 원문 저장
RawJson = JsonSerializer.Serialize(eventDto)

장점:
✅ 데이터 손실 방지
✅ 디버깅 용이
✅ 추후 재처리 가능
```

#### 3. **재주문 관계 설계**
```csharp
public int? ParentOrderId { get; set; }
public string? OrderType { get; set; }  // 신규/재주문/재출력

장점:
✅ 주문 이력 추적
✅ 재주문 분석 가능
✅ 고객 패턴 파악
```

#### 4. **분류별 카드 정렬**
```csharp
public int CardOrder { get; set; }  // 태극기(1), 현수막(2), 간판(3)

장점:
✅ 현장 작업 순서 명확
✅ 카테고리별 우선순위
```

### 7.2 코드 스타일

#### 일관성 ✅
- C# 네이밍 컨벤션 준수
- Nullable Reference Types 활성화
- Async/Await 패턴 일관적 사용

#### 가독성 ✅
- 명확한 변수명 (cardNumber, collectorId)
- 주석 적절히 사용
- 로직 분리 (Parser, Monitor, API)

---

## 8. 다음 단계 제안

### 8.1 Day 2-3 작업 우선순위

#### 핵심 기능 (필수)
1. **주문서 생성 완성**
   - [ ] 품목 검색 자동완성
   - [ ] 파일 업로드
   - [ ] 카드 자동 생성

2. **Collector 고도화**
   - [ ] 재전송 큐 구현
   - [ ] 로컬 DB 캐시
   - [ ] 헬스체크 모니터링

3. **현장 화면**
   - [ ] 카드 목록 (태극기/현수막/간판)
   - [ ] 터치 친화적 UI
   - [ ] 새로고침 자동화

#### 보안 강화 (긴급)
1. **비밀번호 해싱**
   ```csharp
   dotnet add package BCrypt.Net-Next --version 4.0.3
   ```

2. **CSRF 보호**
   ```cshtml
   <form method="post">
       @Html.AntiForgeryToken()
   </form>
   ```

3. **권한 체크 개선**
   ```csharp
   [AdminOnly]
   public class CategoriesModel : PageModel { }
   ```

### 8.2 Week 2 목표

#### 관리 기능
- [ ] 대시보드 (통계, 차트)
- [ ] 이벤트 로그 조회
- [ ] 사용자 관리
- [ ] 백업 기능

#### 최적화
- [ ] 검색 성능 개선 (Full-Text)
- [ ] 캐싱 적용
- [ ] 로깅 체계화

#### 문서화
- [ ] API 문서 (Swagger)
- [ ] 사용자 매뉴얼
- [ ] 운영 가이드

---

## 9. 종합 평가

### 9.1 점수표

| 항목 | 점수 | 평가 |
|------|------|------|
| **아키텍처** | 8/10 | 계층 분리 우수, Service Layer 추가 필요 |
| **코드 품질** | 7/10 | 일관성 좋음, 에러 핸들링 보완 필요 |
| **보안** | 5/10 | ⚠️ 비밀번호 해싱 필수, CSRF 추가 |
| **성능** | 8/10 | 인덱스 최적화 우수, 캐싱 고려 |
| **확장성** | 7/10 | 현재 요구사항 충족, Service 분리 권장 |
| **문서화** | 9/10 | 배포 가이드 매우 상세함 |
| **테스트** | 3/10 | ⚠️ 단위 테스트 없음 |

**종합 점수**: 7.0/10 (Good)

### 9.2 강점
✅ 명확한 비즈니스 로직  
✅ 체계적인 데이터 모델  
✅ 우수한 문서화  
✅ 실무 중심 설계  
✅ 확장 가능한 구조  

### 9.3 개선 필요
⚠️ 보안 강화 (비밀번호, CSRF)  
⚠️ 에러 핸들링  
⚠️ 테스트 코드  
⚠️ Service Layer 분리  
⚠️ API 인증  

---

## 10. 결론 및 권장사항

### 10.1 즉시 조치사항 (Day 2)
1. ✅ **비밀번호 해싱 적용** (BCrypt)
2. ✅ **CSRF 토큰 추가**
3. ✅ **파일 업로드 검증**

### 10.2 단기 개선 (Week 2)
1. Service Layer 분리
2. API 인증 추가
3. 에러 핸들링 강화
4. Collector 재시도 로직

### 10.3 중기 개선 (Week 3-4)
1. 단위 테스트 작성
2. 캐싱 적용
3. 로깅 체계화
4. 성능 모니터링

### 10.4 최종 평가

**이 프로젝트는 Day 1 목표를 성공적으로 달성했습니다.**

✅ **우수한 점**:
- 실무 요구사항을 정확히 반영한 설계
- 체계적인 데이터 모델
- 확장 가능한 아키텍처
- 상세한 배포 문서

⚠️ **주의사항**:
- 보안 강화가 최우선 과제
- 프로덕션 배포 전 반드시 비밀번호 해싱 적용
- API 인증 추가 권장

**프로덕션 배포 준비도**: 60% → 보안 강화 후 80% 가능

---

**리뷰어**: Claude AI Developer  
**작성일**: 2026-02-05  
**다음 리뷰**: Day 3 완료 후
