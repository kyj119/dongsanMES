# 🔍 MES Collector 완벽 가이드

**작성일**: 2026-02-05  
**대상**: Collector 구현 및 테스트 방법

---

## 📋 목차
1. [Collector란 무엇인가?](#collector란-무엇인가)
2. [작동 원리](#작동-원리)
3. [구현 상세](#구현-상세)
4. [테스트 방법](#테스트-방법)
5. [시각화 데모](#시각화-데모)
6. [문제 해결](#문제-해결)

---

## 1. Collector란 무엇인가?

### 개념
**Collector**는 TOPAZ RIP 출력기에서 발생하는 파일 변화를 **실시간으로 모니터링**하여, 작업 상태를 MES 서버로 자동 전송하는 **백그라운드 프로그램**입니다.

### 왜 필요한가?
```
문제: TOPAZ RIP는 독립 프로그램이라 MES와 직접 연동 불가능
해결: 파일 시스템을 모니터링하여 간접 연동

TOPAZ RIP → 파일 생성 → Collector 감지 → MES 서버 전송
```

### 핵심 기능
1. **실시간 파일 모니터링** (FileSystemWatcher)
2. **카드번호 자동 추출** (파일명 파싱)
3. **이벤트 전송** (HTTP POST)
4. **재시도 로직** (네트워크 오류 대응)
5. **로깅** (문제 추적)

---

## 2. 작동 원리

### 전체 흐름도
```
┌─────────────────────────────────────────────────────────┐
│                    TOPAZ RIP 출력기                      │
│  (디자이너가 "20260204-01-1" 작업명으로 출력 시작)      │
└──────────────┬──────────────────────────────────────────┘
               │
               │ 파일 생성
               ▼
┌─────────────────────────────────────────────────────────┐
│            C:\TOPAZ_RIP\ 폴더 구조                       │
│                                                          │
│  ├─ preview\                                            │
│  │   └─ 20260204-01-1.bmp.tsc  ◄─── 1단계: 작업대기    │
│  │                                                       │
│  ├─ printlog\                                           │
│  │   └─ 20260204-01-1_093055.log  ◄─── 2단계: 작업시작 │
│  │                                                       │
│  └─ job\                                                │
│      └─ 20260204-01-10002.job  ◄─── 3단계: 작업완료    │
│                                                          │
└──────────────┬──────────────────────────────────────────┘
               │
               │ FileSystemWatcher 감지
               ▼
┌─────────────────────────────────────────────────────────┐
│                  MES Collector                           │
│                                                          │
│  1. FileMonitorService                                  │
│     └─> 3개 폴더 동시 모니터링                          │
│                                                          │
│  2. FileParserService                                   │
│     └─> 카드번호 추출: "20260204-01-1"                 │
│     └─> 메타데이터 파싱 (수량, 시간 등)                │
│                                                          │
│  3. ApiService                                          │
│     └─> HTTP POST /api/events                           │
│     └─> 재시도 로직 (3회)                               │
│                                                          │
└──────────────┬──────────────────────────────────────────┘
               │
               │ HTTP POST
               ▼
┌─────────────────────────────────────────────────────────┐
│                  MES 서버 (ASP.NET Core)                 │
│                                                          │
│  POST /api/events                                       │
│  {                                                       │
│    "eventType": "작업시작",                             │
│    "cardNumber": "20260204-01-1",                       │
│    "collectorId": "COLLECTOR-001",                      │
│    "timestamp": "2026-02-05T09:30:00Z"                  │
│  }                                                       │
│                                                          │
│  → EventLog 테이블에 저장                                │
│  → Card 상태 업데이트 (대기 → 작업중 → 완료)           │
│                                                          │
└──────────────┬──────────────────────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────────────────────┐
│              현장 터치패널 화면                          │
│                                                          │
│  카드 20260204-01-1                                     │
│  상태: [작업중] ◄─── 실시간 업데이트!                   │
│  시작: 09:30                                             │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

### 3단계 이벤트 상세

#### 1단계: 작업대기 (Preview)
```
파일: C:\TOPAZ_RIP\preview\20260204-01-1.bmp.tsc
의미: RIP가 작업을 받았고, 출력 대기 중
트리거: .bmp 또는 .tsc 파일 생성
이벤트: "작업대기"
```

#### 2단계: 작업시작 (PrintLog)
```
파일: C:\TOPAZ_RIP\printlog\20260204-01-1_093055.log
의미: 실제 출력 시작 (프린터 동작)
트리거: .log 파일 생성
이벤트: "작업시작"

파일 내용 예시:
JobName=8색, Copies=9380, StartTime=2026-02-05 09:30:55
```

#### 3단계: 작업완료 (Job)
```
파일: C:\TOPAZ_RIP\job\20260204-01-10002.job
의미: 출력 완료
트리거: .job 파일 생성
이벤트: "작업완료"

파일 내용 예시:
PrintFile=Z:\테스트\8색.eps
DestSizeX=980.000000
BeginDate=2026-02-05 09:30:55
EndDate=2026-02-05 09:36:03
```

---

## 3. 구현 상세

### 3.1 프로젝트 구조
```
MESCollector/
├── Program.cs                    # 진입점 (HostedService 설정)
├── appsettings.json              # 설정 파일
├── Models/
│   ├── CollectorSettings.cs     # 설정 모델
│   └── EventDto.cs              # 이벤트 DTO
├── Services/
│   ├── FileMonitorService.cs    # 파일 모니터링 (핵심)
│   ├── FileParserService.cs     # 파일 파싱
│   └── ApiService.cs            # HTTP 통신
└── logs/                         # 로그 파일 (자동 생성)
```

### 3.2 핵심 코드 설명

#### FileMonitorService.cs - 메인 로직
```csharp
public class FileMonitorService : BackgroundService
{
    // 1. 폴더 모니터링 설정
    private void SetupFileWatchers()
    {
        // Preview 폴더
        var previewWatcher = new FileSystemWatcher(previewPath)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            Filter = "*.*",
            EnableRaisingEvents = true
        };
        previewWatcher.Created += OnPreviewFileCreated; // 파일 생성 이벤트
    }
    
    // 2. 파일 생성 감지 시 실행
    private async void OnPreviewFileCreated(object sender, FileSystemEventArgs e)
    {
        // 파일명에서 카드번호 추출
        var cardNumber = _parser.ExtractCardNumber(e.Name);
        
        // 이벤트 생성
        var eventDto = new EventDto
        {
            EventType = "작업대기",
            CardNumber = cardNumber,
            CollectorId = _settings.CollectorId,
            Timestamp = DateTime.Now
        };
        
        // 서버로 전송
        await _apiService.SendEventAsync(eventDto);
    }
}
```

#### FileParserService.cs - 파일 파싱
```csharp
public string? ExtractCardNumber(string fileName)
{
    // 예: "20260204-01-1.bmp.tsc" → "20260204-01-1"
    
    // 1. 확장자 제거
    var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
    // "20260204-01-1.bmp"
    
    // 2. 이중 확장자 처리
    if (nameWithoutExt.Contains('.'))
    {
        nameWithoutExt = Path.GetFileNameWithoutExtension(nameWithoutExt);
    }
    // "20260204-01-1"
    
    // 3. 형식 검증: YYYYMMDD-XX-Y
    if (Regex.IsMatch(nameWithoutExt, @"^\d{8}-\d{2}-\d+$"))
    {
        return nameWithoutExt;
    }
    
    return null;
}
```

#### ApiService.cs - HTTP 전송
```csharp
public async Task<bool> SendEventAsync(EventDto eventDto)
{
    var retryCount = 0;
    var maxRetries = 3;
    
    while (retryCount <= maxRetries)
    {
        try
        {
            // HTTP POST 요청
            var url = $"{serverUrl}/api/events";
            var response = await _httpClient.PostAsJsonAsync(url, eventDto);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("전송 성공: {CardNumber}", eventDto.CardNumber);
                return true;
            }
        }
        catch (HttpRequestException)
        {
            // 네트워크 오류 → 재시도
            retryCount++;
            await Task.Delay(TimeSpan.FromSeconds(5 * retryCount));
        }
    }
    
    _logger.LogError("전송 실패: {CardNumber}", eventDto.CardNumber);
    return false;
}
```

---

## 4. 테스트 방법

### 4.1 테스트 환경 준비

#### 옵션 1: 실제 TOPAZ RIP 폴더 (Windows)
```bash
# 1. 폴더 생성
mkdir C:\TOPAZ_RIP\preview
mkdir C:\TOPAZ_RIP\printlog
mkdir C:\TOPAZ_RIP\job

# 2. Collector 실행
cd MESCollector
dotnet run
```

#### 옵션 2: 테스트 폴더 (개발용)
```bash
# 1. 테스트 폴더 생성
mkdir C:\TestCollector\preview
mkdir C:\TestCollector\printlog
mkdir C:\TestCollector\job

# 2. appsettings.json 수정
{
  "Collector": {
    "WatchPaths": {
      "Preview": "C:\\TestCollector\\preview",
      "PrintLog": "C:\\TestCollector\\printlog",
      "Job": "C:\\TestCollector\\job"
    }
  }
}

# 3. Collector 실행
dotnet run
```

### 4.2 수동 테스트 시나리오

#### 시나리오 1: 정상 작업 흐름
```bash
# 터미널 1: MES 서버 실행
cd MESSystem
dotnet run

# 터미널 2: Collector 실행
cd MESCollector
dotnet run

# 터미널 3: 테스트 파일 생성
# 1단계: 작업대기
echo "test" > C:\TestCollector\preview\20260205-01-1.bmp.tsc

# 2단계: 작업시작 (5초 대기)
timeout /t 5
echo "JobName=테스트, Copies=100" > C:\TestCollector\printlog\20260205-01-1_093000.log

# 3단계: 작업완료 (10초 대기)
timeout /t 10
echo "PrintFile=test.eps" > C:\TestCollector\job\20260205-01-10002.job
```

**예상 로그 출력**:
```
[09:30:00] [INF] === MES Collector 시작 ===
[09:30:00] [INF] Collector ID: COLLECTOR-001
[09:30:00] [INF] 서버 연결 성공!
[09:30:00] [INF] === 파일 모니터링 시작 ===
[09:30:15] [INF] [Preview] 파일 감지: 20260205-01-1.bmp.tsc
[09:30:15] [INF] 이벤트 전송 성공: 작업대기 - 20260205-01-1
[09:30:20] [INF] [PrintLog] 파일 감지: 20260205-01-1_093000.log
[09:30:20] [INF] 이벤트 전송 성공: 작업시작 - 20260205-01-1
[09:30:30] [INF] [Job] 파일 감지: 20260205-01-10002.job
[09:30:30] [INF] 이벤트 전송 성공: 작업완료 - 20260205-01-1
```

#### 시나리오 2: 잘못된 카드번호
```bash
# 잘못된 형식의 파일명
echo "test" > C:\TestCollector\preview\invalid-name.bmp.tsc

# 예상 로그:
[09:31:00] [WRN] [Preview] 유효하지 않은 카드번호: invalid-name.bmp.tsc
```

#### 시나리오 3: 서버 연결 실패
```bash
# MES 서버를 중지한 상태에서 파일 생성
echo "test" > C:\TestCollector\preview\20260205-02-1.bmp.tsc

# 예상 로그:
[09:32:00] [INF] [Preview] 파일 감지: 20260205-02-1.bmp.tsc
[09:32:00] [ERR] 네트워크 오류 (재시도 1/3): 20260205-02-1
[09:32:05] [INF] 재시도 대기 중... 5초
[09:32:10] [ERR] 네트워크 오류 (재시도 2/3): 20260205-02-1
[09:32:15] [INF] 재시도 대기 중... 10초
[09:32:25] [ERR] 네트워크 오류 (재시도 3/3): 20260205-02-1
[09:32:30] [ERR] 이벤트 전송 최종 실패: 20260205-02-1
```

### 4.3 자동화된 테스트 스크립트

#### test_collector.bat (Windows)
```batch
@echo off
echo ========================================
echo MES Collector 테스트 스크립트
echo ========================================

set TEST_DIR=C:\TestCollector
set CARD_NUM=20260205-01-1

echo.
echo [1/4] 테스트 폴더 생성...
mkdir %TEST_DIR%\preview 2>nul
mkdir %TEST_DIR%\printlog 2>nul
mkdir %TEST_DIR%\job 2>nul

echo [2/4] 카드번호: %CARD_NUM%

echo.
echo [3/4] 작업대기 이벤트 생성...
echo test > %TEST_DIR%\preview\%CARD_NUM%.bmp.tsc
timeout /t 3 /nobreak >nul

echo [4/4] 작업시작 이벤트 생성...
echo JobName=테스트,Copies=100 > %TEST_DIR%\printlog\%CARD_NUM%_093000.log
timeout /t 3 /nobreak >nul

echo [5/5] 작업완료 이벤트 생성...
echo PrintFile=test.eps > %TEST_DIR%\job\%CARD_NUM%0002.job

echo.
echo ========================================
echo 테스트 완료! Collector 로그를 확인하세요.
echo ========================================
pause
```

#### test_collector.sh (Linux/Mac - 샌드박스)
```bash
#!/bin/bash

echo "========================================"
echo "MES Collector 테스트 스크립트"
echo "========================================"

TEST_DIR="/tmp/TestCollector"
CARD_NUM="20260205-01-1"

echo ""
echo "[1/5] 테스트 폴더 생성..."
mkdir -p $TEST_DIR/preview
mkdir -p $TEST_DIR/printlog
mkdir -p $TEST_DIR/job

echo "[2/5] 카드번호: $CARD_NUM"

echo ""
echo "[3/5] 작업대기 이벤트 생성..."
echo "test" > $TEST_DIR/preview/${CARD_NUM}.bmp.tsc
sleep 3

echo "[4/5] 작업시작 이벤트 생성..."
echo "JobName=테스트,Copies=100" > $TEST_DIR/printlog/${CARD_NUM}_093000.log
sleep 3

echo "[5/5] 작업완료 이벤트 생성..."
echo "PrintFile=test.eps" > $TEST_DIR/job/${CARD_NUM}0002.job

echo ""
echo "========================================"
echo "테스트 완료! Collector 로그를 확인하세요."
echo "========================================"
```

---

## 5. 시각화 데모

### 5.1 실시간 로그 시각화

실제 Collector가 동작할 때 로그는 다음과 같이 표시됩니다:

```
┌─────────────────────────────────────────────────────────────┐
│                  MES Collector 실행 중                       │
│                                                              │
│  09:30:00 [INF] === MES Collector 시작 ===                  │
│  09:30:00 [INF] Collector ID: COLLECTOR-001                 │
│  09:30:00 [INF] 서버 URL: http://localhost:5000             │
│  09:30:00 [INF] 서버 연결 성공!                             │
│  09:30:00 [INF] === 파일 모니터링 시작 ===                  │
│  09:30:00 [INF] Preview 폴더: C:\TOPAZ_RIP\preview          │
│  09:30:00 [INF] PrintLog 폴더: C:\TOPAZ_RIP\printlog        │
│  09:30:00 [INF] Job 폴더: C:\TOPAZ_RIP\job                  │
│                                                              │
│  📁 모니터링 중... (Ctrl+C로 종료)                          │
│                                                              │
│  ┌────────────────────────────────────────────────┐        │
│  │ 09:31:15 [INF] [Preview] 파일 감지:            │        │
│  │              20260205-01-1.bmp.tsc             │        │
│  │ 09:31:15 [INF] 이벤트 전송 성공:                │        │
│  │              작업대기 - 20260205-01-1          │        │
│  └────────────────────────────────────────────────┘        │
│                                                              │
│  ┌────────────────────────────────────────────────┐        │
│  │ 09:31:20 [INF] [PrintLog] 파일 감지:           │        │
│  │              20260205-01-1_093120.log          │        │
│  │ 09:31:20 [INF] 이벤트 전송 성공:                │        │
│  │              작업시작 - 20260205-01-1          │        │
│  │              Metadata: {Copies=100}             │        │
│  └────────────────────────────────────────────────┘        │
│                                                              │
│  ┌────────────────────────────────────────────────┐        │
│  │ 09:31:30 [INF] [Job] 파일 감지:                │        │
│  │              20260205-01-10002.job             │        │
│  │ 09:31:30 [INF] 이벤트 전송 성공:                │        │
│  │              작업완료 - 20260205-01-1          │        │
│  └────────────────────────────────────────────────┘        │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### 5.2 웹 대시보드 (향후 구현)

이벤트 로그를 실시간으로 시각화하는 웹 페이지를 만들 수 있습니다:

```
┌─────────────────────────────────────────────────────────┐
│  MES 이벤트 모니터                  [실시간 새로고침]    │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  📊 오늘의 통계                                          │
│  ┌──────────┬──────────┬──────────┬──────────┐        │
│  │ 작업대기 │ 작업시작 │ 작업완료 │   총계   │        │
│  │   12     │   10     │    8     │    30    │        │
│  └──────────┴──────────┴──────────┴──────────┘        │
│                                                          │
│  📝 최근 이벤트                                          │
│  ┌────────────────────────────────────────────────┐   │
│  │ 시간     │ 이벤트   │ 카드번호       │ 상태   │   │
│  ├────────────────────────────────────────────────┤   │
│  │ 09:31:30 │ 작업완료 │ 20260205-01-1 │ ✅ 성공 │   │
│  │ 09:31:20 │ 작업시작 │ 20260205-01-1 │ ✅ 성공 │   │
│  │ 09:31:15 │ 작업대기 │ 20260205-01-1 │ ✅ 성공 │   │
│  │ 09:30:45 │ 작업완료 │ 20260204-03-2 │ ✅ 성공 │   │
│  │ 09:30:12 │ 작업시작 │ 20260204-02-1 │ ❌ 실패 │   │
│  └────────────────────────────────────────────────┘   │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

---

## 6. 문제 해결

### 6.1 일반적인 문제

#### 문제 1: "폴더를 찾을 수 없습니다"
```
오류: System.IO.DirectoryNotFoundException
원인: TOPAZ_RIP 폴더가 존재하지 않음
해결: 
1. 폴더 경로 확인
2. appsettings.json에서 경로 수정
3. 테스트 폴더 사용
```

#### 문제 2: "서버 연결 실패"
```
오류: HttpRequestException: Connection refused
원인: MES 서버가 실행되지 않음
해결:
1. MES 서버 실행 확인
2. ServerUrl 확인 (http://localhost:5000)
3. 방화벽 설정 확인
```

#### 문제 3: "카드번호를 찾을 수 없습니다"
```
경고: [Preview] 유효하지 않은 카드번호
원인: 파일명이 YYYYMMDD-XX-Y 형식이 아님
해결:
1. 파일명 형식 확인
2. TOPAZ RIP 작업명 설정 확인
3. 정규식 패턴 수정 (필요 시)
```

### 6.2 디버깅 팁

#### 로그 파일 확인
```bash
# 로그 위치
MESCollector/logs/collector-20260205.log

# 실시간 로그 보기 (Linux/Mac)
tail -f MESCollector/logs/collector-20260205.log

# Windows PowerShell
Get-Content MESCollector\logs\collector-20260205.log -Wait
```

#### 디버그 모드 실행
```bash
# appsettings.json
{
  "Serilog": {
    "MinimumLevel": "Debug"  # Information → Debug
  }
}
```

---

## 7. 프로덕션 배포

### 7.1 Windows Service 등록

```bash
# 1. 배포 빌드
cd MESCollector
dotnet publish -c Release -o C:\MESCollector

# 2. Windows Service 등록
sc create MESCollector binPath="C:\MESCollector\MESCollector.exe"
sc description MESCollector "MES 이벤트 수집 서비스"

# 3. 서비스 시작
sc start MESCollector

# 4. 자동 시작 설정
sc config MESCollector start=auto
```

### 7.2 모니터링

```bash
# 서비스 상태 확인
sc query MESCollector

# 로그 확인
type C:\MESCollector\logs\collector-20260205.log

# 이벤트 뷰어
eventvwr.msc → Windows 로그 → 응용 프로그램
```

---

## 8. 다음 단계

### 개선 사항
- [ ] 로컬 SQLite 큐 (전송 실패 시 저장)
- [ ] 웹 대시보드 (실시간 모니터링)
- [ ] 알림 시스템 (Slack, Email)
- [ ] 성능 최적화 (대량 파일 처리)
- [ ] 헬스체크 엔드포인트

---

**Collector 구현 완료! 이제 실제 테스트를 진행해보세요.** 🚀
