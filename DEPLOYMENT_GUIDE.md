# MES 시스템 배포 가이드

## 📦 배포 파일 다운로드

### 파일 위치
```
/home/user/webapp/MESSystem_Deploy.tar.gz
```

### 서버 PC로 전송
```
1. 파일 다운로드 (브라우저 또는 FTP)
2. 서버 PC의 임시 폴더에 저장 (예: C:\Temp\)
3. 압축 해제 (WinRAR, 7-Zip 등)
```

---

## 🗄️ 3. 데이터베이스 설정

### 3.1 SQL Server 연결 확인
```
1. SQL Server Management Studio (SSMS) 실행
2. 연결 정보:
   서버 이름: .\SQLEXPRESS (또는 localhost\SQLEXPRESS)
   인증: SQL Server 인증
   로그인: sa
   암호: [설치 시 설정한 비밀번호]
```

### 3.2 데이터베이스 생성
```sql
1. SSMS에서 새 쿼리 열기

2. database_schema.sql 파일 열기 (C:\MESSystem\database_schema.sql)

3. 실행 (F5 또는 "실행" 버튼)

4. 확인:
   - 왼쪽 트리에서 "데이터베이스" 새로고침
   - "MESSystem" 데이터베이스 생성 확인
   - 테이블 8개 확인 (Users, Categories, Products, Orders, ...)
```

### 3.3 연결 문자열 확인
```
서버: .\SQLEXPRESS
데이터베이스: MESSystem
인증: SQL Server 인증
사용자: sa
암호: [설정한 비밀번호]
```

---

## 🌐 4. IIS 웹 사이트 설정

### 4.1 프로젝트 빌드 (서버 PC에서)

```cmd
1. 명령 프롬프트 (관리자 권한) 실행

2. 프로젝트 폴더로 이동
cd C:\MESSystem

3. 빌드 및 게시
dotnet publish -c Release -o C:\inetpub\wwwroot\MESSystem
```

### 4.2 appsettings.json 수정

파일 위치: `C:\inetpub\wwwroot\MESSystem\appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=MESSystem;User Id=sa;Password=Admin1234!;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "SharedFolderPath": "\\\\192.168.0.122\\Designs\\",
  "UploadPath": "wwwroot/uploads/"
}
```

**중요: Password를 실제 sa 비밀번호로 변경하세요!**

### 4.3 IIS 애플리케이션 풀 생성

```
1. IIS 관리자 실행 (시작 > inetmgr 검색)

2. 왼쪽 트리에서 "애플리케이션 풀" 클릭

3. 오른쪽 "작업" 패널에서 "애플리케이션 풀 추가" 클릭

4. 설정:
   이름: MESSystemPool
   .NET CLR 버전: "관리 코드 없음" 선택
   관리되는 파이프라인 모드: 통합
   
5. 확인
```

### 4.4 IIS 웹 사이트 생성

```
1. IIS 관리자 > 왼쪽 트리 > "사이트" 우클릭 > "웹 사이트 추가"

2. 설정:
   사이트 이름: MESSystem
   애플리케이션 풀: MESSystemPool (드롭다운에서 선택)
   실제 경로: C:\inetpub\wwwroot\MESSystem
   바인딩:
     - 유형: http
     - IP 주소: 모든 할당되지 않음
     - 포트: 80 (또는 8080)
     - 호스트 이름: (비워둠)
   
3. 확인
```

### 4.5 권한 설정

```
1. 탐색기에서 C:\inetpub\wwwroot\MESSystem 폴더 우클릭
   
2. 속성 > 보안 탭 > 편집

3. 추가 버튼 클릭

4. "IIS_IUSRS" 입력 > 이름 확인 > 확인

5. 권한: "읽기 및 실행", "폴더 내용 보기", "읽기" 체크

6. 적용 > 확인
```

---

## 🔧 5. 공유폴더 설정

### 5.1 공유폴더 생성 (192.168.0.122 서버에서)

```
1. D:\Designs 폴더 생성 (또는 원하는 위치)

2. 폴더 우클릭 > 속성 > 공유 탭

3. "고급 공유" 버튼 클릭

4. "이 폴더 공유" 체크
   공유 이름: Designs
   
5. 권한 버튼 클릭
   - Everyone: 모든 권한 체크 (또는 특정 사용자만)
   
6. 적용 > 확인

7. 네트워크 경로 확인: \\192.168.0.122\Designs\
```

### 5.2 폴더 구조 생성

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

## 🚀 6. 서비스 시작 및 테스트

### 6.1 웹 사이트 시작

```
1. IIS 관리자 > 사이트 > MESSystem 선택

2. 오른쪽 "작업" 패널에서 "시작" 클릭

3. 상태 확인: "시작됨" 표시
```

### 6.2 방화벽 설정 (필요 시)

```
1. 제어판 > Windows Defender 방화벽 > 고급 설정

2. 인바운드 규칙 > 새 규칙

3. 규칙 종류: 포트
   프로토콜: TCP
   특정 로컬 포트: 80 (또는 8080)
   작업: 연결 허용
   이름: MES System Web
   
4. 마침
```

### 6.3 접속 테스트

```
1. 서버 PC에서 브라우저 열기

2. 주소 입력:
   http://localhost
   또는
   http://localhost:8080 (포트 변경한 경우)

3. 로그인 화면이 나타나면 성공!
   - 아이디: admin
   - 비밀번호: admin123

4. 다른 PC에서 접속:
   http://192.168.0.XXX (서버 PC IP)
```

---

## 🔍 7. 문제 해결

### 7.1 500 Internal Server Error

**원인**: appsettings.json 설정 오류

**해결**:
```
1. appsettings.json 열기
2. ConnectionStrings 확인
3. Password를 실제 sa 비밀번호로 수정
4. IIS에서 웹 사이트 재시작
```

### 7.2 데이터베이스 연결 실패

**원인**: SQL Server 서비스 중지 또는 인증 오류

**해결**:
```
1. 서비스 확인:
   - 시작 > services.msc 검색
   - "SQL Server (SQLEXPRESS)" 확인
   - 중지된 경우 시작
   
2. SSMS에서 연결 테스트
   - 서버: .\SQLEXPRESS
   - 인증: SQL Server 인증
   - 로그인: sa
   - 연결 성공 확인
```

### 7.3 403 Forbidden Error

**원인**: IIS 권한 부족

**해결**:
```
1. C:\inetpub\wwwroot\MESSystem 폴더 우클릭
2. 속성 > 보안 > 편집
3. IIS_IUSRS 계정에 읽기 권한 부여
4. 적용 > 확인
```

### 7.4 공유폴더 접근 실패

**원인**: 네트워크 권한 문제

**해결**:
```
1. 서버 PC에서 테스트:
   - 탐색기 주소창에 입력: \\192.168.0.122\Designs\
   - 폴더가 열리면 정상
   
2. 권한 확인:
   - 공유폴더 속성 > 공유 탭 > 권한
   - Everyone 또는 특정 사용자 추가
   
3. Windows 자격 증명 추가 (필요 시):
   - 제어판 > 자격 증명 관리자
   - Windows 자격 증명 추가
   - 주소: \\192.168.0.122
   - 사용자 이름/비밀번호 입력
```

---

## 📊 8. 로그 확인

### 8.1 IIS 로그 위치
```
C:\inetpub\logs\LogFiles\W3SVC1\
```

### 8.2 애플리케이션 로그
```
이벤트 뷰어 > Windows 로그 > 응용 프로그램
```

---

## 🔐 9. 보안 강화 (운영 환경)

### 9.1 비밀번호 해시 구현

**현재 상태**: 비밀번호 평문 저장 (개발 단계)

**TODO**: BCrypt 라이브러리 추가 및 해시 구현

```bash
dotnet add package BCrypt.Net-Next
```

### 9.2 HTTPS 설정

```
1. SSL 인증서 발급 (내부망: 자체 서명 인증서)

2. IIS > 사이트 > MESSystem > 바인딩 편집

3. 추가:
   유형: https
   포트: 443
   SSL 인증서: 선택
```

---

## 📝 10. 배포 체크리스트

### 사전 준비
- [ ] Windows Server 또는 Windows 10/11
- [ ] 관리자 권한
- [ ] 인터넷 연결 (설치용)

### 소프트웨어 설치
- [ ] .NET 8.0 Hosting Bundle
- [ ] IIS 활성화
- [ ] SQL Server 2019 Express
- [ ] SQL Server Management Studio (선택)

### 데이터베이스
- [ ] SQL Server 서비스 시작
- [ ] MESSystem 데이터베이스 생성
- [ ] 8개 테이블 생성 확인
- [ ] 초기 데이터 확인 (Users, Categories)

### 웹 애플리케이션
- [ ] 프로젝트 빌드 (dotnet publish)
- [ ] C:\inetpub\wwwroot\MESSystem 복사
- [ ] appsettings.json 수정 (DB 연결)
- [ ] IIS 애플리케이션 풀 생성
- [ ] IIS 웹 사이트 생성
- [ ] 폴더 권한 설정 (IIS_IUSRS)

### 공유폴더
- [ ] \\192.168.0.122\Designs\ 생성
- [ ] 공유 권한 설정
- [ ] 폴더 구조 생성 (2026/01, 02, ...)

### 테스트
- [ ] http://localhost 접속 확인
- [ ] 로그인 테스트 (admin/admin123)
- [ ] 다른 PC에서 접속 테스트
- [ ] 공유폴더 접근 테스트

### 방화벽
- [ ] 포트 80 (또는 8080) 열기
- [ ] 필요 시 내부 IP만 허용

---

## 📞 11. 지원 정보

### 로그인 정보
- **관리자**: admin / admin123
- **현장 사용자**: field01 / user123

### 데이터베이스 정보
- **서버**: .\SQLEXPRESS
- **DB 이름**: MESSystem
- **인증**: SQL Server 인증 (sa)

### 네트워크 정보
- **공유폴더**: \\192.168.0.122\Designs\
- **웹 사이트**: http://[서버IP]

---

**배포 완료 후 다음 단계: Collector 개발 및 설치**
