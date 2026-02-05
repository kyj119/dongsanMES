# 🚀 MES 시스템 - 서버 PC 이전 완료 가이드

## 📦 배포 파일 다운로드

### 파일 정보
- **파일명**: `MESSystem_Deploy.tar.gz`
- **크기**: 15MB
- **위치**: `/home/user/webapp/MESSystem_Deploy.tar.gz`

### 포함 내용
```
MESSystem/
├── ✅ ASP.NET Core 프로젝트 전체
├── ✅ database_schema.sql (DB 생성 스크립트)
├── ✅ deploy.bat (자동 배포 스크립트)
├── ✅ appsettings.json (설정 파일)
└── ✅ 모든 소스 코드

README.md
DEPLOYMENT_GUIDE.md (상세 배포 가이드)
QUICK_START.md (빠른 시작 가이드)
```

---

## 🎯 서버 PC에서 해야 할 작업 (요약)

### 1️⃣ 사전 준비 (한 번만, 20분)

#### A. .NET 8.0 Runtime 설치
```
다운로드: https://dotnet.microsoft.com/download/dotnet/8.0
설치 파일: ASP.NET Core Runtime 8.0.x - Windows Hosting Bundle
설치 후: 서버 재시작
```

#### B. SQL Server 2019 Express 설치
```
다운로드: https://www.microsoft.com/ko-kr/sql-server/sql-server-downloads
설치 유형: Express (무료)
중요: sa 비밀번호 설정 (예: Admin1234!)
```

#### C. IIS 활성화
```
제어판 > 프로그램 > Windows 기능 켜기/끄기
체크: 인터넷 정보 서비스 (전체)
```

---

### 2️⃣ 파일 전송 및 압축 해제 (1분)

```
1. MESSystem_Deploy.tar.gz 다운로드
2. 서버 PC로 전송 (USB, 네트워크 등)
3. 압축 해제 → C:\MESSystem\
```

---

### 3️⃣ 자동 배포 실행 (3분)

```
1. C:\MESSystem\ 폴더 열기
2. deploy.bat 우클릭 > "관리자 권한으로 실행"
3. 화면 지시 따라가기
4. 완료 메시지 확인
```

**자동 수행 내용:**
- ✅ 프로젝트 빌드 (Release 모드)
- ✅ C:\inetpub\wwwroot\MESSystem\ 복사
- ✅ IIS 애플리케이션 풀 생성 (MESSystemPool)
- ✅ IIS 웹 사이트 생성 (포트 80)
- ✅ 권한 설정

---

### 4️⃣ 설정 파일 수정 (1분)

#### 파일: `C:\inetpub\wwwroot\MESSystem\appsettings.json`

**수정 전:**
```json
"Password=CHANGE_ME"
```

**수정 후:**
```json
"Password=Admin1234!"
```
(SQL Server sa 비밀번호로 변경)

**전체 연결 문자열 예시:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=MESSystem;User Id=sa;Password=Admin1234!;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

---

### 5️⃣ 데이터베이스 생성 (2분)

#### 방법 A: SQL Server Management Studio (SSMS)
```
1. SSMS 실행
2. 연결: .\SQLEXPRESS (sa / Admin1234!)
3. 파일 열기: C:\MESSystem\database_schema.sql
4. 실행 (F5)
5. "완료" 메시지 확인
```

#### 방법 B: 명령줄 (빠름)
```cmd
sqlcmd -S .\SQLEXPRESS -U sa -P Admin1234! -i C:\MESSystem\database_schema.sql
```

**생성 결과:**
- ✅ MESSystem 데이터베이스
- ✅ 8개 테이블 (Users, Categories, Products, Orders, ...)
- ✅ 초기 데이터 (관리자, 분류)

---

### 6️⃣ 접속 테스트 (1분)

```
1. 브라우저 열기: http://localhost

2. 로그인:
   아이디: admin
   비밀번호: admin123

3. 성공! "주문서 목록" 화면 표시
```

---

## 🌐 네트워크 접속 설정

### 서버 PC에서 IP 확인
```cmd
ipconfig
```
→ IPv4 주소 확인 (예: 192.168.0.10)

### 방화벽 설정
```
1. 제어판 > Windows Defender 방화벽 > 고급 설정
2. 인바운드 규칙 > 새 규칙
3. 포트: TCP 80
4. 연결 허용
5. 이름: MES System
```

### 다른 PC에서 접속
```
브라우저: http://192.168.0.10
```

---

## 📁 공유폴더 설정 (디자인 파일용)

### 192.168.0.122 서버에서

```
1. D:\Designs 폴더 생성

2. 우클릭 > 속성 > 공유 탭
   "고급 공유" 클릭
   "이 폴더 공유" 체크
   공유 이름: Designs

3. 권한:
   Everyone: 모든 권한 (또는 특정 사용자)

4. 확인

5. 네트워크 경로: \\192.168.0.122\Designs\
```

### 폴더 구조
```
\\192.168.0.122\Designs\
├── 2026\
│   ├── 01\
│   ├── 02\
│   └── ...
├── Archive\
└── Templates\
```

---

## 🔍 배포 완료 체크리스트

### 소프트웨어
- [ ] .NET 8.0 Runtime 설치
- [ ] SQL Server 2019 Express 설치
- [ ] IIS 활성화 및 실행

### 배포
- [ ] MESSystem 프로젝트 압축 해제
- [ ] deploy.bat 실행 (관리자 권한)
- [ ] appsettings.json 수정 (DB 비밀번호)
- [ ] database_schema.sql 실행

### 테스트
- [ ] http://localhost 접속 가능
- [ ] admin / admin123 로그인 성공
- [ ] 주문서 목록 화면 표시
- [ ] 다른 PC에서 http://[서버IP] 접속 확인

### 공유폴더
- [ ] \\192.168.0.122\Designs\ 생성
- [ ] 공유 권한 설정
- [ ] 네트워크 접근 테스트

---

## ⚠️ 자주 발생하는 문제

### 1. "500 Internal Server Error"
**원인**: DB 연결 실패
**해결**: appsettings.json의 Password 확인

### 2. "사이트에 연결할 수 없음"
**원인**: IIS 웹 사이트 미시작
**해결**: IIS 관리자 > MESSystem > 시작

### 3. "데이터베이스를 찾을 수 없습니다"
**원인**: database_schema.sql 미실행
**해결**: SSMS에서 스크립트 실행

### 4. 다른 PC에서 접속 안 됨
**원인**: 방화벽
**해결**: 포트 80 인바운드 규칙 추가

---

## 📚 추가 문서

### 상세 가이드
- `DEPLOYMENT_GUIDE.md` - 단계별 상세 배포 가이드 (11개 섹션)
- `QUICK_START.md` - 5분 빠른 시작 가이드
- `README.md` - 프로젝트 개요 및 현재 상태

### 도움말
- IIS 로그: `C:\inetpub\logs\LogFiles\`
- 이벤트 뷰어: Windows 로그 > 응용 프로그램

---

## 🎯 배포 후 다음 단계

### 즉시 (Day 1)
1. ✅ 웹 애플리케이션 배포 완료
2. ✅ 로그인/권한 시스템 작동
3. ⏭️ 관리자 화면 개발 (품목/주문서 관리)

### 곧 (Day 2-3)
1. ⏭️ Collector 프로그램 개발
2. ⏭️ TOPAZ_RIP 연동
3. ⏭️ 자동 작업 추적

### 이후 (Day 4-7)
1. ⏭️ 현장 터치패널 화면
2. ⏭️ 재주문 기능
3. ⏭️ 통합 테스트

---

## 📞 지원 정보

### 기본 로그인
- **관리자**: admin / admin123
- **현장 사용자**: field01 / user123

### 데이터베이스
- **서버**: .\SQLEXPRESS
- **DB**: MESSystem
- **사용자**: sa / [설정한 비밀번호]

### 네트워크
- **로컬**: http://localhost
- **네트워크**: http://[서버IP]
- **공유폴더**: \\192.168.0.122\Designs\

---

**전체 배포 시간: 약 30분**
**난이도: ★★☆☆☆**

배포 완료 후 다음 개발 단계로 진행하세요! 🚀
