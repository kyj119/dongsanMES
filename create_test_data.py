#!/usr/bin/env python3
"""
테스트 데이터 생성 스크립트
주문서 입력이 안 될 때 사용
"""
import sqlite3
from datetime import datetime

# 데이터베이스 경로 (MES 서버 PC에서 실행)
DB_PATH = r"C:\dongsanMES\MESSystem\MESSystem.db"

def create_test_order():
    """테스트 주문서 및 카드 생성"""
    
    conn = sqlite3.connect(DB_PATH)
    cursor = conn.cursor()
    
    try:
        print("=" * 60)
        print("테스트 데이터 생성 시작")
        print("=" * 60)
        
        # 1. 테스트 거래처 추가
        print("\n[1/4] 거래처 추가...")
        cursor.execute("""
            INSERT OR IGNORE INTO Clients (Name, Address, Phone, Mobile, IsActive, CreatedAt)
            VALUES (?, ?, ?, ?, ?, ?)
        """, ('테스트거래처', '서울시 강남구', '02-1234-5678', '010-1234-5678', 1, datetime.now()))
        
        client_id = cursor.lastrowid
        if client_id == 0:
            cursor.execute("SELECT Id FROM Clients WHERE Name = ?", ('테스트거래처',))
            client_id = cursor.fetchone()[0]
        print(f"✅ 거래처 ID: {client_id}")
        
        # 2. 주문서 추가
        print("\n[2/4] 주문서 추가...")
        order_number = f"ORD-{datetime.now().strftime('%Y%m%d')}-03"
        cursor.execute("""
            INSERT INTO Orders (
                OrderNumber, ClientName, ClientId, ShippingDate, ShippingTime,
                ShippingMethod, PaymentMethod, IsDeleted, Status, Priority,
                CreatedBy, CreatedAt
            )
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        """, (
            order_number, '테스트거래처', client_id, datetime.now().date(),
            '16:00', '한진택배', '선불', 0, '작성', 3, 'admin', datetime.now()
        ))
        order_id = cursor.lastrowid
        print(f"✅ 주문서: {order_number} (ID: {order_id})")
        
        # 3. 주문서 품목 추가
        print("\n[3/4] 품목 추가...")
        items = [
            ('TG-001', '태극기', '100x150cm', 100, 5000, '20260206-03-1_태극기.eps'),
            ('HS-001', '달력', '90x120cm', 50, 8000, '20260206-03-2_달력.eps'),
            ('HS-002', '현수막', '180x240cm', 30, 12000, '20260206-03-3_현수막.eps'),
        ]
        
        for idx, (code, name, spec, qty, price, filename) in enumerate(items, 1):
            cursor.execute("""
                INSERT INTO OrderItems (
                    OrderId, CategoryId, ProductCode, ProductName, Spec,
                    Quantity, UnitPrice, TotalPrice, DesignFileName, FilePath, CreatedAt
                )
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
            """, (
                order_id, 1, code, name, spec, qty, price, qty * price,
                filename, f'Z:\\Designs\\2026\\02\\20260206-03\\{filename}',
                datetime.now()
            ))
            print(f"  ✅ 품목 {idx}: {name} ({qty}개)")
        
        # 4. 카드 생성
        print("\n[4/4] 카드 생성...")
        card_numbers = ['20260206-03-1', '20260206-03-2', '20260206-03-3']
        
        for idx, card_number in enumerate(card_numbers, 1):
            cursor.execute("""
                INSERT INTO Cards (OrderId, CardNumber, Status, CreatedBy, CreatedAt)
                VALUES (?, ?, ?, ?, ?)
            """, (order_id, card_number, '대기', 'admin', datetime.now()))
            card_id = cursor.lastrowid
            print(f"  ✅ 카드 {idx}: {card_number} (ID: {card_id})")
        
        # 커밋
        conn.commit()
        
        print("\n" + "=" * 60)
        print("✅ 테스트 데이터 생성 완료!")
        print("=" * 60)
        print(f"\n📌 주문서 번호: {order_number}")
        print(f"📌 카드 번호: {', '.join(card_numbers)}")
        print(f"\n🌐 MES 웹에서 확인: http://localhost:5285")
        print(f"   현장 대시보드 또는 주문서 목록에서 확인하세요!")
        
    except Exception as e:
        conn.rollback()
        print(f"\n❌ 에러 발생: {e}")
        import traceback
        traceback.print_exc()
    
    finally:
        conn.close()

if __name__ == "__main__":
    try:
        create_test_order()
    except FileNotFoundError:
        print(f"❌ 데이터베이스 파일을 찾을 수 없습니다: {DB_PATH}")
        print("경로를 확인하세요!")
