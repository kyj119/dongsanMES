# 데이터베이스 마이그레이션 빠른 가이드

## ✅ 방법 1: DB Browser for SQLite 사용 (가장 쉬움, 추천!)

이미 DB Browser를 다운받으셨다면 이 방법이 가장 쉽습니다!

### 단계:

1. **DB Browser for SQLite 실행**

2. **데이터베이스 열기**
   - 메뉴: `Database` → `Open Database` (또는 Ctrl+O)
   - 파일 선택: `C:\dongsanMES\MESSystem\mes.db`

3. **SQL 실행 탭**
   - 상단 탭: `Execute SQL` 클릭 (또는 F5 탭)

4. **migrate_erp.sql 실행**
   - 방법 A (복사/붙여넣기):
     1. `C:\dongsanMES\MESSystem\migrate_erp.sql` 파일을 메모장으로 열기
     2. 전체 내용 복사 (Ctrl+A → Ctrl+C)
     3. DB Browser의 SQL 창에 붙여넣기 (Ctrl+V)
     4. ▶️ 실행 버튼 클릭 (또는 Ctrl+Enter)
   
   - 방법 B (파일 불러오기):
     1. SQL 창 위의 📂 아이콘 클릭 (또는 File → Import → SQL file)
     2. `migrate_erp.sql` 선택
     3. ▶️ 실행 버튼 클릭

5. **seed_data.sql 실행**
   - 위와 동일한 방법으로 `seed_data.sql` 실행

6. **저장**
   - 메뉴: `File` → `Write Changes` (또는 Ctrl+S)
   - "Save successful" 메시지 확인

7. **확인**
   - `Browse Data` 탭 클릭
   - 테이블 선택: `Clients` → 6개 데이터 확인
   - 테이블 선택: `Products` → 15개 데이터 확인
   - 테이블 선택: `Users` → 5개 데이터 확인

---

## ✅ 방법 2: C# 코드로 마이그레이션 (자동화)

### 실행:

```powershell
cd C:\dongsanMES\MESSystem
dotnet run -- migrate
```

### 예상 출력:

```
=== 데이터베이스 마이그레이션 시작 ===

1. ERP 테이블 생성 중...
   ✅ 완료

2. 더미 데이터 삽입 중...
   ✅ 완료

3. 마이그레이션 결과 확인:
   - 총 테이블 수: 14
   - SalesClosings: ✅
   - TaxInvoices: ✅
   - Payments: ✅
   - BankTransactions: ✅

   데이터 확인:
   - 거래처: 6개
   - 품목: 15개
   - 주문서: 5개
   - 사용자: 5개

=== 마이그레이션 성공! ===
```

---

## ❌ 오류 해결

### 오류 1: "Build failed"

```powershell
# 먼저 빌드만 시도
cd C:\dongsanMES\MESSystem
dotnet clean
dotnet restore
dotnet build
```

빌드 오류가 있으면 메시지를 공유해주세요.

### 오류 2: "already exists"

이미 마이그레이션이 완료된 것입니다. 무시해도 됩니다.

### 오류 3: "column already exists"

이미 컬럼이 추가되어 있습니다. 무시해도 됩니다.

---

## 🎯 마이그레이션 완료 후

### 1. 앱 실행

```powershell
cd C:\dongsanMES\MESSystem
dotnet run
```

### 2. 브라우저 접속

```
http://localhost:5000
```

### 3. 로그인 테스트

- **admin / admin123** (시스템 관리자)
- **accounting01 / account123** (회계 담당자)
- **manager01 / manager123** (관리자)
- **field01 / user123** (현장 작업자)

### 4. 사용자 관리 확인

```
http://localhost:5000/Admin/Users
```

---

## 📝 추가 정보

### DB Browser 다운로드 링크

이미 설치하셨지만, 다시 필요하면:
https://sqlitebrowser.org/dl/

**권장:** DB Browser for SQLite - Standard installer for 64-bit Windows

### SQLite 명령줄 도구 (선택사항)

PowerShell에서 명령줄을 사용하고 싶다면:

```powershell
# Chocolatey로 설치 (권장)
choco install sqlite

# 또는 수동 다운로드
# https://www.sqlite.org/download.html
# sqlite-tools-win-x64-*.zip 다운로드
```

---

## 🚀 빠른 시작 (요약)

1. **DB Browser 실행**
2. **Database → Open Database** → `mes.db` 선택
3. **Execute SQL 탭**
4. **migrate_erp.sql 내용 복사/붙여넣기** → ▶️ 실행
5. **seed_data.sql 내용 복사/붙여넣기** → ▶️ 실행
6. **File → Write Changes** (저장)
7. **dotnet run** (앱 실행)
8. **브라우저에서 http://localhost:5000 접속**

---

**문제가 생기면 스크린샷과 함께 오류 메시지를 공유해주세요!** 🚀
