# 데이터베이스 마이그레이션 가이드

## 📋 실행 방법

### Option 1: SQL 스크립트 직접 실행 (빠름, 추천)

#### Windows PowerShell에서:

```powershell
# 1. MESSystem 디렉토리로 이동
cd C:\dongsanMES\MESSystem

# 2. SQLite DB 경로 확인
# appsettings.json에서 ConnectionString 확인
# 예: "Data Source=mes.db"

# 3. ERP 마이그레이션 실행
Get-Content migrate_erp.sql | sqlite3 mes.db

# 4. 더미 데이터 삽입
Get-Content seed_data.sql | sqlite3 mes.db

# 5. 확인
sqlite3 mes.db "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;"
```

#### Linux/Mac에서:

```bash
# 1. MESSystem 디렉토리로 이동
cd /home/user/webapp/MESSystem

# 2. ERP 마이그레이션 실행
sqlite3 mes.db < migrate_erp.sql

# 3. 더미 데이터 삽입
sqlite3 mes.db < seed_data.sql

# 4. 확인
sqlite3 mes.db "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;"
```

---

### Option 2: Entity Framework Migration (권장, 프로덕션)

```powershell
# 1. MESSystem 디렉토리로 이동
cd C:\dongsanMES\MESSystem

# 2. 마이그레이션 생성
dotnet ef migrations add AddERPSalesManagement

# 3. 데이터베이스 업데이트
dotnet ef database update

# 4. 더미 데이터는 SQL로 실행
Get-Content seed_data.sql | sqlite3 mes.db
```

---

## ✅ 마이그레이션 후 확인

### 1. 테이블 생성 확인

```sql
-- SQLite에서 실행
SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;

-- 결과에 다음 테이블들이 있어야 함:
-- BankTransactions
-- SalesClosings
-- SalesClosingItems
-- TaxInvoices
-- TaxInvoiceItems
-- Payments
```

### 2. Clients 테이블 컬럼 확인

```sql
PRAGMA table_info(Clients);

-- BusinessNumber, CeoName, BusinessType, BusinessItem 컬럼 추가 확인
```

### 3. Orders 테이블 컬럼 확인

```sql
PRAGMA table_info(Orders);

-- IsSalesClosed, SalesClosingItemId 컬럼 추가 확인
```

### 4. 더미 데이터 확인

```sql
-- 거래처 6개
SELECT COUNT(*) FROM Clients WHERE Id >= 1 AND Id <= 6;

-- 품목 15개
SELECT COUNT(*) FROM Products WHERE Id >= 1 AND Id <= 15;

-- 주문서 5개
SELECT COUNT(*) FROM Orders WHERE Id >= 1 AND Id <= 5;

-- 사용자 5명
SELECT COUNT(*) FROM Users WHERE Id >= 1 AND Id <= 5;
```

---

## 🚨 문제 해결

### 오류 1: "table already exists"

```sql
-- 테이블 삭제 후 재실행
DROP TABLE IF EXISTS SalesClosings;
DROP TABLE IF EXISTS SalesClosingItems;
DROP TABLE IF EXISTS TaxInvoices;
DROP TABLE IF EXISTS TaxInvoiceItems;
DROP TABLE IF EXISTS Payments;
DROP TABLE IF EXISTS BankTransactions;

-- 그 다음 migrate_erp.sql 다시 실행
```

### 오류 2: "column already exists"

이미 컬럼이 추가되어 있는 경우 무시해도 됩니다.

```sql
-- 확인
PRAGMA table_info(Clients);
PRAGMA table_info(Orders);
```

### 오류 3: "foreign key constraint failed"

```sql
-- 외래키 체크 비활성화 (임시)
PRAGMA foreign_keys = OFF;

-- 마이그레이션 실행
.read migrate_erp.sql

-- 외래키 체크 활성화
PRAGMA foreign_keys = ON;
```

---

## 📊 더미 데이터 내용

### 거래처 (6개)
1. (주)동산무역 - VIP 거래처
2. 서울광고기획 - 월 정기 거래
3. OO건설(주) - 대형 현수막
4. 부산마케팅 - 분기별 주문
5. 대한상사 - 소량 다품종
6. 글로벌무역 - 수출용

### 품목 (15개)
- **태극기 (5개)**: 90x135, 150x225, 소형, 대형, 차량용
- **현수막 (5개)**: 표준형, 대형, 소형, 초대형, 배너형
- **간판 (5개)**: 아크릴 소형/중형, LED 소형/대형, 네온사인

### 주문서 (5개)
- 주문서 1: 태극기 100매 + 소형 50매 (완료)
- 주문서 2: 현수막 표준 10개 + 대형 3개 (완료)
- 주문서 3: 현수막 초대형 5개 (완료)
- 주문서 4: 간판 아크릴 2개 + LED 1개 (진행중)
- 주문서 5: 태극기 150x225 30매 + 차량용 100매 (완료)

### 사용자 (5명)
- admin / admin123 (시스템 관리자)
- field01 / user123 (현장 사용자)
- manager01 / manager123 (관리자)
- field02 / user123 (현장 사용자)
- accounting01 / account123 (회계 관리자)

---

## 🔐 보안 주의사항

**⚠️ 프로덕션 환경에서는:**

1. 더미 데이터 삭제
2. 비밀번호 해시 적용
3. 테스트 계정 삭제
4. 실제 거래처 데이터 입력

```sql
-- 더미 데이터 삭제 (프로덕션)
DELETE FROM Users WHERE Id > 2; -- admin, field01만 남기기
DELETE FROM Orders WHERE Id >= 1 AND Id <= 5;
DELETE FROM OrderItems WHERE OrderId >= 1 AND OrderId <= 5;
DELETE FROM Clients WHERE Id >= 1 AND Id <= 6;
DELETE FROM Products WHERE Id >= 1 AND Id <= 15;
```

---

## ✅ 완료 체크리스트

- [ ] migrate_erp.sql 실행
- [ ] seed_data.sql 실행
- [ ] 테이블 생성 확인
- [ ] 더미 데이터 확인
- [ ] 빌드 테스트 (dotnet build)
- [ ] 앱 실행 테스트 (dotnet run)
- [ ] 로그인 테스트 (admin/admin123)
- [ ] 사용자 관리 페이지 접근 확인

---

**작성일**: 2026-02-11  
**버전**: 1.0
