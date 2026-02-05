# MES 시스템 (생산 현장 관리 시스템)

## 프로젝트 개요
- **이름**: MES System
- **목적**: 사내 생산현장용 주문서 관리 및 카드 기반 작업 추적 시스템
- **기술 스택**: ASP.NET Core 8.0 Razor Pages + SQL Server
- **현재 상태**: 🚧 Day 1 진행 중 (기본 구조 완료)

## 완료된 작업 ✅

### Day 1: 기본 구조 생성
- [x] .NET 8.0 SDK 설치
- [x] ASP.NET Core Razor Pages 프로젝트 생성
- [x] 데이터베이스 모델 생성 (User, Category, Product, Order, OrderItem, Card, CardItem, EventLog)
- [x] Entity Framework Core DbContext 설정
- [x] 데이터베이스 스키마 SQL 스크립트 생성 (`database_schema.sql`)
- [x] 로그인/로그아웃 기능 구현
- [x] 세션 기반 인증 구현
- [x] 관리자/사용자 권한 분리
- [x] 기본 레이아웃 및 메뉴 구조
- [x] Collector용 API 엔드포인트 (`POST /api/events`)

## 다음 작업 예정 (Day 2-3)

### Collector 개발
- [ ] Windows Service 프로젝트 생성
- [ ] TOPAZ_RIP 폴더 모니터링 (preview/printlog/job)
- [ ] 파일 파싱 로직 (LOG, JOB)
- [ ] 카드번호 검증
- [ ] HTTP API 연동
- [ ] 재전송 큐 구현

## 데이터베이스 구조

### 테이블 목록
1. **Users** - 사용자 (관리자/현장)
2. **Categories** - 분류 (태극기/현수막/간판)
3. **Products** - 품목 마스터
4. **Orders** - 주문서 헤더
5. **OrderItems** - 주문서 품목 라인
6. **Cards** - 카드 컨테이너
7. **CardItems** - 카드 품목 스냅샷
8. **EventLogs** - Collector 이벤트 로그

### 초기 데이터
- **분류**: 태극기(1), 현수막(2), 간판(3)
- **사용자**:
  - admin / admin123 (관리자)
  - field01 / user123 (현장 사용자)

## 주요 기능 설계

### 파일명 자동 관리 전략
```
1. 관리자가 주문서 작성 시 디자인 파일 업로드
   원본: "태극기_디자인.ai"
   
2. 서버가 자동으로 파일명 변경
   → "20260204-01-1_태극기.ai"
   
3. 공유폴더에 저장
   경로: \\192.168.0.122\Designs\2026\02\20260204-01\
   
4. TOPAZ_RIP 작업명: "20260204-01-1"
   파일명에서 카드번호 자동 추출
```

### Collector 동작 방식
```
C:\TOPAZ_RIP\preview\    → *.bmp.tsc (출력 대기)
C:\TOPAZ_RIP\printlog\   → *_시각.log (출력 시작)
C:\TOPAZ_RIP\job\        → *0002.job (출력 완료)
```

## 출고방법별 입력 필드 규칙

| 출고방법 | 착불/선불 | 출고시간 |
|---------|---------|---------|
| 대신택배 | ✅ 필수 | ❌ 없음 |
| 대신화물 | ✅ 필수 | ❌ 없음 |
| 한진택배 | ✅ 필수 | ❌ 없음 |
| 퀵 | ✅ 필수 | ✅ 필수 |
| 용차 | ✅ 필수 | ✅ 필수 |
| 방문수령 | ❌ 없음 | ✅ 필수 |
| 직접배송 | ❌ 없음 | ✅ 필수 |

## 프로젝트 구조
```
MESSystem/
├── Data/
│   └── ApplicationDbContext.cs
├── Models/
│   ├── User.cs
│   ├── Category.cs
│   ├── Product.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   ├── Card.cs
│   ├── CardItem.cs
│   └── EventLog.cs
├── Pages/
│   ├── Account/
│   │   ├── Login.cshtml
│   │   └── Logout.cshtml
│   ├── Admin/
│   │   ├── Orders/ (예정)
│   │   ├── Products/ (예정)
│   │   └── Categories/ (예정)
│   └── Cards/ (예정)
├── database_schema.sql
├── Program.cs
└── appsettings.json
```

## 설정 파일

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MESSystem;..."
  },
  "SharedFolderPath": "\\\\192.168.0.122\\Designs\\",
  "UploadPath": "wwwroot/uploads/"
}
```

## 데이터베이스 설치 방법

### SQL Server에서 실행:
```sql
-- 1. SQL Server Management Studio 실행
-- 2. database_schema.sql 파일 열기
-- 3. 실행 (F5)
```

## 로컬 개발 환경 실행

```bash
# 프로젝트 빌드
cd /home/user/webapp/MESSystem
export PATH="/home/user/.dotnet:$PATH"
dotnet build

# 실행 (향후)
dotnet run
```

## API 엔드포인트

### POST /api/events
Collector에서 이벤트 전송용

**요청:**
```json
{
  "eventType": "작업시작",
  "cardNumber": "20260204-01-1",
  "collectorId": "MACHINE-01",
  "timestamp": "2026-02-04T09:30:00Z",
  "metadata": {
    "file_type": "printlog",
    "copies": "100"
  }
}
```

**응답:**
```json
{
  "status": "success",
  "event_id": 1,
  "message": "이벤트가 기록되었습니다."
}
```

## 보안 고려사항
- [ ] TODO: 비밀번호 해시 구현 (BCrypt)
- [ ] TODO: CSRF 토큰 추가
- [ ] TODO: 권한 체크 PageModel 기반 클래스

## 배포 계획
- **서버**: Windows Server (사내 네트워크)
- **DB**: SQL Server 2019+
- **IIS**: ASP.NET Core 호스팅

## 개발 일정

### Week 1
- ✅ Day 1: 기본 구조 + 인증
- 🚧 Day 2-3: Collector 개발
- Day 4-5: 관리자 화면
- Day 6-7: 현장 화면

### Week 2
- Day 8: 재주문 기능
- Day 9: Collector 고도화
- Day 10-11: 관리 기능
- Day 12: 통합 테스트
- Day 13: 배포 준비
- Day 14: 교육 + 운영 시작

## 참고 사항
- 공유폴더 경로: `\\192.168.0.122\Designs\`
- 품목 라인 제한: 최대 20개
- 삭제 정책: 논리 삭제 (IsDeleted)
- 카드번호 형식: `YYYYMMDD-XX-Y` (예: 20260204-01-1)

---

**마지막 업데이트**: 2026-02-05
**현재 진행도**: Day 1 완료 (기본 구조)
