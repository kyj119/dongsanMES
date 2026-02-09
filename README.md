# MES 시스템 (생산 현장 관리 시스템)

[![GitHub](https://img.shields.io/badge/GitHub-dongsanMES-blue)](https://github.com/kyj119/dongsanMES)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Private-red)](https://github.com/kyj119/dongsanMES)

## 프로젝트 개요
- **이름**: MES System (Manufacturing Execution System)
- **목적**: 사내 생산현장용 주문서 관리 및 카드 기반 작업 추적 시스템
- **기술 스택**: ASP.NET Core 8.0 Razor Pages + SQLite (개발) / SQL Server (운영)
- **현재 상태**: ✅ **Day 2 완료** - Collector 개발 및 설치 가이드 완성
- **GitHub**: https://github.com/kyj119/dongsanMES

## 📊 프로젝트 진행 현황

### ✅ Day 1: 기본 구조 및 인증 시스템
- [x] ASP.NET Core 8.0 Razor Pages 프로젝트 생성
- [x] 데이터베이스 모델 8개 완성 (User, Category, Product, Client, Order, OrderItem, Card, CardItem, EventLog)
- [x] Entity Framework Core + SQLite 연동
- [x] 로그인/로그아웃 + 세션 인증
- [x] 관리자/현장 권한 분리
- [x] Collector API 엔드포인트 (`POST /api/events`)
- [x] 빌드 성공 및 문서화

### ✅ Day 2: Collector 완성 및 문서화
- [x] **Collector 상세 구현 가이드** (COLLECTOR_GUIDE.md - 24KB)
- [x] **Collector 빠른 이해** (COLLECTOR_SUMMARY.md - 8.8KB)
- [x] **출력기 설치 가이드** (COLLECTOR_INSTALL_GUIDE.md - 11KB, QUICK_INSTALL_COLLECTOR.md - 6.4KB)
- [x] **파일 형식 상세 설명** (TOPAZ_RIP_FILE_FORMAT.md - 24KB)
- [x] **Windows 자동 설치 스크립트** (install_collector.bat)
- [x] **웹 기반 실시간 대시보드** (collector_dashboard.html)
- [x] **터미널 시뮬레이션 데모** (demo_collector.sh)
- [x] **보안 체크리스트** (SECURITY_CHECKLIST.md - 14KB)
- [x] **전체 코드 리뷰 보고서** (CODE_REVIEW_REPORT.md - 24KB)
- [x] MESSystem 기능 개선 (Client 관리, Order 확장)

### 🚀 Day 3 예정: 현장 UI 개발
- [ ] 현장용 카드 목록 화면 (분류별 필터링)
- [ ] 카드 상세 화면 (품목 정보 + 실시간 상태)
- [ ] 카드 인쇄 기능 (라벨 프린터 연동)
- [ ] 터치 친화적 UI 구현

## 📚 주요 문서

| 문서명 | 용도 | 크기 | 대상 |
|--------|------|------|------|
| **QUICK_INSTALL_COLLECTOR.md** | ⭐ 출력기 설치 (10분) | 6.4KB | 현장 담당자 |
| **COLLECTOR_SUMMARY.md** | Collector 빠른 이해 | 8.8KB | 개발자 |
| **COLLECTOR_GUIDE.md** | Collector 상세 구현 | 24KB | 개발자 |
| **TOPAZ_RIP_FILE_FORMAT.md** | 파일 형식 설명 | 24KB | 개발자 |
| **CODE_REVIEW_REPORT.md** | 전체 코드 리뷰 | 24KB | 관리자 |
| **SECURITY_CHECKLIST.md** | 보안 체크리스트 | 14KB | 보안 담당자 |

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

## 🎯 핵심 개념 요약

### MES 시스템이란?
**파일명 = 카드번호** 연결 방식으로 **TOPAZ RIP 출력 작업을 실시간 추적**하는 시스템입니다.

### 3단계 작업 흐름
```
1️⃣ 작업대기 (Preview)
   출력기에서 출력 대기 시 → .bmp.tsc 파일 생성
   → Collector가 감지 → MES 서버에 "작업대기" 이벤트 전송

2️⃣ 작업시작 (PrintLog)
   출력 시작 시 → .log 파일 생성 (수량, 시작시간 포함)
   → Collector가 파싱 → "작업시작" 이벤트 전송

3️⃣ 작업완료 (Job)
   출력 완료 시 → .job 파일 생성 (사이즈, 완료시간 포함)
   → Collector가 파싱 → "작업완료" 이벤트 전송
```

### Collector 동작 예시
```bash
# 출력기 PC: C:\TOPAZ_RIP\
preview/20260205-01-1.bmp.tsc      # 파일명에서 카드번호 추출
printlog/20260205-01-1_025927.log  # JobName, Copies 파싱
job/20260205-01-10002.job          # PrintFile, DestSize 파싱

# 각 단계마다 HTTP POST → http://localhost:5000/api/events
```

## 🔧 Collector 테스트 방법

### 1️⃣ 웹 대시보드 (추천)
**실시간 모니터링 + 시뮬레이터 내장**
```bash
# 대시보드 실행
cd /home/user/webapp
pm2 start server.js --name dashboard

# 브라우저에서 접속
http://localhost:3000/collector_dashboard.html
```

**기능:**
- 📊 실시간 통계 (대기/시작/완료/총 이벤트)
- 🔄 3단계 작업 흐름 애니메이션
- 🎮 이벤트 시뮬레이터 (버튼 클릭으로 테스트)
- 📝 최근 이벤트 로그 (카드번호, 시간, 메타데이터)

### 2️⃣ 터미널 데모
**Step-by-step 시각화 + 테스트 파일 생성**
```bash
cd /home/user/webapp
chmod +x demo_collector.sh
./demo_collector.sh

# 생성된 테스트 파일 확인
ls -la /tmp/TOPAZ_RIP_TEST/preview/   # *.bmp.tsc
cat /tmp/TOPAZ_RIP_TEST/printlog/*.log # 출력 로그
cat /tmp/TOPAZ_RIP_TEST/job/*.job      # 작업 완료 파일
```

### 3️⃣ 출력기 PC 실제 설치
**Windows 자동 설치 스크립트 제공**
```bash
# 1. install_collector.bat를 출력기 PC로 복사
# 2. 관리자 권한으로 실행 (우클릭 → 관리자 권한으로 실행)
# 3. 가이드 문서 참고: QUICK_INSTALL_COLLECTOR.md
```

**설치 완료 후:**
- `C:\MESCollector\` 폴더 생성
- `C:\TOPAZ_RIP\` 모니터링 준비
- appsettings.json 수정 필요 (ServerUrl, CollectorId)

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

## 🏗️ 프로젝트 구조

```
webapp/
├── MESSystem/                    # ASP.NET Core 웹 애플리케이션
│   ├── Data/
│   │   └── ApplicationDbContext.cs
│   ├── Models/                   # 데이터 모델 (8개)
│   │   ├── User.cs
│   │   ├── Category.cs
│   │   ├── Product.cs
│   │   ├── Client.cs
│   │   ├── Order.cs
│   │   ├── OrderItem.cs
│   │   ├── Card.cs
│   │   ├── CardItem.cs
│   │   └── EventLog.cs
│   ├── Pages/                    # Razor Pages (26개)
│   │   ├── Account/
│   │   │   ├── Login.cshtml
│   │   │   └── Logout.cshtml
│   │   └── Admin/
│   │       ├── Categories/       # 분류 관리 (CRUD)
│   │       ├── Clients/          # 거래처 관리 (CRUD)
│   │       ├── Products/         # 품목 관리 (CRUD)
│   │       └── Orders/           # 주문서 관리 (CRUD)
│   ├── Services/
│   │   ├── FileUploadService.cs
│   │   └── OrderNumberService.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── database_schema.sql
│
├── MESCollector/                 # Windows Service (데이터 수집기)
│   ├── Models/
│   │   ├── CollectorSettings.cs
│   │   └── EventDto.cs
│   ├── Services/                 # 핵심 로직 (3개)
│   │   ├── ApiService.cs         # HTTP 통신 + 재시도
│   │   ├── FileMonitorService.cs # FileSystemWatcher
│   │   └── FileParserService.cs  # 파일 파싱 (정규식)
│   ├── Program.cs
│   └── appsettings.json
│
├── 📚 Documentation/
│   ├── README.md                 # 프로젝트 개요 (이 문서)
│   ├── COLLECTOR_GUIDE.md        # Collector 상세 가이드 (24KB)
│   ├── COLLECTOR_SUMMARY.md      # Collector 빠른 이해 (8.8KB)
│   ├── COLLECTOR_INSTALL_GUIDE.md # 출력기 설치 가이드 (11KB)
│   ├── QUICK_INSTALL_COLLECTOR.md # 빠른 설치 (6.4KB)
│   ├── TOPAZ_RIP_FILE_FORMAT.md  # 파일 형식 설명 (24KB)
│   ├── CODE_REVIEW_REPORT.md     # 코드 리뷰 (24KB)
│   ├── SECURITY_CHECKLIST.md     # 보안 체크리스트 (14KB)
│   ├── DEPLOYMENT_GUIDE.md       # 배포 가이드
│   └── SERVER_TRANSFER_GUIDE.md  # 서버 이전 가이드
│
├── 🛠️ Tools/
│   ├── install_collector.bat     # Windows 자동 설치 스크립트
│   ├── demo_collector.sh         # 터미널 시뮬레이션 데모
│   ├── collector_dashboard.html  # 웹 기반 실시간 대시보드
│   └── server.js                 # Node.js 간단 HTTP 서버
│
└── .git/                         # Git 버전 관리
```

### 📊 코드 통계
- **MESSystem**: C# 파일 36개, Razor Pages 26개, Models 9개
- **MESCollector**: C# 파일 9개, Services 3개
- **총 코드 라인**: ~2,000줄
- **문서**: 12개 (총 150KB)

## 🚀 빠른 시작

### 개발 환경 요구사항
- **운영체제**: Windows 10+ (서버 배포용)
- **.NET SDK**: 8.0 이상
- **데이터베이스**: SQLite (개발) / SQL Server 2019+ (운영)
- **IDE**: Visual Studio 2022 또는 VS Code

### 1️⃣ 프로젝트 클론
```bash
git clone https://github.com/kyj119/dongsanMES.git
cd dongsanMES
```

### 2️⃣ MESSystem 실행 (웹 서버)
```bash
cd MESSystem
dotnet restore
dotnet build
dotnet run

# 브라우저에서 접속
# http://localhost:5000

# 로그인
# 관리자: admin / admin123
# 현장: field01 / user123
```

### 3️⃣ Collector 테스트 (데모)
```bash
# 방법 1: 웹 대시보드 (추천)
cd ..
node server.js
# http://localhost:3000/collector_dashboard.html

# 방법 2: 터미널 시뮬레이션
chmod +x demo_collector.sh
./demo_collector.sh
```

### 4️⃣ 출력기 PC 설치 (실전)
1. `install_collector.bat`를 출력기 PC로 복사
2. 관리자 권한으로 실행
3. `QUICK_INSTALL_COLLECTOR.md` 가이드 참고
4. `appsettings.json` 수정:
   - ServerUrl: MES 서버 주소
   - CollectorId: 출력기 식별자

## API 엔드포인트

### POST /api/events
Collector에서 이벤트 전송용

## 📡 API 엔드포인트

### POST /api/events
**Collector → MES 서버 이벤트 전송**

**요청 예시:**
```json
{
  "eventType": "작업시작",
  "cardNumber": "20260205-01-1",
  "collectorId": "COLLECTOR-001",
  "timestamp": "2026-02-05T09:30:00Z",
  "metadata": {
    "JobName": "태극기_90x135",
    "Copies": "100",
    "StartTime": "2026-02-05 09:30:00",
    "PrinterName": "MIMAKI-JV300"
  }
}
```

**성공 응답:**
```json
{
  "status": "success",
  "eventId": 123,
  "message": "이벤트가 기록되었습니다."
}
```

### GET /api/clients/search?name={검색어}
**거래처 검색 API**

### GET /api/products/search?query={검색어}
**품목 검색 API**

## 🗄️ 데이터베이스 구조

### 테이블 목록
1. **Users** - 사용자 (관리자/현장)
2. **Categories** - 분류 (태극기/현수막/간판)
3. **Products** - 품목 마스터
4. **Clients** - 거래처 마스터
5. **Orders** - 주문서 헤더
6. **OrderItems** - 주문서 품목 라인
7. **Cards** - 카드 컨테이너
8. **CardItems** - 카드 품목 스냅샷
9. **EventLogs** - Collector 이벤트 로그

### 초기 데이터
- **분류**: 태극기(1), 현수막(2), 간판(3)
- **사용자**:
  - admin / admin123 (관리자)
  - field01 / user123 (현장 사용자)

## 🔒 보안 체크리스트

자세한 내용은 **SECURITY_CHECKLIST.md** 참고

### 긴급 (High Priority)
- [ ] 비밀번호 해시 구현 (BCrypt.Net)
- [ ] SQL Injection 방어 (EF Core 파라미터화)
- [ ] XSS 방어 (Razor 자동 인코딩)
- [ ] CSRF 토큰 추가

### 중요 (Medium Priority)
- [ ] 권한 체크 기반 클래스
- [ ] API 인증 (JWT 또는 API Key)
- [ ] Collector 통신 암호화 (HTTPS)

## 📦 배포 가이드

자세한 내용은 **DEPLOYMENT_GUIDE.md**, **SERVER_TRANSFER_GUIDE.md** 참고

### 배포 환경
- **서버**: Windows Server 2019+ (사내 네트워크)
- **웹 서버**: IIS 10+
- **데이터베이스**: SQL Server 2019+
- **런타임**: .NET 8.0 Hosting Bundle

### 배포 준비물
1. MESSystem 빌드 결과물
2. MESCollector 실행 파일
3. database_schema.sql
4. 설정 파일 (appsettings.json)
5. IIS 설정 가이드

## 🤝 기여 가이드

이 프로젝트는 사내 프로젝트입니다. 기여는 팀원만 가능합니다.

## 📞 문의

- **프로젝트 관리자**: kyj119
- **GitHub Issues**: https://github.com/kyj119/dongsanMES/issues

## 📝 변경 이력

### [v0.2.1] - 2026-02-09 (Bug Fix)
**수정:**
- Razor 구문 오류 수정 (빌드 오류 해결)
  - Cards/Index_New.cshtml: .Count 개 패턴 수정
  - Cards/Index.cshtml: @cardItems.Count 개, @item.OrderItem.Quantity개 수정
  - Admin/Orders/Detail.cshtml: 3개 .Count 개 패턴 수정
  - Admin/Orders/Print.cshtml: .Count 개 패턴 수정
  - Cards/Detail.cshtml: 2개 .Count 개 패턴 수정
- Razor 파서가 속성명과 한글 단위를 올바르게 구분하도록 괄호 사용

### [v0.2] - 2026-02-05 (Day 2)
**추가:**
- Collector 전체 문서 작성 (5개 문서)
- 웹 기반 실시간 대시보드
- 터미널 시뮬레이션 데모
- Windows 자동 설치 스크립트
- 보안 체크리스트
- 전체 코드 리뷰 보고서
- Client(거래처) 관리 CRUD

**개선:**
- Order 모델 거래처 연동
- 주문서 작성 폼 개선

### [v0.1] - 2026-02-04 (Day 1)
**추가:**
- 프로젝트 초기 구조
- 데이터베이스 모델 8개
- 로그인/로그아웃 인증
- Collector API 엔드포인트
- 배포 가이드 문서

---

## ⭐ 프로젝트 특징

1. **🎯 실시간 작업 추적**: 파일명 = 카드번호 자동 매칭
2. **🔄 3단계 상태 관리**: 대기 → 시작 → 완료
3. **📊 시각화 도구**: 웹 대시보드 + 터미널 데모
4. **📚 완벽한 문서화**: 12개 문서 (150KB)
5. **🛠️ 자동화 도구**: Windows 설치 스크립트
6. **🔒 보안 체크리스트**: 14KB 상세 가이드

---

**GitHub Repository**: https://github.com/kyj119/dongsanMES

**마지막 업데이트**: 2026-02-09  
**현재 진행도**: ✅ v0.2.1 완료 (빌드 오류 수정)
