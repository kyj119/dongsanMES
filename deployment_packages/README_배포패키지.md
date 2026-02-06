# 🚀 MES 시스템 배포 패키지 가이드

**작성일**: 2026-02-06  
**프로젝트**: dongsanMES

---

## 📦 배포 패키지 구성

### 1️⃣ MES 서버 배포 패키지 (MESSystem)
**대상**: 사무실 또는 서버실의 MES 서버 PC  
**용도**: 웹 기반 MES 시스템 (주문서 관리, 카드 관리, 현장 대시보드)

**필요한 파일**:
```
📁 1_MESServer_Package/
├── 📁 MESSystem/              ← 전체 폴더 복사
│   ├── Program.cs
│   ├── MESSystem.csproj
│   ├── appsettings.json
│   ├── Data/
│   ├── Models/
│   ├── Pages/
│   ├── Controllers/
│   ├── Services/
│   ├── wwwroot/
│   └── ...
├── 📄 DEPLOYMENT_GUIDE.md     ← 서버 배포 가이드
├── 📄 SERVER_TRANSFER_GUIDE.md ← 서버 이전 가이드
└── 📄 SECURITY_CHECKLIST.md   ← 보안 체크리스트
```

**설치 방법**:
1. Windows Server에 .NET 8.0 설치
2. MESSystem 폴더 복사 → `C:\MESSystem\`
3. `appsettings.json` 수정 (DB 연결 등)
4. IIS 설정 또는 `dotnet run`으로 실행
5. 방화벽 포트 5000 오픈

---

### 2️⃣ Collector 배포 패키지 (출력기 PC용)
**대상**: TOPAZ RIP가 설치된 출력기 PC  
**용도**: 출력 작업 파일 자동 감지 → MES 서버로 전송

**필요한 파일**:
```
📁 2_Collector_Package/
├── 📁 MESCollector/           ← 전체 폴더 복사
│   ├── Program.cs
│   ├── MESCollector.csproj
│   ├── appsettings.json       ← ⚠️ 반드시 수정 필요!
│   ├── Models/
│   │   ├── CollectorSettings.cs
│   │   └── EventDto.cs
│   └── Services/
│       ├── ApiService.cs
│       ├── FileMonitorService.cs
│       └── FileParserService.cs
│
├── 📄 install_collector.bat   ← ⭐ 자동 설치 스크립트
├── 📄 QUICK_INSTALL_COLLECTOR.md  ← ⭐ 빠른 설치 가이드 (필독!)
├── 📄 COLLECTOR_INSTALL_GUIDE.md  ← 상세 설치 가이드
├── 📄 TOPAZ_RIP_FILE_FORMAT.md    ← 파일 형식 설명
└── 📄 TROUBLESHOOTING.md          ← 문제 해결 가이드
```

**설치 방법** (5분):
1. `install_collector.bat` 우클릭 → 관리자 권한으로 실행
2. `appsettings.json` 수정:
   - `ServerUrl`: MES 서버 IP 입력 (예: `http://192.168.0.100:5000`)
   - `CollectorId`: 출력기 이름 입력 (예: `COLLECTOR-1F`)
   - `WatchDirectories`: TOPAZ RIP 경로 확인 (예: `C:\TOPAZ_RIP\preview`)
3. `dotnet run` 테스트
4. Windows Service로 등록 (선택)

**중요**: 반드시 `QUICK_INSTALL_COLLECTOR.md` 읽어보세요!

---

### 3️⃣ 개발 문서 패키지 (참고용)
**대상**: 개발자, 시스템 관리자  
**용도**: 시스템 이해, 문제 해결, 추가 개발

**필요한 파일**:
```
📁 3_Documentation_Package/
├── 📄 README.md                   ← ⭐ 프로젝트 전체 개요
├── 📄 QUICK_START.md              ← 빠른 시작 가이드
├── 📄 COLLECTOR_SUMMARY.md        ← Collector 요약
├── 📄 COLLECTOR_GUIDE.md          ← Collector 상세 가이드
├── 📄 CODE_REVIEW_REPORT.md       ← 코드 리뷰 보고서
├── 📄 SERVER_DEBUG_GUIDE.md       ← 디버깅 가이드
└── 📄 REVIEW_SUMMARY.md           ← 리뷰 요약
```

---

## 🎯 배포 시나리오별 가이드

### 시나리오 1: 완전 새로운 설치
```
1단계: MES 서버 설치
   → 1_MESServer_Package 전체 복사
   → DEPLOYMENT_GUIDE.md 참고

2단계: Collector 설치 (출력기마다)
   → 2_Collector_Package 전체 복사
   → QUICK_INSTALL_COLLECTOR.md 참고

3단계: 연동 테스트
   → MES 서버 실행 → Collector 실행 → 테스트 파일 생성
```

### 시나리오 2: Collector만 추가 설치
```
→ 2_Collector_Package만 출력기 PC에 복사
→ install_collector.bat 실행
→ appsettings.json에서 MES 서버 IP만 수정
```

### 시나리오 3: 서버 이전
```
→ SERVER_TRANSFER_GUIDE.md 참고
→ 데이터베이스 백업
→ MESSystem 폴더 복사
→ Collector의 ServerUrl 업데이트
```

---

## ⚙️ 필수 설정 파일 수정 항목

### MESSystem/appsettings.json (서버)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=mes_production.db"  // SQLite 경로
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5000"  // 외부 접속 허용
      }
    }
  }
}
```

### MESCollector/appsettings.json (출력기)
```json
{
  "Collector": {
    "ServerUrl": "http://192.168.0.100:5000",  // ⚠️ MES 서버 IP
    "CollectorId": "COLLECTOR-1F",             // ⚠️ 출력기 이름
    "WatchDirectories": {
      "Preview": "C:\\TOPAZ_RIP\\preview",     // ⚠️ TOPAZ RIP 경로 확인
      "PrintLog": "C:\\TOPAZ_RIP\\printlog",
      "Job": "C:\\TOPAZ_RIP\\job"
    }
  }
}
```

---

## 📋 배포 체크리스트

### MES 서버 (1대)
- [ ] Windows Server 또는 Windows 10/11 준비
- [ ] .NET 8.0 SDK 설치
- [ ] IIS 설치 (선택)
- [ ] 방화벽 포트 5000 오픈
- [ ] 1_MESServer_Package 복사
- [ ] dotnet run 또는 IIS 배포
- [ ] 웹 접속 테스트: `http://서버IP:5000`

### Collector (출력기마다)
- [ ] 출력기 PC에 .NET 8.0 Runtime 설치
- [ ] TOPAZ RIP 경로 확인: `C:\TOPAZ_RIP\`
- [ ] 2_Collector_Package 복사
- [ ] install_collector.bat 실행
- [ ] appsettings.json 수정 (ServerUrl, CollectorId)
- [ ] dotnet run 테스트
- [ ] MES 서버 연결 확인
- [ ] Windows Service 등록 (선택)

---

## 🆘 문제 발생 시

### Collector 연결 실패
→ `TROUBLESHOOTING.md` 참고  
→ 방화벽 확인  
→ ServerUrl 확인

### 파일 감지 안됨
→ `TOPAZ_RIP_FILE_FORMAT.md` 참고  
→ WatchDirectories 경로 확인  
→ 폴더 권한 확인

### 서버 에러
→ `SERVER_DEBUG_GUIDE.md` 참고  
→ 로그 확인: `MESSystem/Logs/`

---

## 📞 추가 지원

**GitHub**: https://github.com/kyj119/dongsanMES  
**Issues**: https://github.com/kyj119/dongsanMES/issues

---

## 🔄 업데이트 방법

### Git 사용 가능한 경우
```bash
cd C:\MESSystem
git pull origin main
dotnet build
dotnet run
```

### Git 없는 경우
1. GitHub에서 최신 버전 다운로드
2. 기존 폴더 백업
3. 새 파일로 교체
4. appsettings.json 복원

---

**마지막 업데이트**: 2026-02-06  
**버전**: Day 4 완료 (EPS 썸네일, 우선순위 시스템)
