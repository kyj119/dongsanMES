# MES 시스템 빠른 설치 가이드 (5분 완성)

## ⚡ 빠른 시작 (요약)

### 필수 사전 설치 (한 번만)
1. .NET 8.0 Hosting Bundle → https://dotnet.microsoft.com/download/dotnet/8.0
2. SQL Server 2019 Express → https://www.microsoft.com/sql-server/sql-server-downloads
3. IIS 활성화 (Windows 기능)

### 배포 실행 (3단계)
1. 프로젝트 압축 풀기 → `C:\MESSystem\`
2. **관리자 권한**으로 `deploy.bat` 실행
3. SQL Server에서 `database_schema.sql` 실행

---

## 📋 상세 설치 가이드

### 1단계: 필수 소프트웨어 설치 (20분)

#### A. .NET 8.0 설치
```
1. https://dotnet.microsoft.com/download/dotnet/8.0 접속
2. "ASP.NET Core Runtime 8.0.x - Windows Hosting Bundle" 다운로드
3. 설치 후 서버 재시작
```

#### B. SQL Server 설치
```
1. https://www.microsoft.com/ko-kr/sql-server/sql-server-downloads
2. "Express" 다운로드 및 실행
3. 설치 유형: "기본"
4. 인스턴스: SQLEXPRESS (기본값)
5. 인증 모드: "혼합 모드"
   → sa 비밀번호 설정: Admin1234! (기억할 것!)
```

#### C. IIS 활성화
```
1. 제어판 > 프로그램 > Windows 기능 켜기/끄기
2. 체크:
   ☑ 인터넷 정보 서비스
     ☑ 웹 관리 도구 > IIS 관리 콘솔
     ☑ World Wide Web 서비스 (전체)
3. 확인 후 대기 (약 3분)
```

---

### 2단계: 프로젝트 배포 (5분)

#### A. 파일 준비
```
1. MESSystem_Deploy.tar.gz 다운로드
2. 압축 해제 → C:\MESSystem\
3. 폴더 확인:
   C:\MESSystem\
   ├── database_schema.sql
   ├── deploy.bat
   ├── Program.cs
   ├── appsettings.json
   └── ...
```

#### B. 자동 배포 실행
```
1. C:\MESSystem\ 폴더 열기
2. deploy.bat 우클릭
3. "관리자 권한으로 실행" 선택
4. 화면 지시 따라가기
5. "배포 완료!" 메시지 확인
```

#### C. 설정 파일 수정
```
1. 메모장(관리자 권한)으로 열기:
   C:\inetpub\wwwroot\MESSystem\appsettings.json

2. Password 수정:
   "Password=Admin1234!" (SQL Server sa 비밀번호)

3. 저장 (Ctrl+S)
```

---

### 3단계: 데이터베이스 생성 (3분)

#### 방법 1: SQL Server Management Studio (추천)
```
1. SSMS 다운로드 (아직 없다면):
   https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms

2. SSMS 실행
3. 연결:
   서버 이름: .\SQLEXPRESS
   인증: SQL Server 인증
   로그인: sa
   암호: Admin1234!

4. 파일 열기: C:\MESSystem\database_schema.sql
5. 실행 (F5)
6. "명령이 완료되었습니다" 확인
```

#### 방법 2: 명령줄 (빠름)
```cmd
1. 명령 프롬프트 열기
2. 실행:
sqlcmd -S .\SQLEXPRESS -U sa -P Admin1234! -i C:\MESSystem\database_schema.sql
3. "MES 시스템 데이터베이스 생성 완료" 확인
```

---

### 4단계: 접속 테스트 (1분)

```
1. 브라우저 열기 (Chrome, Edge 등)
2. 주소 입력: http://localhost
3. 로그인 화면 확인
4. 로그인:
   아이디: admin
   비밀번호: admin123
5. "주문서 목록" 화면 확인 → 성공!
```

---

## 🔧 문제 해결 (자주 묻는 질문)

### Q1. "500 Internal Server Error" 발생
**원인**: 데이터베이스 연결 실패

**해결**:
```
1. appsettings.json 열기
2. Password 확인 (sa 비밀번호와 일치?)
3. SQL Server 서비스 시작 확인:
   - 시작 > services.msc 검색
   - "SQL Server (SQLEXPRESS)" 찾기
   - 상태가 "실행 중"인지 확인
   - 중지된 경우: 우클릭 > 시작
4. IIS에서 웹 사이트 재시작
```

### Q2. 로그인 화면이 안 나옴
**원인**: IIS 웹 사이트 미시작

**해결**:
```
1. 시작 > inetmgr 검색 (IIS 관리자)
2. 왼쪽 트리: 사이트 > MESSystem
3. 오른쪽: "시작" 버튼 클릭
4. 브라우저 새로고침
```

### Q3. 다른 PC에서 접속 안 됨
**원인**: 방화벽

**해결**:
```
1. 서버 PC IP 확인:
   - 명령 프롬프트: ipconfig
   - IPv4 주소 확인 (예: 192.168.0.10)

2. 방화벽 규칙 추가:
   - 제어판 > Windows Defender 방화벽
   - 고급 설정 > 인바운드 규칙
   - 새 규칙: 포트 80, TCP, 연결 허용

3. 다른 PC 브라우저:
   http://192.168.0.10 (서버 IP)
```

### Q4. 테이블이 생성 안 됨
**원인**: SQL 스크립트 실행 실패

**해결**:
```
1. SSMS에서 다시 시도:
   - 파일 > 열기 > database_schema.sql
   - 전체 선택 (Ctrl+A)
   - 실행 (F5)

2. 오류 메시지 확인
3. "MESSystem" 데이터베이스가 이미 있으면 삭제 후 재실행
```

---

## 📞 빠른 점검 체크리스트

배포 완료 후 다음을 확인하세요:

- [ ] .NET 8.0 설치 완료 (`dotnet --version` 확인)
- [ ] SQL Server 서비스 실행 중
- [ ] IIS 웹 사이트 "MESSystem" 시작됨
- [ ] `C:\inetpub\wwwroot\MESSystem\` 폴더 존재
- [ ] appsettings.json의 Password 수정됨
- [ ] MESSystem 데이터베이스 및 8개 테이블 존재
- [ ] http://localhost 접속 가능
- [ ] admin / admin123 로그인 성공

---

## 🎯 다음 단계

### 운영 준비
1. **관리자 비밀번호 변경** (보안)
   - SQL Server: Users 테이블에서 admin 비밀번호 수정
   
2. **공유폴더 설정**
   - \\192.168.0.122\Designs\ 폴더 생성
   - 공유 권한 설정
   
3. **백업 설정**
   - SQL Server 자동 백업 구성
   - 주간 전체 백업 + 일일 증분 백업

### Collector 설치 (Day 2-3 작업)
- Windows Service 프로그램 별도 설치
- TOPAZ_RIP 연동
- 작업 자동 추적

---

## 📝 연락처 정보

### 기본 로그인
- 관리자: admin / admin123
- 현장: field01 / user123

### 데이터베이스
- 서버: .\SQLEXPRESS
- DB: MESSystem
- 사용자: sa / Admin1234!

### 웹 사이트
- 로컬: http://localhost
- 네트워크: http://[서버IP]

---

**설치 시간: 약 30분 (처음 설치) / 5분 (재배포)**
**난이도: ★★☆☆☆ (중)**

문제 발생 시: 이벤트 뷰어 > Windows 로그 > 응용 프로그램 확인
