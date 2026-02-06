-- 테스트용 주문서 및 카드 직접 생성 스크립트
-- MESSystem.db에 직접 실행

-- 1. 테스트 거래처 추가 (이미 있을 수 있음)
INSERT OR IGNORE INTO Clients (Id, Name, Address, Phone, Mobile, IsActive, CreatedAt)
VALUES (10, '테스트거래처', '서울시 강남구', '02-1234-5678', '010-1234-5678', 1, datetime('now'));

-- 2. 테스트 주문서 추가
INSERT INTO Orders (
    OrderNumber, 
    ClientName, 
    ClientId,
    ShippingDate, 
    ShippingTime,
    ShippingMethod,
    PaymentMethod,
    IsDeleted,
    Status,
    Priority,
    CreatedBy,
    CreatedAt
)
VALUES (
    'ORD-20260206-03',
    '테스트거래처',
    10,
    date('now'),
    '16:00',
    '한진택배',
    '선불',
    0,
    '작성',
    3,
    'admin',
    datetime('now')
);

-- 방금 생성한 주문서 ID 가져오기 (자동 증가 값)
-- SQLite에서는 last_insert_rowid() 사용

-- 3. 주문서 품목 추가 (3개)
INSERT INTO OrderItems (
    OrderId,
    CategoryId,
    ProductCode,
    ProductName,
    Spec,
    Quantity,
    UnitPrice,
    TotalPrice,
    DesignFileName,
    FilePath,
    Memo,
    CreatedAt
)
VALUES 
-- 품목 1: 태극기
(
    last_insert_rowid(), -- 방금 생성한 주문서 ID
    1, -- 태극기 카테고리
    'TG-001',
    '태극기',
    '100x150cm',
    100,
    5000,
    500000,
    '20260206-03-1_태극기.eps',
    'Z:\Designs\2026\02\20260206-03\20260206-03-1_태극기.eps',
    '',
    datetime('now')
),
-- 품목 2: 달력
(
    last_insert_rowid(),
    2, -- 현수막 카테고리
    'HS-001',
    '달력',
    '90x120cm',
    50,
    8000,
    400000,
    '20260206-03-2_달력.eps',
    'Z:\Designs\2026\02\20260206-03\20260206-03-2_달력.eps',
    '',
    datetime('now')
),
-- 품목 3: 현수막
(
    last_insert_rowid(),
    2, -- 현수막 카테고리
    'HS-002',
    '현수막',
    '180x240cm',
    30,
    12000,
    360000,
    '20260206-03-3_현수막.eps',
    'Z:\Designs\2026\02\20260206-03\20260206-03-3_현수막.eps',
    '',
    datetime('now')
);

-- 4. 카드 생성 (3개 - 품목별로 1개씩)
INSERT INTO Cards (
    OrderId,
    CardNumber,
    Status,
    CreatedBy,
    CreatedAt
)
VALUES
-- 카드 1
(
    last_insert_rowid(),
    '20260206-03-1',
    '대기',
    'admin',
    datetime('now')
),
-- 카드 2  
(
    last_insert_rowid(),
    '20260206-03-2',
    '대기',
    'admin',
    datetime('now')
),
-- 카드 3
(
    last_insert_rowid(),
    '20260206-03-3',
    '대기',
    'admin',
    datetime('now')
);

-- 5. 카드 품목 매핑 (CardItems)
-- 카드 ID를 수동으로 확인 후 입력해야 함
-- 또는 카드 생성 후 SELECT로 ID 확인

-- 확인 쿼리
SELECT 
    o.OrderNumber,
    o.ClientName,
    c.CardNumber,
    c.Status
FROM Orders o
LEFT JOIN Cards c ON c.OrderId = o.Id
WHERE o.OrderNumber = 'ORD-20260206-03'
ORDER BY c.CardNumber;
