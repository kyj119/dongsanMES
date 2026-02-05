#!/bin/bash

# MES Collector 시각화 데모 스크립트
# 작성일: 2026-02-05

echo "╔════════════════════════════════════════════════════════════╗"
echo "║         MES Collector 시각화 데모                          ║"
echo "║         실시간 파일 모니터링 시뮬레이션                    ║"
echo "╚════════════════════════════════════════════════════════════╝"
echo ""

# 색상 코드
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# 테스트 디렉토리 설정
TEST_DIR="/tmp/TOPAZ_RIP_TEST"
PREVIEW_DIR="$TEST_DIR/preview"
PRINTLOG_DIR="$TEST_DIR/printlog"
JOB_DIR="$TEST_DIR/job"

# 카드번호 생성
CARD_DATE=$(date +%Y%m%d)
CARD_NUM="${CARD_DATE}-01-1"

echo "${CYAN}[단계 1/7] 테스트 환경 초기화...${NC}"
rm -rf $TEST_DIR
mkdir -p $PREVIEW_DIR
mkdir -p $PRINTLOG_DIR
mkdir -p $JOB_DIR
echo "${GREEN}✓ 테스트 폴더 생성 완료${NC}"
echo "  - Preview:  $PREVIEW_DIR"
echo "  - PrintLog: $PRINTLOG_DIR"
echo "  - Job:      $JOB_DIR"
echo ""
sleep 2

echo "${CYAN}[단계 2/7] MES 시스템 시뮬레이션 시작${NC}"
echo "${YELLOW}시나리오: 태극기 주문 출력 과정${NC}"
echo "  주문번호: ${CARD_DATE}-01"
echo "  카드번호: ${CARD_NUM}"
echo "  품목: 태극기 90x135cm"
echo "  수량: 100매"
echo ""
sleep 2

echo "┌─────────────────────────────────────────────────────────┐"
echo "│                  TOPAZ RIP 출력기                        │"
echo "│  디자이너가 '${CARD_NUM}' 작업을 시작합니다...     │"
echo "└─────────────────────────────────────────────────────────┘"
echo ""
sleep 2

# =================================================================
# 이벤트 1: 작업대기
# =================================================================
echo "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo "${BLUE}[이벤트 1/3] 작업대기 (Preview)${NC}"
echo "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"

echo ""
echo "  $(date '+%H:%M:%S') ${CYAN}[TOPAZ RIP]${NC} RIP 처리 중..."
sleep 1

FILE1="$PREVIEW_DIR/${CARD_NUM}.bmp.tsc"
echo "preview file" > "$FILE1"
echo "  $(date '+%H:%M:%S') ${GREEN}[파일생성]${NC} ${CARD_NUM}.bmp.tsc"

sleep 1
echo ""
echo "  $(date '+%H:%M:%S') ${YELLOW}[Collector]${NC} 파일 감지!"
echo "  $(date '+%H:%M:%S') ${YELLOW}[Collector]${NC} 카드번호 추출: ${CARD_NUM}"
echo "  $(date '+%H:%M:%S') ${YELLOW}[Collector]${NC} 이벤트 생성 중..."

sleep 1
echo ""
echo "  📤 HTTP POST /api/events"
echo "  {" 
echo "    \"eventType\": \"작업대기\","
echo "    \"cardNumber\": \"${CARD_NUM}\","
echo "    \"collectorId\": \"COLLECTOR-001\","
echo "    \"timestamp\": \"$(date -Iseconds)\""
echo "  }"

sleep 1
echo ""
echo "  $(date '+%H:%M:%S') ${GREEN}[MES서버]${NC} 이벤트 수신 성공!"
echo "  $(date '+%H:%M:%S') ${GREEN}[MES서버]${NC} 카드 상태 업데이트: [대기]"
echo ""
sleep 2

# =================================================================
# 이벤트 2: 작업시작
# =================================================================
echo "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo "${BLUE}[이벤트 2/3] 작업시작 (PrintLog)${NC}"
echo "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"

echo ""
echo "  $(date '+%H:%M:%S') ${CYAN}[TOPAZ RIP]${NC} 프린터 출력 시작!"
sleep 1

TIMESTAMP=$(date '+%H%M%S')
FILE2="$PRINTLOG_DIR/${CARD_NUM}_${TIMESTAMP}.log"
cat > "$FILE2" << EOF
JobName=태극기_90x135
Copies=100
StartTime=$(date '+%Y-%m-%d %H:%M:%S')
PrinterName=MIMAKI-JV300
EOF

echo "  $(date '+%H:%M:%S') ${GREEN}[파일생성]${NC} ${CARD_NUM}_${TIMESTAMP}.log"

sleep 1
echo ""
echo "  $(date '+%H:%M:%S') ${YELLOW}[Collector]${NC} 파일 감지!"
echo "  $(date '+%H:%M:%S') ${YELLOW}[Collector]${NC} LOG 파일 파싱 중..."

sleep 1
echo ""
echo "  📄 파싱 결과:"
echo "    - JobName: 태극기_90x135"
echo "    - Copies: 100"
echo "    - StartTime: $(date '+%Y-%m-%d %H:%M:%S')"

sleep 1
echo ""
echo "  📤 HTTP POST /api/events"
echo "  {"
echo "    \"eventType\": \"작업시작\","
echo "    \"cardNumber\": \"${CARD_NUM}\","
echo "    \"collectorId\": \"COLLECTOR-001\","
echo "    \"timestamp\": \"$(date -Iseconds)\","
echo "    \"metadata\": {"
echo "      \"JobName\": \"태극기_90x135\","
echo "      \"Copies\": \"100\""
echo "    }"
echo "  }"

sleep 1
echo ""
echo "  $(date '+%H:%M:%S') ${GREEN}[MES서버]${NC} 이벤트 수신 성공!"
echo "  $(date '+%H:%M:%S') ${GREEN}[MES서버]${NC} 카드 상태 업데이트: [대기] → [작업중]"
echo ""
echo "  🖨️  프린터 출력 중..."
for i in {1..5}; do
    echo -n "  ▓"
    sleep 0.5
done
echo " 완료!"
echo ""
sleep 2

# =================================================================
# 이벤트 3: 작업완료
# =================================================================
echo "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo "${BLUE}[이벤트 3/3] 작업완료 (Job)${NC}"
echo "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"

echo ""
echo "  $(date '+%H:%M:%S') ${CYAN}[TOPAZ RIP]${NC} 출력 완료!"
sleep 1

FILE3="$JOB_DIR/${CARD_NUM}0002.job"
cat > "$FILE3" << EOF
PrintFile=Z:\Designs\2026\02\${CARD_DATE}-01\${CARD_NUM}_태극기.ai
DestSizeX=900.000000
DestSizeY=1350.000000
BeginDate=$(date '+%Y-%m-%d %H:%M:%S')
EndDate=$(date '+%Y-%m-%d %H:%M:%S')
TotalPages=100
PrinterName=MIMAKI-JV300
EOF

echo "  $(date '+%H:%M:%S') ${GREEN}[파일생성]${NC} ${CARD_NUM}0002.job"

sleep 1
echo ""
echo "  $(date '+%H:%M:%S') ${YELLOW}[Collector]${NC} 파일 감지!"
echo "  $(date '+%H:%M:%S') ${YELLOW}[Collector]${NC} JOB 파일 파싱 중..."

sleep 1
echo ""
echo "  📄 파싱 결과:"
echo "    - PrintFile: ${CARD_NUM}_태극기.ai"
echo "    - Size: 900 x 1350 mm"
echo "    - TotalPages: 100"
echo "    - EndTime: $(date '+%Y-%m-%d %H:%M:%S')"

sleep 1
echo ""
echo "  📤 HTTP POST /api/events"
echo "  {"
echo "    \"eventType\": \"작업완료\","
echo "    \"cardNumber\": \"${CARD_NUM}\","
echo "    \"collectorId\": \"COLLECTOR-001\","
echo "    \"timestamp\": \"$(date -Iseconds)\","
echo "    \"metadata\": {"
echo "      \"PrintFile\": \"${CARD_NUM}_태극기.ai\","
echo "      \"DestSizeX\": \"900.000000\","
echo "      \"TotalPages\": \"100\""
echo "    }"
echo "  }"

sleep 1
echo ""
echo "  $(date '+%H:%M:%S') ${GREEN}[MES서버]${NC} 이벤트 수신 성공!"
echo "  $(date '+%H:%M:%S') ${GREEN}[MES서버]${NC} 카드 상태 업데이트: [작업중] → [완료]"
echo ""
sleep 2

# =================================================================
# 최종 요약
# =================================================================
echo ""
echo "╔════════════════════════════════════════════════════════════╗"
echo "║                    시뮬레이션 완료!                         ║"
echo "╚════════════════════════════════════════════════════════════╝"
echo ""

echo "${GREEN}✓ 작업 흐름 요약:${NC}"
echo ""
echo "  1️⃣  작업대기 → Preview 폴더에 .bmp.tsc 생성"
echo "  2️⃣  작업시작 → PrintLog 폴더에 .log 생성 (수량, 시작시간)"
echo "  3️⃣  작업완료 → Job 폴더에 .job 생성 (완료시간, 사이즈)"
echo ""

echo "${CYAN}📊 생성된 파일:${NC}"
ls -lh "$PREVIEW_DIR" | tail -n +2
ls -lh "$PRINTLOG_DIR" | tail -n +2
ls -lh "$JOB_DIR" | tail -n +2
echo ""

echo "${YELLOW}💡 파일 내용 확인:${NC}"
echo ""
echo "${BLUE}▸ PrintLog 내용:${NC}"
cat "$FILE2"
echo ""
echo "${BLUE}▸ Job 내용:${NC}"
cat "$FILE3"
echo ""

echo "╔════════════════════════════════════════════════════════════╗"
echo "║  이제 실제 Collector를 실행하여 동일한 과정을 테스트하세요  ║"
echo "╚════════════════════════════════════════════════════════════╝"
echo ""
echo "${GREEN}다음 단계:${NC}"
echo "  1. MES 서버 실행: cd MESSystem && dotnet run"
echo "  2. Collector 실행: cd MESCollector && dotnet run"
echo "  3. 이 스크립트 재실행으로 이벤트 생성 테스트"
echo ""

echo "${YELLOW}테스트 폴더 위치: $TEST_DIR${NC}"
echo ""
