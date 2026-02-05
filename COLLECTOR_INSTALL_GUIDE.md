# 🚀 MES Collector 출력기 PC 설치 완벽 가이드

**작성일**: 2026-02-05  
**소요 시간**: 10분  
**난이도**: ⭐⭐ (쉬움)

---

## 🎯 설치 개요

```
1단계: 파일 준비 (2분)     → MESCollector 폴더 복사
2단계: .NET 설치 (3분)      → 무료 다운로드 및 설치
3단계: 설정 수정 (2분)      → appsettings.json 편집
4단계: 테스트 실행 (2분)    → 콘솔에서 실행
5단계: 서비스 등록 (1분)    → Windows Service 등록 (선택)
```

---

## 📦 1단계: 파일 준비

### 옵션 A: GitHub에서 다운로드 (권장)

**방법 1: 직접 다운로드**
```
1. 브라우저에서 GitHub 저장소 접속
2. "Code" → "Download ZIP" 클릭
3. ZIP 압축 해제
4. MESCollector 폴더를 C:\로 복사
```

**방법 2: Git Clone**
```bash
# Windows PowerShell 또는 CMD
cd C:\
git clone https://github.com/YOUR_REPO/MESSystem.git
cd MESSystem\MESCollector
```

### 옵션 B: 수동 복사

**필요한 파일 목록:**
```
MESCollector/
├── MESCollector.csproj
├── Program.cs
├── appsettings.json          ← 설정 파일 (중요!)
├── Models/
│   ├── CollectorSettings.cs
│   └── EventDto.cs
└── Services/
    ├── FileMonitorService.cs
    ├── FileParserService.cs
    └── ApiService.cs
```

**복사 방법:**
1. 샌드박스에서 `/home/user/webapp/MESCollector/` 전체 폴더 압축
2. 압축 파일을 출력기 PC로 전송
3. `C:\MESCollector\`에 압축 해제

---

## 🔧 2단계: .NET 8.0 Runtime 설치

### 다운로드
**공식 사이트**: https://dotnet.microsoft.com/download/dotnet/8.0

**설치할 파일**:
```
ASP.NET Core Runtime 8.0.x - Windows x64
또는
.NET Desktop Runtime 8.0.x - Windows x64
```

### 설치 확인
```cmd
# Windows CMD 또는 PowerShell
dotnet --version
```

**예상 출력**:
```
8.0.x
```

---

## ⚙️ 3단계: 설정 파일 수정

### appsettings.json 편집
```
위치: C:\MESCollector\appsettings.json
편집기: 메모장, VS Code, 또는 아무 텍스트 편집기
```

### 필수 수정 항목
```json
{
  "Collector": {
    "ServerUrl": "http://192.168.0.100:5000",  ← MES 서버 IP로 변경!
    "CollectorId": "COLLECTOR-MACHINE-01",      ← 출력기 식별자
    "WatchPaths": {
      "Preview": "C:\\TOPAZ_RIP\\preview",     ← 실제 경로로 변경
      "PrintLog": "C:\\TOPAZ_RIP\\printlog",   ← 실제 경로로 변경
      "Job": "C:\\TOPAZ_RIP\\job"              ← 실제 경로로 변경
    },
    "RetryCount": 3,
    "RetryDelaySeconds": 5
  }
}
```

### 설정 예시

**출력기 1 (1층)**
```json
{
  "Collector": {
    "ServerUrl": "http://192.168.0.100:5000",
    "CollectorId": "COLLECTOR-1F",
    "WatchPaths": {
      "Preview": "C:\\TOPAZ_RIP\\preview",
      "PrintLog": "C:\\TOPAZ_RIP\\printlog",
      "Job": "C:\\TOPAZ_RIP\\job"
    }
  }
}
```

**출력기 2 (2층)**
```json
{
  "Collector": {
    "ServerUrl": "http://192.168.0.100:5000",
    "CollectorId": "COLLECTOR-2F",
    "WatchPaths": {
      "Preview": "D:\\RIP\\preview",     ← 다른 경로도 가능
      "PrintLog": "D:\\RIP\\printlog",
      "Job": "D:\\RIP\\job"
    }
  }
}
```

---

## 🧪 4단계: 테스트 실행

### 방법 1: 콘솔에서 실행 (권장 - 테스트용)

```cmd
# Windows CMD 또는 PowerShell
cd C:\MESCollector
dotnet run
```

**예상 출력**:
```
[09:30:00] [INF] === MES Collector 시작 ===
[09:30:00] [INF] Collector ID: COLLECTOR-MACHINE-01
[09:30:00] [INF] 서버 URL: http://192.168.0.100:5000
[09:30:00] [INF] 서버 연결 성공!
[09:30:00] [INF] === 파일 모니터링 시작 ===
[09:30:00] [INF] Preview 폴더: C:\TOPAZ_RIP\preview
[09:30:00] [INF] PrintLog 폴더: C:\TOPAZ_RIP\printlog
[09:30:00] [INF] Job 폴더: C:\TOPAZ_RIP\job
```

### 테스트 파일 생성

**새 CMD 창 열기 (Collector는 계속 실행 중)**

```batch
@echo off
echo 테스트 파일 생성 중...

REM 1. 작업대기 이벤트
echo test > C:\TOPAZ_RIP\preview\20260205-99-1.bmp.tsc
timeout /t 3

REM 2. 작업시작 이벤트
(
echo JobName=테스트작업
echo Copies=100
echo StartTime=2026-02-05 09:30:00
) > C:\TOPAZ_RIP\printlog\20260205-99-1_093000.log
timeout /t 3

REM 3. 작업완료 이벤트
(
echo PrintFile=test.ai
echo DestSizeX=900.000000
echo EndDate=2026-02-05 09:35:00
) > C:\TOPAZ_RIP\job\20260205-99-10002.job

echo 테스트 완료!
pause
```

**파일 저장**: `C:\MESCollector\test_collector.bat`

**실행**:
```cmd
cd C:\MESCollector
test_collector.bat
```

### 예상 결과

**Collector 콘솔 출력**:
```
[09:30:15] [INF] [Preview] 파일 감지: 20260205-99-1.bmp.tsc
[09:30:15] [INF] 이벤트 전송 성공: 작업대기 - 20260205-99-1

[09:30:18] [INF] [PrintLog] 파일 감지: 20260205-99-1_093000.log
[09:30:18] [INF] 이벤트 전송 성공: 작업시작 - 20260205-99-1

[09:30:21] [INF] [Job] 파일 감지: 20260205-99-10002.job
[09:30:21] [INF] 이벤트 전송 성공: 작업완료 - 20260205-99-1
```

---

## 🔄 5단계: Windows Service 등록 (프로덕션용)

### 빌드 (배포용 실행 파일 생성)

```cmd
cd C:\MESCollector
dotnet publish -c Release -o C:\MESCollector\Release
```

### Service 등록

```cmd
# 관리자 권한 CMD 필요!
sc create MESCollector binPath="C:\MESCollector\Release\MESCollector.exe" start=auto
sc description MESCollector "MES 이벤트 수집 서비스"
sc start MESCollector
```

### Service 관리

```cmd
# 상태 확인
sc query MESCollector

# 시작
sc start MESCollector

# 중지
sc stop MESCollector

# 삭제 (필요 시)
sc delete MESCollector
```

### Service 로그 확인

```cmd
# 로그 위치
C:\MESCollector\Release\logs\collector-YYYYMMDD.log

# 실시간 로그 보기 (PowerShell)
Get-Content C:\MESCollector\Release\logs\collector-20260205.log -Wait
```

---

## 📊 실전 시나리오: TOPAZ RIP 연동

### 사전 작업

1. **TOPAZ RIP 작업명 규칙 설정**
   ```
   디자이너에게 안내:
   - 작업명 형식: YYYYMMDD-XX-Y
   - 예: 20260205-01-1
   - 절대 변경하지 않기!
   ```

2. **TOPAZ RIP 설정 확인**
   ```
   출력 폴더:
   - Preview: C:\TOPAZ_RIP\preview
   - PrintLog: C:\TOPAZ_RIP\printlog
   - Job: C:\TOPAZ_RIP\job
   ```

### 실제 작업 흐름

```
1. 관리자가 MES에서 주문서 작성
   └─> 카드번호: 20260205-01-1 생성

2. 디자이너가 TOPAZ RIP에서 작업
   └─> 작업명: "20260205-01-1" 입력
   └─> 파일 선택 및 RIP 시작

3. Collector 자동 감지
   └─> Preview 파일 생성 → "작업대기" 전송
   └─> PrintLog 파일 생성 → "작업시작" 전송
   └─> Job 파일 생성 → "작업완료" 전송

4. MES 웹에서 실시간 확인
   └─> 카드 상태: 대기 → 작업중 → 완료
```

---

## 🔍 문제 해결

### 문제 1: "서버 연결 실패"
```
오류: HttpRequestException: Connection refused

해결:
1. MES 서버 실행 확인
   cd C:\MESSystem
   dotnet run

2. 방화벽 확인
   - 5000 포트 열기
   - 인바운드 규칙 추가

3. 네트워크 확인
   ping 192.168.0.100
   telnet 192.168.0.100 5000

4. ServerUrl 확인
   appsettings.json의 IP 주소 재확인
```

### 문제 2: "폴더를 찾을 수 없습니다"
```
오류: DirectoryNotFoundException

해결:
1. 폴더 수동 생성
   mkdir C:\TOPAZ_RIP\preview
   mkdir C:\TOPAZ_RIP\printlog
   mkdir C:\TOPAZ_RIP\job

2. appsettings.json 경로 확인
   백슬래시 이중 처리: "C:\\TOPAZ_RIP\\preview"
```

### 문제 3: "카드번호를 찾을 수 없습니다"
```
경고: [Preview] 유효하지 않은 카드번호

원인: 파일명 형식이 YYYYMMDD-XX-Y가 아님

해결:
1. 파일명 확인
   ✅ 20260205-01-1.bmp.tsc
   ❌ test-01-1.bmp.tsc
   ❌ 2026-02-05-01-1.bmp.tsc

2. TOPAZ RIP 작업명 규칙 준수
```

### 문제 4: ".NET이 설치되지 않음"
```
오류: dotnet: command not found

해결:
1. .NET 8.0 Runtime 다운로드
   https://dotnet.microsoft.com/download/dotnet/8.0

2. 설치 확인
   dotnet --version
```

---

## 📋 체크리스트

### 설치 전
- [ ] Windows 출력기 PC 준비
- [ ] MES 서버 IP 주소 확인
- [ ] TOPAZ RIP 폴더 경로 확인
- [ ] 관리자 권한 확보

### 설치 중
- [ ] MESCollector 파일 복사 완료
- [ ] .NET 8.0 Runtime 설치 완료
- [ ] appsettings.json 수정 완료
- [ ] 폴더 생성 완료

### 테스트
- [ ] 콘솔 실행 성공
- [ ] 서버 연결 확인
- [ ] 테스트 파일로 이벤트 전송 확인
- [ ] MES 웹에서 이벤트 로그 확인

### 프로덕션 배포
- [ ] dotnet publish 빌드 완료
- [ ] Windows Service 등록 완료
- [ ] Service 자동 시작 설정 완료
- [ ] 로그 파일 확인

---

## 🎓 추가 팁

### 여러 출력기 관리

**출력기마다 다른 CollectorId 사용**:
```json
출력기 1: "CollectorId": "COLLECTOR-1F"
출력기 2: "CollectorId": "COLLECTOR-2F"
출력기 3: "CollectorId": "COLLECTOR-3F"
```

**장점**:
- MES 웹에서 어느 출력기인지 구분 가능
- 출력기별 통계 확인 가능
- 문제 발생 시 원인 파악 쉬움

### 로그 활용

**로그 레벨 조정**:
```json
"Serilog": {
  "MinimumLevel": "Debug"  // Information → Debug (상세 로그)
}
```

**로그 보관 기간**:
```json
"retainedFileCountLimit": 30  // 30일치 보관
```

### 성능 최적화

**대량 파일 처리 시**:
```json
"RetryCount": 5,           // 재시도 횟수 증가
"RetryDelaySeconds": 3     // 재시도 간격 단축
```

---

## 📞 지원

### 로그 확인 위치
```
콘솔 모드: 화면 출력
Service 모드: C:\MESCollector\logs\collector-YYYYMMDD.log
```

### 일반적인 로그 메시지

**성공**:
```
[INF] 이벤트 전송 성공: 작업시작 - 20260205-01-1
```

**경고**:
```
[WRN] [Preview] 유효하지 않은 카드번호: test.bmp.tsc
```

**오류**:
```
[ERR] 네트워크 오류 (재시도 1/3): 20260205-01-1
[ERR] 이벤트 전송 최종 실패: 20260205-01-1
```

---

## 🎉 완료!

**설치가 완료되었습니다!**

이제 출력기 PC에서:
1. ✅ Collector가 파일을 실시간 감지
2. ✅ MES 서버로 자동 전송
3. ✅ 웹에서 실시간 상태 확인

**다음 단계**:
- 실제 TOPAZ RIP 작업으로 테스트
- 현장 직원 교육
- 프로덕션 배포

---

**마지막 업데이트**: 2026-02-05  
**버전**: 1.0
