# 🎯 출력기 PC Collector 설치 빠른 가이드

**작성일**: 2026-02-05  
**대상**: 출력기 PC에 Collector 설치하려는 담당자

---

## ⚡ 5분 빠른 설치 (요약)

### 1️⃣ 준비물
```
✅ Windows 출력기 PC
✅ MESCollector 폴더 (GitHub 또는 샌드박스에서)
✅ MES 서버 IP 주소
✅ 관리자 권한
```

### 2️⃣ 설치 순서
```
1. install_collector.bat 실행 (관리자 권한)
2. appsettings.json에서 ServerUrl 수정
3. MESCollector 소스 파일 복사
4. dotnet run으로 테스트
5. Windows Service 등록 (선택)
```

### 3️⃣ 테스트 순서
```
1. MES 서버 실행
2. Collector 실행 (dotnet run)
3. test_collector.bat 실행
4. 이벤트 전송 확인
```

---

## 📂 필요한 파일 목록

### GitHub에서 다운로드할 파일
```
webapp/
├── MESCollector/              ← 이 폴더 전체
│   ├── Program.cs
│   ├── MESCollector.csproj
│   ├── appsettings.json
│   ├── Models/
│   └── Services/
│
├── install_collector.bat      ← 설치 스크립트
├── COLLECTOR_INSTALL_GUIDE.md ← 상세 가이드
└── TOPAZ_RIP_FILE_FORMAT.md  ← 파일 형식 가이드
```

---

## 💾 다운로드 방법

### 방법 1: GitHub에서 다운로드
```
1. GitHub 저장소 접속
2. "Code" → "Download ZIP"
3. 압축 해제 후 필요한 파일만 복사
```

### 방법 2: 개별 파일 복사
```
출력기 PC에서 필요한 파일:
C:\MESCollector\
├── install_collector.bat
├── MESCollector.csproj
├── Program.cs
├── appsettings.json
├── Models\
│   ├── CollectorSettings.cs
│   └── EventDto.cs
└── Services\
    ├── FileMonitorService.cs
    ├── FileParserService.cs
    └── ApiService.cs
```

---

## 🚀 설치 실행

### Step 1: 설치 스크립트 실행
```
1. install_collector.bat 우클릭
2. "관리자 권한으로 실행" 클릭
3. .NET이 없으면 설치 링크 표시됨
4. 설치 완료 메시지 확인
```

### Step 2: 설정 파일 수정
```
C:\MESCollector\appsettings.json 열기

수정할 내용:
- ServerUrl: "http://192.168.0.XXX:5000"  ← 실제 MES 서버 IP
- CollectorId: "COLLECTOR-출력기이름"     ← 구분 가능한 이름

예시:
{
  "Collector": {
    "ServerUrl": "http://192.168.0.100:5000",
    "CollectorId": "COLLECTOR-1F",
    ...
  }
}
```

### Step 3: 테스트 실행
```cmd
# CMD 열기
cd C:\MESCollector
dotnet run
```

**성공 메시지**:
```
[INF] === MES Collector 시작 ===
[INF] Collector ID: COLLECTOR-1F
[INF] 서버 연결 성공!
[INF] === 파일 모니터링 시작 ===
```

### Step 4: 이벤트 테스트
```cmd
# 새 CMD 창 열기
cd C:\MESCollector
test_collector.bat
```

**Collector 콘솔에서 확인**:
```
[INF] [Preview] 파일 감지: 20260205-99-1.bmp.tsc
[INF] 이벤트 전송 성공: 작업대기 - 20260205-99-1
[INF] [PrintLog] 파일 감지: 20260205-99-1_093000.log
[INF] 이벤트 전송 성공: 작업시작 - 20260205-99-1
[INF] [Job] 파일 감지: 20260205-99-10002.job
[INF] 이벤트 전송 성공: 작업완료 - 20260205-99-1
```

---

## ✅ 설치 완료 체크리스트

### 설치 전
- [ ] Windows 출력기 PC 준비
- [ ] MES 서버 IP 확인: `_______________`
- [ ] TOPAZ RIP 폴더 경로 확인: `_______________`

### 설치 중
- [ ] install_collector.bat 실행 완료
- [ ] .NET 8.0 Runtime 설치 확인
- [ ] C:\TOPAZ_RIP\ 폴더 생성 확인
- [ ] appsettings.json 수정 완료

### 테스트
- [ ] dotnet run 실행 성공
- [ ] "서버 연결 성공!" 메시지 확인
- [ ] test_collector.bat 실행
- [ ] 3개 이벤트 전송 성공 확인
- [ ] MES 웹에서 이벤트 로그 확인

### 프로덕션 (선택)
- [ ] Windows Service 등록
- [ ] 자동 시작 설정
- [ ] 재부팅 후 자동 실행 확인

---

## 🔧 실제 TOPAZ RIP 연동

### 사전 작업
```
1. 디자이너에게 작업명 규칙 교육:
   - 형식: YYYYMMDD-XX-Y
   - 예: 20260205-01-1
   - 절대 다른 형식 사용 금지!

2. TOPAZ RIP 출력 폴더 확인:
   - Preview: C:\TOPAZ_RIP\preview
   - PrintLog: C:\TOPAZ_RIP\printlog
   - Job: C:\TOPAZ_RIP\job
```

### 실제 작업 흐름
```
1. MES에서 주문서 작성 → 카드번호: 20260205-01-1 생성
2. 디자이너가 TOPAZ RIP에서 작업명 "20260205-01-1" 입력
3. Collector가 자동으로 파일 감지 및 전송
4. MES 웹에서 실시간 상태 확인
```

---

## 🆘 문제 해결

### "서버 연결 실패"
```
해결:
1. MES 서버 실행 확인
2. ping 192.168.0.100 테스트
3. 방화벽 5000 포트 열기
4. appsettings.json의 ServerUrl 재확인
```

### ".NET을 찾을 수 없음"
```
해결:
1. https://dotnet.microsoft.com/download/dotnet/8.0 접속
2. "ASP.NET Core Runtime 8.0.x - Windows x64" 다운로드
3. 설치 후 PC 재부팅
4. dotnet --version 확인
```

### "폴더를 찾을 수 없음"
```
해결:
1. C:\TOPAZ_RIP\ 폴더 수동 생성
2. preview, printlog, job 하위 폴더 생성
3. appsettings.json 경로 재확인 (백슬래시 이중 처리)
```

---

## 📞 지원

### 상세 문서
- **COLLECTOR_INSTALL_GUIDE.md** - 완전한 설치 가이드
- **TOPAZ_RIP_FILE_FORMAT.md** - 파일 형식 설명
- **COLLECTOR_GUIDE.md** - Collector 작동 원리

### 로그 확인
```
위치: C:\MESCollector\logs\collector-YYYYMMDD.log

실시간 보기 (PowerShell):
Get-Content C:\MESCollector\logs\collector-20260205.log -Wait
```

---

## 📊 요약

### 설치 단계
```
1. install_collector.bat 실행      (2분)
2. appsettings.json 수정           (1분)
3. MESCollector 파일 복사          (1분)
4. dotnet run 테스트              (2분)
5. test_collector.bat 테스트      (1분)
────────────────────────────────────────
총 소요 시간: 약 7분
```

### 핵심 설정
```json
{
  "ServerUrl": "http://실제MES서버IP:5000",
  "CollectorId": "COLLECTOR-출력기이름",
  "WatchPaths": {
    "Preview": "C:\\TOPAZ_RIP\\preview",
    "PrintLog": "C:\\TOPAZ_RIP\\printlog",
    "Job": "C:\\TOPAZ_RIP\\job"
  }
}
```

### 테스트 명령어
```cmd
# 실행
cd C:\MESCollector
dotnet run

# 테스트
test_collector.bat

# 서비스 등록
sc create MESCollector binPath="C:\MESCollector\MESCollector.exe"
sc start MESCollector
```

---

**설치 완료 후 실제 TOPAZ RIP 작업으로 테스트하세요!** 🚀

**마지막 업데이트**: 2026-02-05
