# 🖨️ TOPAZ RIP 실제 파일 형식 가이드

**작성일**: 2026-02-05  
**대상**: 현장 출력기 PC에서 생성되는 파일 분석

---

## 📍 현장 환경 구성

### 실제 배치도
```
┌─────────────────────────────────────────────────────────────┐
│                   사내 네트워크                              │
│                                                              │
│  ┌──────────────┐         ┌──────────────┐                 │
│  │  MES 서버    │ ◄─────► │ 출력기 PC    │                 │
│  │ (192.168.x.x)│  HTTP   │ (Windows)    │                 │
│  │              │         │              │                 │
│  │  - ASP.NET   │         │  - Collector │                 │
│  │  - SQLite    │         │  - TOPAZ RIP │                 │
│  │  - Web UI    │         │  - Printer   │                 │
│  └──────────────┘         └──────────────┘                 │
│                                   │                         │
│                                   │ USB/Network             │
│                                   ▼                         │
│                           ┌──────────────┐                 │
│                           │ 대형 프린터   │                 │
│                           │ (MIMAKI 등)  │                 │
│                           └──────────────┘                 │
└─────────────────────────────────────────────────────────────┘
```

### 출력기 PC 폴더 구조
```
C:\TOPAZ_RIP\
├── preview\      ← Collector가 감지하는 폴더 1
├── printlog\     ← Collector가 감지하는 폴더 2
└── job\          ← Collector가 감지하는 폴더 3
```

---

## 📁 1. Preview 폴더 (.bmp.tsc 파일)

### 위치
```
C:\TOPAZ_RIP\preview\
```

### 생성 시점
**RIP가 작업을 받고 프리뷰 이미지를 생성할 때**

### 파일 예시
```
20260205-01-1.bmp.tsc
20260205-01-2.bmp.tsc
20260205-02-1.bmp.tsc
```

### 파일 형식
```
파일명: {카드번호}.bmp.tsc
확장자: .bmp.tsc (이중 확장자)
크기: 매우 작음 (메타데이터만)
```

### 실제 파일 내용
```
(바이너리 파일이거나 매우 간단한 메타데이터)
→ Collector는 파일명만 읽습니다! 내용은 읽지 않음!
```

### Collector 처리 방식
```csharp
// 파일 생성 감지
파일명: "20260205-01-1.bmp.tsc"

// 카드번호 추출
ExtractCardNumber("20260205-01-1.bmp.tsc")
→ 결과: "20260205-01-1"

// 이벤트 생성
{
  "eventType": "작업대기",
  "cardNumber": "20260205-01-1",
  "collectorId": "COLLECTOR-001",
  "timestamp": "2026-02-05T09:30:00Z"
}
```

**⚠️ 중요**: Preview 파일은 **내용을 읽지 않고 파일명만 파싱**합니다!

---

## 📁 2. PrintLog 폴더 (.log 파일)

### 위치
```
C:\TOPAZ_RIP\printlog\
```

### 생성 시점
**실제 프린터가 출력을 시작할 때**

### 파일 예시
```
20260205-01-1_093055.log
20260205-01-1_143022.log
```

### 파일명 형식
```
{카드번호}_{시간}.log

예: 20260205-01-1_093055.log
    └─ 카드번호: 20260205-01-1
    └─ 시간: 09:30:55
```

### 실제 파일 내용 (텍스트 형식)
```
JobName=태극기_90x135
Copies=100
StartTime=2026-02-05 09:30:55
EndTime=2026-02-05 09:36:03
PrinterName=MIMAKI-JV300
Width=900
Height=1350
```

### Collector가 읽는 부분
```csharp
// 1. 파일명에서 카드번호 추출
ExtractCardNumber("20260205-01-1_093055.log")
→ "20260205-01-1"

// 2. 파일 내용 파싱
var content = File.ReadAllText("C:\\TOPAZ_RIP\\printlog\\20260205-01-1_093055.log");

// 정규식으로 필요한 정보 추출
JobName = Regex.Match(content, @"JobName=([^,\r\n]+)").Groups[1].Value;
→ "태극기_90x135"

Copies = Regex.Match(content, @"Copies=(\d+)").Groups[1].Value;
→ "100"

StartTime = Regex.Match(content, @"StartTime=([^,\r\n]+)").Groups[1].Value;
→ "2026-02-05 09:30:55"
```

### 전송 데이터
```json
{
  "eventType": "작업시작",
  "cardNumber": "20260205-01-1",
  "collectorId": "COLLECTOR-001",
  "timestamp": "2026-02-05T09:30:55Z",
  "metadata": {
    "JobName": "태극기_90x135",
    "Copies": "100",
    "StartTime": "2026-02-05 09:30:55"
  }
}
```

---

## 📁 3. Job 폴더 (.job 파일)

### 위치
```
C:\TOPAZ_RIP\job\
```

### 생성 시점
**프린터 출력이 완전히 완료될 때**

### 파일 예시
```
20260205-01-10002.job
20260205-01-20002.job
```

### 파일명 형식
```
{카드번호}0002.job

예: 20260205-01-10002.job
    └─ 카드번호: 20260205-01-1
    └─ 0002: 작업 시퀀스 번호 (TOPAZ RIP가 자동 추가)
```

### 실제 파일 내용 (텍스트 형식)
```
PrintFile=Z:\Designs\2026\02\20260205-01\20260205-01-1_태극기.ai
DestSizeX=900.000000
DestSizeY=1350.000000
BeginDate=2026-02-05 09:30:55
EndDate=2026-02-05 09:36:03
TotalPages=100
PrinterName=MIMAKI-JV300
Resolution=1440dpi
InkUsed=150ml
```

### Collector가 읽는 부분
```csharp
// 1. 파일명에서 카드번호 추출
ExtractCardNumber("20260205-01-10002.job")
→ "20260205-01-1" (0002 제거)

// 2. 파일 내용 파싱
var content = File.ReadAllText("C:\\TOPAZ_RIP\\job\\20260205-01-10002.job");

// 정규식으로 필요한 정보 추출
PrintFile = Regex.Match(content, @"PrintFile=([^\r\n]+)").Groups[1].Value;
→ "Z:\Designs\2026\02\20260205-01\20260205-01-1_태극기.ai"

DestSizeX = Regex.Match(content, @"DestSizeX=([0-9.]+)").Groups[1].Value;
→ "900.000000"

EndDate = Regex.Match(content, @"EndDate=([^\r\n]+)").Groups[1].Value;
→ "2026-02-05 09:36:03"
```

### 전송 데이터
```json
{
  "eventType": "작업완료",
  "cardNumber": "20260205-01-1",
  "collectorId": "COLLECTOR-001",
  "timestamp": "2026-02-05T09:36:03Z",
  "metadata": {
    "PrintFile": "20260205-01-1_태극기.ai",
    "DestSizeX": "900.000000",
    "DestSizeY": "1350.000000",
    "EndDate": "2026-02-05 09:36:03"
  }
}
```

---

## 🔍 실제 Collector 코드 분석

### FileParserService.cs - LOG 파일 파싱
```csharp
public Dictionary<string, string>? ParseLogFile(string filePath)
{
    try
    {
        // 1. 파일 전체 읽기
        var content = File.ReadAllText(filePath);
        var metadata = new Dictionary<string, string>();

        // 2. JobName 추출
        // 예: "JobName=태극기_90x135" → "태극기_90x135"
        var jobNameMatch = Regex.Match(content, @"JobName=([^,\r\n]+)");
        if (jobNameMatch.Success)
        {
            metadata["JobName"] = jobNameMatch.Groups[1].Value.Trim();
        }

        // 3. Copies 추출
        // 예: "Copies=100" → "100"
        var copiesMatch = Regex.Match(content, @"Copies=(\d+)");
        if (copiesMatch.Success)
        {
            metadata["Copies"] = copiesMatch.Groups[1].Value;
        }

        // 4. StartTime 추출
        // 예: "StartTime=2026-02-05 09:30:55" → "2026-02-05 09:30:55"
        var startTimeMatch = Regex.Match(content, @"StartTime=([^,\r\n]+)");
        if (startTimeMatch.Success)
        {
            metadata["StartTime"] = startTimeMatch.Groups[1].Value.Trim();
        }

        return metadata.Count > 0 ? metadata : null;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"LOG 파일 파싱 실패: {filePath}, 오류: {ex.Message}");
        return null;
    }
}
```

### FileParserService.cs - 카드번호 추출
```csharp
public string? ExtractCardNumber(string fileName)
{
    try
    {
        // 1. 확장자 제거
        // "20260205-01-1.bmp.tsc" → "20260205-01-1.bmp"
        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        
        // 2. 이중 확장자 처리
        // "20260205-01-1.bmp" → "20260205-01-1"
        if (nameWithoutExt.Contains('.'))
        {
            nameWithoutExt = Path.GetFileNameWithoutExtension(nameWithoutExt);
        }

        // 3. 카드번호 형식 검증: YYYYMMDD-XX-Y
        // 정규식: ^\d{8}-\d{2}-\d+$
        //   ^\d{8}   : 8자리 숫자로 시작 (YYYYMMDD)
        //   -        : 하이픈
        //   \d{2}    : 2자리 숫자 (XX)
        //   -        : 하이픈
        //   \d+      : 1자리 이상 숫자 (Y)
        //   $        : 문자열 끝
        if (IsValidCardNumber(nameWithoutExt))
        {
            return nameWithoutExt;
        }

        return null;
    }
    catch
    {
        return null;
    }
}
```

---

## 🎯 실전 시나리오

### 시나리오: 태극기 100매 출력

#### 1단계: 디자이너가 RIP 작업 시작
```
디자이너 작업:
1. TOPAZ RIP 프로그램 실행
2. 작업명 입력: "20260205-01-1"
3. 파일 선택: "태극기_90x135.ai"
4. 출력 수량: 100
5. RIP 시작 버튼 클릭
```

**결과**: `C:\TOPAZ_RIP\preview\20260205-01-1.bmp.tsc` 생성

```
Collector 동작:
├─ FileSystemWatcher 감지
├─ 카드번호 추출: "20260205-01-1"
└─ POST /api/events {"eventType": "작업대기", ...}
```

#### 2단계: 프린터 출력 시작
```
TOPAZ RIP 자동 동작:
1. 프린터로 데이터 전송 시작
2. 로그 파일 생성
```

**결과**: `C:\TOPAZ_RIP\printlog\20260205-01-1_093055.log` 생성

```
파일 내용:
JobName=태극기_90x135
Copies=100
StartTime=2026-02-05 09:30:55
PrinterName=MIMAKI-JV300
```

```
Collector 동작:
├─ FileSystemWatcher 감지
├─ 파일 읽기 및 파싱
├─ 메타데이터 추출 (JobName, Copies, StartTime)
└─ POST /api/events {"eventType": "작업시작", "metadata": {...}}
```

#### 3단계: 출력 완료
```
TOPAZ RIP 자동 동작:
1. 프린터 출력 100% 완료
2. Job 파일 생성
```

**결과**: `C:\TOPAZ_RIP\job\20260205-01-10002.job` 생성

```
파일 내용:
PrintFile=Z:\Designs\2026\02\20260205-01\20260205-01-1_태극기.ai
DestSizeX=900.000000
DestSizeY=1350.000000
BeginDate=2026-02-05 09:30:55
EndDate=2026-02-05 09:36:03
TotalPages=100
```

```
Collector 동작:
├─ FileSystemWatcher 감지
├─ 파일 읽기 및 파싱
├─ 메타데이터 추출 (PrintFile, Size, EndDate)
└─ POST /api/events {"eventType": "작업완료", "metadata": {...}}
```

---

## ⚙️ Collector 설정 (appsettings.json)

### 출력기 PC별 설정
```json
{
  "Collector": {
    "ServerUrl": "http://192.168.0.100:5000",  // MES 서버 IP
    "CollectorId": "COLLECTOR-MACHINE-01",      // 출력기 식별자
    "WatchPaths": {
      "Preview": "C:\\TOPAZ_RIP\\preview",     // 실제 경로
      "PrintLog": "C:\\TOPAZ_RIP\\printlog",
      "Job": "C:\\TOPAZ_RIP\\job"
    },
    "RetryCount": 3,
    "RetryDelaySeconds": 5
  }
}
```

### 여러 대 출력기 운영
```
출력기 1 (1층):
- CollectorId: "COLLECTOR-1F"
- 폴더: C:\TOPAZ_RIP\...

출력기 2 (2층):
- CollectorId: "COLLECTOR-2F"
- 폴더: C:\TOPAZ_RIP\...

출력기 3 (3층):
- CollectorId: "COLLECTOR-3F"
- 폴더: C:\TOPAZ_RIP\...
```

---

## 🔧 정규식 패턴 상세

### 1. 카드번호 추출
```csharp
Pattern: ^\d{8}-\d{2}-\d+$

예시 매칭:
✅ 20260205-01-1
✅ 20260205-99-5
✅ 20241231-15-100

예시 실패:
❌ 2026-02-05-01-1  (날짜 형식 다름)
❌ 20260205-1-1     (중간 부분 1자리)
❌ test-01-1        (숫자 아님)
```

### 2. JobName 추출
```csharp
Pattern: JobName=([^,\r\n]+)

예시:
Input:  "JobName=태극기_90x135,Copies=100"
Output: "태극기_90x135"

설명:
- JobName=      : 리터럴 문자열
- ([^,\r\n]+)   : 캡처 그룹 (쉼표, 줄바꿈 제외한 모든 문자)
```

### 3. Copies 추출
```csharp
Pattern: Copies=(\d+)

예시:
Input:  "Copies=100"
Output: "100"

설명:
- Copies=  : 리터럴 문자열
- (\d+)    : 캡처 그룹 (1개 이상의 숫자)
```

---

## 🧪 테스트 방법

### 수동 파일 생성 테스트 (Windows)
```batch
@echo off
REM Preview 파일 생성
echo test > C:\TOPAZ_RIP\preview\20260205-01-1.bmp.tsc

REM 3초 대기
timeout /t 3

REM PrintLog 파일 생성
echo JobName=테스트작업 > C:\TOPAZ_RIP\printlog\20260205-01-1_093000.log
echo Copies=100 >> C:\TOPAZ_RIP\printlog\20260205-01-1_093000.log
echo StartTime=2026-02-05 09:30:00 >> C:\TOPAZ_RIP\printlog\20260205-01-1_093000.log

REM 5초 대기
timeout /t 5

REM Job 파일 생성
echo PrintFile=test.ai > C:\TOPAZ_RIP\job\20260205-01-10002.job
echo DestSizeX=900.000000 >> C:\TOPAZ_RIP\job\20260205-01-10002.job
echo EndDate=2026-02-05 09:35:00 >> C:\TOPAZ_RIP\job\20260205-01-10002.job
```

---

## 📊 요약

### Collector가 감지하는 것
| 폴더 | 파일 | 읽는 내용 | 추출 정보 |
|------|------|----------|-----------|
| **preview/** | .bmp.tsc | ❌ 파일명만 | 카드번호 |
| **printlog/** | .log | ✅ 전체 파일 | 카드번호, JobName, Copies, StartTime |
| **job/** | .job | ✅ 전체 파일 | 카드번호, PrintFile, Size, EndDate |

### 핵심 포인트
1. ✅ **파일명 = 카드번호** (가장 중요!)
2. ✅ **LOG/JOB 파일은 텍스트 형식** (키=값 구조)
3. ✅ **정규식으로 필요한 값만 추출**
4. ✅ **파일 생성 즉시 감지 및 전송**
5. ✅ **각 출력기 PC마다 Collector 1개 설치**

---

**이제 실제 현장에서 어떻게 작동하는지 완벽히 이해하셨습니다!** 🎉
