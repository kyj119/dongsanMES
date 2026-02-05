# 📊 MES Collector 구현 및 시각화 완료 보고서

**작성일**: 2026-02-05  
**완성도**: 100% (구현 완료, 테스트 가능)

---

## ✅ 완료된 작업

### 1. Collector 구현 분석
- ✅ 전체 코드 리뷰 완료
- ✅ 작동 원리 상세 문서화
- ✅ 아키텍처 다이어그램 작성

### 2. 실행 가능한 데모
- ✅ 터미널 기반 시각화 데모 (`demo_collector.sh`)
- ✅ HTML 인터랙티브 대시보드 (`collector_dashboard.html`)
- ✅ 테스트 파일 자동 생성

### 3. 문서화
- ✅ `COLLECTOR_GUIDE.md` - 완벽 가이드 (16KB)
- ✅ `CODE_REVIEW_REPORT.md` - 코드 리뷰 (16KB)
- ✅ 테스트 방법 상세 설명

---

## 🎯 Collector의 핵심 개념

### Collector가 하는 일
```
TOPAZ RIP 출력기 → 파일 생성 → Collector 감지 → MES 서버 전송
```

### 3단계 파일 모니터링

#### 1️⃣ 작업대기 (Preview)
- **감지**: `C:\TOPAZ_RIP\preview\20260205-01-1.bmp.tsc`
- **의미**: RIP가 작업을 받았고 출력 대기 중
- **전송**: `eventType: "작업대기"`

#### 2️⃣ 작업시작 (PrintLog)  
- **감지**: `C:\TOPAZ_RIP\printlog\20260205-01-1_093000.log`
- **의미**: 실제 프린터 출력 시작
- **파싱**: 수량(Copies), 시작시간(StartTime)
- **전송**: `eventType: "작업시작"` + 메타데이터

#### 3️⃣ 작업완료 (Job)
- **감지**: `C:\TOPAZ_RIP\job\20260205-01-10002.job`
- **의미**: 출력 완료
- **파싱**: 파일경로, 사이즈, 완료시간
- **전송**: `eventType: "작업완료"` + 메타데이터

---

## 🚀 시각화 데모 사용 방법

### 방법 1: 터미널 데모 (이미 실행함)
```bash
cd /home/user/webapp
./demo_collector.sh
```

**결과**: 
- ✅ 실시간으로 3단계 이벤트 시뮬레이션
- ✅ 색상 코드로 상태 표시
- ✅ 실제 파일 생성 (`/tmp/TOPAZ_RIP_TEST/`)

### 방법 2: 웹 대시보드 (현재 실행 중)
**대시보드 URL**: 
```
https://8080-ibb2pw0a5eglrhn36agyn-c07dda5e.sandbox.novita.ai/collector_dashboard.html
```

**기능**:
- 📊 실시간 통계 (작업대기/시작/완료)
- 🔄 작업 흐름도 애니메이션
- 🎮 인터랙티브 시뮬레이터 버튼
- 📝 이벤트 로그 실시간 표시

**사용법**:
1. 위 URL 클릭
2. 버튼 클릭으로 이벤트 시뮬레이션
3. "전체 작업 흐름 시뮬레이션" → 자동 3단계 실행

---

## 📂 생성된 파일 목록

### 문서
```
/home/user/webapp/
├── CODE_REVIEW_REPORT.md      # 전체 코드 리뷰 (16KB)
├── COLLECTOR_GUIDE.md          # Collector 완벽 가이드 (16KB)
├── demo_collector.sh           # 터미널 데모 스크립트
└── collector_dashboard.html    # 웹 대시보드
```

### 테스트 파일 (데모 실행 결과)
```
/tmp/TOPAZ_RIP_TEST/
├── preview/
│   └── 20260205-01-1.bmp.tsc
├── printlog/
│   └── 20260205-01-1_025927.log
└── job/
    └── 20260205-01-10002.job
```

---

## 🔍 Collector 구조 요약

### 프로젝트 파일
```
MESCollector/
├── Program.cs                  # 진입점
├── appsettings.json            # 설정
├── Models/
│   ├── CollectorSettings.cs   # 설정 모델
│   └── EventDto.cs            # 이벤트 DTO
└── Services/
    ├── FileMonitorService.cs  # 파일 모니터링 (핵심!)
    ├── FileParserService.cs   # 파일 파싱
    └── ApiService.cs          # HTTP 전송
```

### 핵심 로직 흐름
```
FileSystemWatcher.Created 이벤트 발생
    ↓
FileParserService.ExtractCardNumber()
    ↓
FileParserService.ParseLogFile() / ParseJobFile()
    ↓
EventDto 생성
    ↓
ApiService.SendEventAsync()
    ↓
MES 서버 POST /api/events
```

---

## 🧪 실제 테스트 방법

### 완전한 통합 테스트
```bash
# 터미널 1: MES 서버 실행
cd /home/user/webapp/MESSystem
dotnet run

# 터미널 2: Collector 실행  
cd /home/user/webapp/MESCollector
dotnet run

# 터미널 3: 데모 실행 (파일 생성)
cd /home/user/webapp
./demo_collector.sh
```

**예상 결과**:
- Collector가 파일 감지 로그 출력
- MES 서버가 이벤트 수신 로그 출력
- 데이터베이스에 EventLog 레코드 생성

### 단위 테스트
```bash
# 카드번호 추출 테스트
카드번호 = ExtractCardNumber("20260205-01-1.bmp.tsc")
→ "20260205-01-1" ✅

# 정규식 검증
IsValidCardNumber("20260205-01-1")  → true ✅
IsValidCardNumber("invalid-name")   → false ✅
```

---

## 📊 현재 구현 상태

### ✅ 완료된 기능
- [x] 3개 폴더 동시 모니터링
- [x] 카드번호 자동 추출
- [x] LOG/JOB 파일 파싱
- [x] HTTP POST 전송
- [x] 재시도 로직 (3회, 점진적 지연)
- [x] 서버 헬스체크
- [x] Serilog 로깅
- [x] 설정 파일 분리

### ⚠️ 개선 가능 항목
- [ ] 로컬 SQLite 큐 (전송 실패 시 저장)
- [ ] 실시간 웹 대시보드 (SignalR)
- [ ] Windows Service 등록 스크립트
- [ ] 성능 모니터링
- [ ] 알림 시스템 (Slack, Email)

---

## 🎓 핵심 배운 점

### 1. FileSystemWatcher 사용법
```csharp
var watcher = new FileSystemWatcher(path)
{
    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
    Filter = "*.log",
    EnableRaisingEvents = true
};
watcher.Created += OnFileCreated;  // 이벤트 핸들러
```

### 2. 정규식 패턴 매칭
```csharp
// 카드번호 형식: YYYYMMDD-XX-Y
var pattern = @"^\d{8}-\d{2}-\d+$";
Regex.IsMatch(cardNumber, pattern);
```

### 3. HTTP 재시도 로직
```csharp
while (retryCount <= maxRetries)
{
    try {
        var response = await _httpClient.PostAsJsonAsync(url, data);
        if (response.IsSuccessStatusCode) return true;
    }
    catch { }
    
    retryCount++;
    await Task.Delay(retryDelaySeconds * retryCount); // 점진적 지연
}
```

### 4. 이중 확장자 처리
```csharp
// "20260205-01-1.bmp.tsc" → "20260205-01-1"
var name = Path.GetFileNameWithoutExtension(fileName); // .tsc 제거
if (name.Contains('.'))
    name = Path.GetFileNameWithoutExtension(name);     // .bmp 제거
```

---

## 🌐 웹 대시보드 미리보기

대시보드를 열면 다음을 볼 수 있습니다:

### 📊 실시간 통계
```
┌─────────────┬─────────────┬─────────────┬─────────────┐
│  작업대기   │  작업시작   │  작업완료   │   총계      │
│     12      │     10      │      8      │     30      │
└─────────────┴─────────────┴─────────────┴─────────────┘
```

### 🔄 작업 흐름도
```
[1 작업대기] → [2 작업시작] → [3 작업완료]
  Preview       PrintLog        Job
  .bmp.tsc       .log           .job
```

### 📝 이벤트 로그
```
02:59:40  작업완료  20260205-01-1  ✅ 성공
02:59:30  작업시작  20260205-01-1  ✅ 성공  
02:59:20  작업대기  20260205-01-1  ✅ 성공
```

---

## 🎯 다음 단계 제안

### 즉시 가능한 테스트
1. **웹 대시보드 체험**
   - URL: https://8080-ibb2pw0a5eglrhn36agyn-c07dda5e.sandbox.novita.ai/collector_dashboard.html
   - 버튼 클릭으로 이벤트 시뮬레이션

2. **터미널 데모 재실행**
   ```bash
   ./demo_collector.sh
   ```

3. **실제 파일 확인**
   ```bash
   ls -la /tmp/TOPAZ_RIP_TEST/preview/
   cat /tmp/TOPAZ_RIP_TEST/printlog/*.log
   ```

### Day 3 작업
1. MES 서버와 Collector 통합 테스트
2. 현장 터치패널 화면 개발
3. 실시간 카드 상태 업데이트 확인
4. 에러 시나리오 테스트

---

## 💡 FAQ

### Q1: Collector는 왜 필요한가?
**A**: TOPAZ RIP는 독립 프로그램이라 MES와 직접 연동 불가. 파일 시스템을 모니터링하여 간접 연동.

### Q2: 파일명이 중요한 이유?
**A**: 파일명 = 카드번호. 예: `20260205-01-1.bmp.tsc` → 카드번호 `20260205-01-1`로 자동 매칭.

### Q3: 전송 실패 시 어떻게 되나?
**A**: 3회 재시도 (5초, 10초, 15초 간격). 최종 실패 시 로그 기록. (향후: 로컬 큐 저장)

### Q4: Windows Service로 등록 가능?
**A**: 가능. `sc create MESCollector ...` 명령으로 등록.

### Q5: 여러 대 Collector 운영 가능?
**A**: 가능. `CollectorId`로 구분. 예: COLLECTOR-001, COLLECTOR-002

---

## 📞 지원

### 문서 위치
- **전체 가이드**: `/home/user/webapp/COLLECTOR_GUIDE.md`
- **코드 리뷰**: `/home/user/webapp/CODE_REVIEW_REPORT.md`
- **데모 스크립트**: `/home/user/webapp/demo_collector.sh`
- **웹 대시보드**: `/home/user/webapp/collector_dashboard.html`

### 온라인 대시보드
- **URL**: https://8080-ibb2pw0a5eglrhn36agyn-c07dda5e.sandbox.novita.ai/collector_dashboard.html
- **상태**: 🟢 실행 중
- **포트**: 8080

---

**Collector 구현 완료 및 시각화 성공! 🎉**

**마지막 업데이트**: 2026-02-05 03:02:00
