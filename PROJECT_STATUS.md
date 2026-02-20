# 프로젝트 상태 요약 (PROJECT_STATUS.md)

**마지막 업데이트**: 2026-02-11
**버전**: v0.3.0-erp-dev
**현재 브랜치**: genspark_ai_developer

---

## 📊 프로젝트 개요

- **이름**: DongsanMES (동산 MES + ERP 통합 시스템)
- **타입**: ASP.NET Core 8.0 Razor Pages + SQLite
- **목적**: 생산 현장 관리 (MES) + 회계/매출 관리 (ERP)
- **GitHub**: https://github.com/kyj119/dongsanMES
- **PR**: https://github.com/kyj119/dongsanMES/pull/1

---

## ✅ 완료된 작업

### Phase 1: MES 기본 시스템 (완료)
- ✅ 데이터베이스 모델 9개 (User, Category, Product, Client, Order, OrderItem, Card, CardItem, EventLog)
- ✅ 로그인/로그아웃 인증 시스템
- ✅ 주문서 관리 (CRUD)
- ✅ 품목/거래처 관리
- ✅ 카드 기반 작업 추적
- ✅ Collector API 엔드포인트

### Phase 2: ERP 매출 관리 데이터 모델 (완료)
- ✅ 6개 신규 테이블 추가
  - SalesClosing (매출 마감)
  - SalesClosingItem (매출 마감 항목)
  - TaxInvoice (세금계산서)
  - TaxInvoiceItem (세금계산서 품목)
  - Payment (입금)
  - BankTransaction (통장 거래)
- ✅ 기존 모델 확장
  - Client: BusinessNumber, CeoName, BusinessType, BusinessItem 추가
  - Order: IsSalesClosed, SalesClosingItemId 추가

### Phase 3: 사용자 관리 시스템 (완료)
- ✅ 사용자 목록/검색/필터
- ✅ 사용자 생성 (유효성 검사)
- ✅ 사용자 수정 (권한/상태 변경)
- ✅ 사용자 삭제 (안전 체크)
- ✅ Admin 권한 체크

### Phase 4: 데이터베이스 마이그레이션 (완료)
- ✅ migrate_erp.sql 작성
- ✅ seed_data.sql 작성 (더미 데이터)
- ✅ DatabaseMigrator.cs (C# 자동화)
- ✅ 마이그레이션 가이드 문서
- ✅ DB Browser로 마이그레이션 실행 완료 ✅

---

## 🗄️ 데이터베이스 스키마

### 파일명
- **실제 파일**: `MESSystem.db`
- **연결 문자열**: `Data Source=MESSystem.db` (appsettings.json)

### 테이블 목록 (총 14개)
1. Users (5명 - admin, field01, manager01, field02, accounting01)
2. Categories (3개 - 태극기, 현수막, 간판)
3. Products (15개 - 각 분류별 5개씩)
4. Clients (6개 - 더미 거래처)
5. Orders (5개 - 테스트 주문서)
6. OrderItems (10개)
7. Cards
8. CardItems
9. EventLogs
10. **SalesClosings** (신규)
11. **SalesClosingItems** (신규)
12. **TaxInvoices** (신규)
13. **TaxInvoiceItems** (신규)
14. **Payments** (신규)
15. **BankTransactions** (신규)

### 주요 컬럼명 (자주 헷갈리는 것들)

#### Products 테이블
- ✅ `DefaultSpec` (규격)
- ✅ `IsDeleted` (NOT NULL, 필수)
- ❌ ~~Size~~, ~~Unit~~, ~~UnitPrice~~ (없음!)

#### OrderItems 테이블
- ✅ `Spec` (규격)
- ✅ `Description` (설명)
- ✅ `LineNumber` (라인 번호)
- ✅ `IsDeleted` (NOT NULL, 필수)
- ❌ ~~ProductCode~~, ~~ProductName~~, ~~ProductSize~~, ~~TotalPrice~~, ~~Memo~~ (없음!)

#### Orders 테이블
- ✅ `IsSalesClosed` (매출 마감 여부)
- ✅ `SalesClosingItemId` (매출 마감 항목 ID)

#### Client 테이블
- ✅ `BusinessNumber` (사업자등록번호)
- ✅ `CeoName` (대표자명)
- ✅ `BusinessType` (업태)
- ✅ `BusinessItem` (종목)

---

## 📁 프로젝트 구조

```
C:\dongsanMES\
├── MESSystem\
│   ├── appsettings.json (ConnectionString: MESSystem.db)
│   ├── MESSystem.db ⬅️ SQLite 데이터베이스
│   ├── migrate_erp.sql ⬅️ 마이그레이션 스크립트
│   ├── seed_data.sql ⬅️ 더미 데이터
│   ├── DatabaseMigrator.cs
│   ├── Program.cs
│   ├── Data\
│   │   └── ApplicationDbContext.cs
│   ├── Models\
│   │   ├── User.cs
│   │   ├── Category.cs
│   │   ├── Product.cs
│   │   ├── Client.cs
│   │   ├── Order.cs
│   │   ├── OrderItem.cs
│   │   ├── Card.cs
│   │   ├── CardItem.cs
│   │   ├── EventLog.cs
│   │   ├── SalesClosing.cs ⬅️ 신규
│   │   ├── SalesClosingItem.cs ⬅️ 신규
│   │   ├── TaxInvoice.cs ⬅️ 신규
│   │   ├── TaxInvoiceItem.cs ⬅️ 신규
│   │   ├── Payment.cs ⬅️ 신규
│   │   └── BankTransaction.cs ⬅️ 신규
│   ├── Pages\
│   │   ├── Admin\
│   │   │   ├── Users\ ⬅️ 신규
│   │   │   │   ├── Index.cshtml
│   │   │   │   ├── Create.cshtml
│   │   │   │   └── Edit.cshtml
│   │   │   ├── Categories\
│   │   │   ├── Clients\
│   │   │   ├── Products\
│   │   │   └── Orders\
│   │   └── Cards\
│   └── Services\
└── MESCollector\
```

---

## 🔐 로그인 계정

| Username | Password | Role | 설명 |
|----------|----------|------|------|
| admin | admin123 | 관리자 | 시스템 관리자 (삭제 불가) |
| field01 | user123 | 사용자 | 현장 작업자 |
| manager01 | manager123 | 관리자 | 관리자 |
| field02 | user123 | 사용자 | 현장 작업자 |
| accounting01 | account123 | 관리자 | 회계 담당자 |

---

## 🚀 실행 방법

### 1. 빌드
```powershell
cd C:\dongsanMES\MESSystem
dotnet clean
dotnet restore
dotnet build
```

### 2. 실행
```powershell
dotnet run
```

### 3. 접속
```
http://localhost:5000
```

---

## 🔧 마이그레이션 상태

### 완료된 작업 ✅
1. ✅ migrate_erp.sql 실행 (DB Browser)
2. ✅ seed_data.sql 실행 (DB Browser)
3. ✅ MESSystem.db 저장 완료
4. ✅ 빌드 오류 모두 수정
5. ✅ Razor 구문 오류 수정
6. ✅ 컬럼명 불일치 수정

### 확인 완료 ✅
- ✅ Clients: 6개
- ✅ Products: 15개
- ✅ Orders: 5개
- ✅ OrderItems: 10개
- ✅ Users: 5명
- ✅ ERP 테이블: 6개 생성

---

## ⏭️ 다음 단계 (미완료)

### Phase 5: ERP UI 개발 (예정)
- [ ] 매출 마감 화면
  - [ ] 거래처별 기간 조회
  - [ ] 주문서 선택
  - [ ] 할인/추가 비용 입력
  - [ ] 매출 확정
- [ ] 세금계산서 관리
  - [ ] XML 생성 서비스
  - [ ] 목록/상세 화면
  - [ ] 다운로드 기능
- [ ] 입금 관리
  - [ ] 수동 입금 등록
  - [ ] 입금 매칭
  - [ ] 미수금 대시보드

---

## 🐛 알려진 이슈

### 해결됨 ✅
- ✅ SQLite Error: 'no such column: o.IsSalesClosed' → migrate_erp.sql 실행으로 해결
- ✅ 'table Products has no column named Size' → seed_data.sql 컬럼명 수정
- ✅ 'NOT NULL constraint failed: Products.IsDeleted' → IsDeleted 컬럼 추가
- ✅ Razor 구문 오류 (RZ1031) → if-else 블록으로 수정
- ✅ 한글 단위 파싱 오류 (CS1061) → 괄호로 감싸기
- ✅ DatabaseMigrator namespace 오류 → using MESSystem 추가

### 현재 이슈
- 없음 (모두 해결됨)

---

## 📝 중요 참고사항

### Razor 구문 주의사항
1. **한글 단위 사용 시**: 반드시 괄호로 감싸기
   - ❌ `@Model.Count개`
   - ✅ `@(Model.Count)개`

2. **Tag Helper 속성에 C# 코드 사용 금지**
   - ❌ `<input ... @(condition ? "disabled" : "") />`
   - ✅ if-else 블록으로 분리

### 데이터베이스 작업 시
1. **파일명**: `MESSystem.db` (mes.db 아님!)
2. **변경 후 저장**: DB Browser에서 Ctrl+S 필수
3. **컬럼명 확인**: Models 폴더의 C# 클래스 참고

### Git 워크플로우
1. 항상 `genspark_ai_developer` 브랜치에서 작업
2. 변경사항 커밋 → 푸시 → PR 자동 업데이트
3. PR: https://github.com/kyj119/dongsanMES/pull/1

---

## 🔄 새 채팅 시작 시 전달사항

**이 파일(`PROJECT_STATUS.md`)을 먼저 읽어주세요!**

그 다음:
1. 현재 작업 중인 기능이 무엇인지
2. 발생한 오류 메시지 (있다면)
3. 원하는 작업 내용

이렇게 알려주시면 컨텍스트를 빠르게 파악할 수 있습니다.

---

**마지막 성공 상태**: 데이터베이스 마이그레이션 완료, 앱 실행 준비 완료 ✅
