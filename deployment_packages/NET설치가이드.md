# 🔧 .NET 8.0 설치 가이드 (현장용)

**작성일**: 2026-02-06  
**중요도**: ⭐⭐⭐ 필수!

---

## 📋 어디에 무엇을 설치해야 하나요?

| PC 종류 | 필요한 .NET | 용도 | 다운로드 |
|---------|------------|------|---------|
| **MES 서버** | .NET 8.0 SDK | 개발+실행 | SDK 설치 |
| **출력기 PC** | .NET 8.0 Runtime | 실행만 | Runtime 설치 |

---

## 1️⃣ MES 서버 PC - .NET 8.0 SDK 설치

### 설치 이유
- MES 웹 시스템 실행
- 빌드/컴파일 필요
- 개발 및 테스트

### 다운로드 링크
```
https://dotnet.microsoft.com/download/dotnet/8.0
```

### 설치할 파일 (선택)
**추천**: **ASP.NET Core Runtime 8.0.x - Windows x64 Hosting Bundle**
- 용량: 약 60MB
- 포함: Runtime + ASP.NET Core + IIS 지원
- ✅ 서버 배포용으로 최적

**또는**: **.NET 8.0 SDK - Windows x64**
- 용량: 약 200MB
- 포함: SDK + Runtime + 개발 도구
- ✅ 개발도 할 경우

### 설치 순서
```
1. 다운로드한 설치 파일 실행
2. "Install" 클릭
3. 설치 완료 후 PC 재부팅 (권장)
4. 확인: 
   PowerShell/CMD 열기
   dotnet --version
   → 8.0.x 출력되면 성공!
```

---

## 2️⃣ 출력기 PC - .NET 8.0 Runtime 설치 ⭐ 중요!

### 설치 이유
- Collector 프로그램 실행에 필요
- 출력 작업 모니터링
- SDK는 필요 없음 (실행만 하므로)

### 다운로드 링크
```
https://dotnet.microsoft.com/download/dotnet/8.0
```

### 설치할 파일 (명확히!)
**필수**: **.NET Runtime 8.0.x - Windows x64**
- 용량: 약 25MB
- **또는** ASP.NET Core Runtime 8.0.x - Windows x64
- ✅ 출력기 PC는 이것만 있으면 됨!

### 설치 순서
```
1. 출력기 PC에서 위 링크 접속
2. ".NET Runtime 8.0.x" 섹션 찾기
3. "Download x64" 클릭 (Windows 64비트용)
4. 다운로드한 파일 실행
5. "Install" 클릭
6. 설치 완료 후 PC 재부팅 (권장)
7. 확인:
   CMD 열기
   dotnet --version
   → 8.0.x 출력되면 성공!
```

---

## 🖼️ 다운로드 페이지 안내 (스크린샷 참고용)

### 페이지 구조
```
https://dotnet.microsoft.com/download/dotnet/8.0

┌─────────────────────────────────────────────────┐
│  .NET 8.0                                       │
├─────────────────────────────────────────────────┤
│                                                 │
│  [.NET SDK 8.0.x]                              │  ← 서버용 (개발+실행)
│    Windows x64 Installer                       │
│                                                 │
│  [.NET Runtime 8.0.x]                          │  ← 출력기용 (실행만) ⭐
│    Windows x64 Installer                       │
│                                                 │
│  [ASP.NET Core Runtime 8.0.x]                  │  ← 서버용 (추천)
│    Windows x64 Hosting Bundle                  │
│                                                 │
└─────────────────────────────────────────────────┘
```

---

## ✅ 설치 확인 방법

### 모든 PC에서 공통 확인
```powershell
# PowerShell 또는 CMD 열기
dotnet --version

# 출력 예시 (성공):
8.0.1

# 출력 예시 (실패):
'dotnet'은(는) 내부 또는 외부 명령, 실행할 수 있는 프로그램, 또는
배치 파일이 아닙니다.
```

### 상세 정보 확인
```powershell
dotnet --info

# 출력 예시:
.NET SDK:
 Version:   8.0.101
 ...

.NET runtimes installed:
  Microsoft.AspNetCore.App 8.0.1
  Microsoft.NETCore.App 8.0.1
```

---

## 🆘 설치 시 문제 해결

### 문제 1: "dotnet: command not found" 또는 "내부 또는 외부 명령이 아닙니다"

**원인**: 환경 변수가 설정되지 않음

**해결**:
1. PC 재부팅
2. 그래도 안 되면:
   ```powershell
   # 시스템 환경 변수 확인
   시작 > "환경 변수" 검색 > "시스템 환경 변수 편집"
   → Path에 다음 경로 있는지 확인:
   C:\Program Files\dotnet\
   ```

### 문제 2: 구버전 .NET이 설치되어 있음

**확인**:
```powershell
dotnet --version
# 출력: 6.0.25  ← .NET 6.0 (오래된 버전)
```

**해결**:
- .NET 8.0을 추가 설치하면 됨 (구버전과 공존 가능)
- 또는 "프로그램 추가/제거"에서 구버전 제거 후 8.0 설치

### 문제 3: Windows 7에서 설치 안 됨

**원인**: .NET 8.0은 Windows 7 미지원

**지원 OS**:
- ✅ Windows 10 (버전 1607 이상)
- ✅ Windows 11
- ✅ Windows Server 2016 이상
- ❌ Windows 7 (지원 안 됨)

---

## 📦 오프라인 설치 (인터넷 없는 PC)

### 1. 인터넷 되는 PC에서 다운로드
```
https://dotnet.microsoft.com/download/dotnet/8.0
→ "Download x64" 클릭
→ 설치 파일 저장 (예: dotnet-runtime-8.0.1-win-x64.exe)
```

### 2. USB로 출력기 PC에 복사
```
USB 드라이브 → 출력기 PC
```

### 3. 설치 파일 실행
```
dotnet-runtime-8.0.1-win-x64.exe 더블클릭
→ "Install" 클릭
```

---

## 🎯 빠른 참조 (출력용)

### MES 서버 PC
```
다운로드: https://dotnet.microsoft.com/download/dotnet/8.0
설치: ASP.NET Core Runtime 8.0.x - Hosting Bundle (x64)
확인: dotnet --version
```

### 출력기 PC (Collector)
```
다운로드: https://dotnet.microsoft.com/download/dotnet/8.0
설치: .NET Runtime 8.0.x (x64)
확인: dotnet --version
```

---

## 💾 직접 다운로드 링크 (2026년 2월 기준)

**주의**: 최신 버전은 위 공식 페이지에서 확인하세요!

### .NET 8.0 Runtime (출력기용)
```
https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-8.0.1-windows-x64-installer
```

### ASP.NET Core Hosting Bundle (서버용)
```
https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-8.0.1-windows-hosting-bundle-installer
```

---

## 📞 문제 발생 시

### 확인 사항
1. ✅ Windows 버전 확인 (Windows 10/11?)
2. ✅ 64비트 설치 확인 (x64?)
3. ✅ 관리자 권한으로 설치?
4. ✅ PC 재부팅했나요?

### 로그 확인
```powershell
# 설치 로그 위치
C:\Users\[사용자명]\AppData\Local\Temp\
→ dd_setup_*.log 파일 확인
```

---

## 🔄 업데이트

.NET 8.0은 정기적으로 보안 업데이트가 나옵니다:
- 8.0.1, 8.0.2, 8.0.3 등
- 마이너 버전은 호환됨 (업데이트 권장)
- 메이저 버전 변경 시 (예: 9.0) 프로젝트 수정 필요

---

**마지막 업데이트**: 2026-02-06  
**공식 문서**: https://learn.microsoft.com/ko-kr/dotnet/core/install/windows
