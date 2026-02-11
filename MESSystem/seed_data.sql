-- 더미 데이터 삽입 스크립트
-- 테스트 및 데모용 데이터

-- 1. 거래처 6개 추가
INSERT INTO Clients (Id, Name, Address, Phone, Mobile, Email, BusinessNumber, CeoName, BusinessType, BusinessItem, Note, IsActive, CreatedAt) VALUES
(1, '(주)동산무역', '서울시 강남구 테헤란로 123', '02-1234-5678', '010-1234-5678', 'dongsan@example.com', '123-45-67890', '김동산', '도소매업', '인쇄물 판매', 'VIP 거래처', 1, datetime('now')),
(2, '서울광고기획', '서울시 서초구 서초대로 456', '02-2345-6789', '010-2345-6789', 'seoul@example.com', '234-56-78901', '이서울', '서비스업', '광고기획', '월 정기 거래', 1, datetime('now')),
(3, 'OO건설(주)', '경기도 성남시 분당구 판교역로 789', '031-3456-7890', '010-3456-7890', 'oo@example.com', '345-67-89012', '박건설', '건설업', '종합건설', '대형 현수막 주문', 1, datetime('now')),
(4, '부산마케팅', '부산시 해운대구 센텀중앙로 234', '051-4567-8901', '010-4567-8901', 'busan@example.com', '456-78-90123', '최부산', '서비스업', '마케팅', '분기별 주문', 1, datetime('now')),
(5, '대한상사', '대전시 유성구 대덕대로 567', '042-5678-9012', '010-5678-9012', 'daehan@example.com', '567-89-01234', '정대한', '도소매업', '판촉물', '소량 다품종', 1, datetime('now')),
(6, '글로벌무역', '인천시 연수구 송도과학로 890', '032-6789-0123', '010-6789-0123', 'global@example.com', '678-90-12345', '한글로벌', '무역업', '수출입', '수출용 제품', 1, datetime('now'));

-- 2. 태극기 품목 5개 추가 (CategoryId = 1)
INSERT INTO Products (Id, CategoryId, Code, Name, DefaultSpec, IsActive, IsDeleted, CreatedAt) VALUES
(1, 1, 'TG-001', '태극기 90x135', '90cm x 135cm', 1, 0, datetime('now')),
(2, 1, 'TG-002', '태극기 150x225', '150cm x 225cm', 1, 0, datetime('now')),
(3, 1, 'TG-003', '태극기 소형 30x45', '30cm x 45cm', 1, 0, datetime('now')),
(4, 1, 'TG-004', '태극기 대형 270x405', '270cm x 405cm', 1, 0, datetime('now')),
(5, 1, 'TG-005', '태극기 차량용 20x30', '20cm x 30cm', 1, 0, datetime('now'));

-- 3. 현수막 품목 5개 추가 (CategoryId = 2)
INSERT INTO Products (Id, CategoryId, Code, Name, DefaultSpec, IsActive, IsDeleted, CreatedAt) VALUES
(6, 2, 'HS-001', '현수막 표준형', '90cm x 180cm', 1, 0, datetime('now')),
(7, 2, 'HS-002', '현수막 대형', '150cm x 300cm', 1, 0, datetime('now')),
(8, 2, 'HS-003', '현수막 소형', '60cm x 120cm', 1, 0, datetime('now')),
(9, 2, 'HS-004', '현수막 초대형', '200cm x 500cm', 1, 0, datetime('now')),
(10, 2, 'HS-005', '현수막 배너형', '80cm x 200cm', 1, 0, datetime('now'));

-- 4. 간판 품목 5개 추가 (CategoryId = 3)
INSERT INTO Products (Id, CategoryId, Code, Name, DefaultSpec, IsActive, IsDeleted, CreatedAt) VALUES
(11, 3, 'KB-001', '간판 아크릴 소형', '50cm x 100cm', 1, 0, datetime('now')),
(12, 3, 'KB-002', '간판 아크릴 중형', '100cm x 200cm', 1, 0, datetime('now')),
(13, 3, 'KB-003', '간판 LED 소형', '60cm x 120cm', 1, 0, datetime('now')),
(14, 3, 'KB-004', '간판 LED 대형', '150cm x 300cm', 1, 0, datetime('now')),
(15, 3, 'KB-005', '간판 네온사인', '80cm x 150cm', 1, 0, datetime('now'));

-- 5. 주문서 5개 추가
INSERT INTO Orders (Id, OrderNumber, ClientId, ClientName, ClientAddress, ClientPhone, ClientMobile, ShippingMethod, PaymentMethod, ShippingDate, ShippingTime, FilePath, Status, Version, CreatedAt, UpdatedAt, CreatedBy, IsDeleted, ParentOrderId, OrderType, IsSalesClosed, SalesClosingItemId) VALUES
(1, '20260210-01', 1, '(주)동산무역', '서울시 강남구 테헤란로 123', '02-1234-5678', '010-1234-5678', '대신택배', '착불', '2026-02-15', NULL, NULL, '완료', 1, datetime('now', '-5 days'), datetime('now', '-1 days'), 'admin', 0, NULL, '신규', 0, NULL),
(2, '20260211-01', 2, '서울광고기획', '서울시 서초구 서초대로 456', '02-2345-6789', '010-2345-6789', '한진택배', '선불', '2026-02-16', NULL, NULL, '완료', 1, datetime('now', '-4 days'), datetime('now', '-1 days'), 'admin', 0, NULL, '신규', 0, NULL),
(3, '20260211-02', 3, 'OO건설(주)', '경기도 성남시 분당구 판교역로 789', '031-3456-7890', '010-3456-7890', '용차', '착불', '2026-02-18', '14:00:00', NULL, '완료', 1, datetime('now', '-3 days'), datetime('now'), 'admin', 0, NULL, '신규', 0, NULL),
(4, '20260212-01', 4, '부산마케팅', '부산시 해운대구 센텀중앙로 234', '051-4567-8901', '010-4567-8901', '대신화물', '착불', '2026-02-20', NULL, NULL, '진행중', 1, datetime('now', '-2 days'), datetime('now', '-1 days'), 'admin', 0, NULL, '신규', 0, NULL),
(5, '20260213-01', 5, '대한상사', '대전시 유성구 대덕대로 567', '042-5678-9012', '010-5678-9012', '퀵', '선불', '2026-02-14', '10:00:00', NULL, '완료', 1, datetime('now', '-1 days'), datetime('now'), 'admin', 0, NULL, '신규', 0, NULL);

-- 6. 주문서 품목 추가
-- 주문서 1: 태극기 90x135 100매 + 태극기 소형 50매
INSERT INTO OrderItems (OrderId, ProductId, Spec, Description, Quantity, UnitPrice, LineNumber, IsDeleted) VALUES
(1, 1, '90cm x 135cm', '긴급 주문', 100, 10000, 1, 0),
(1, 3, '30cm x 45cm', NULL, 50, 3000, 2, 0);

-- 주문서 2: 현수막 표준형 10개 + 현수막 대형 3개
INSERT INTO OrderItems (OrderId, ProductId, Spec, Description, Quantity, UnitPrice, LineNumber, IsDeleted) VALUES
(2, 6, '90cm x 180cm', '이벤트용', 10, 25000, 1, 0),
(2, 7, '150cm x 300cm', NULL, 3, 45000, 2, 0);

-- 주문서 3: 현수막 초대형 5개
INSERT INTO OrderItems (OrderId, ProductId, Spec, Description, Quantity, UnitPrice, LineNumber, IsDeleted) VALUES
(3, 9, '200cm x 500cm', '건설 현장용', 5, 85000, 1, 0);

-- 주문서 4: 간판 아크릴 중형 2개 + 간판 LED 소형 1개
INSERT INTO OrderItems (OrderId, ProductId, Spec, Description, Quantity, UnitPrice, LineNumber, IsDeleted) VALUES
(4, 12, '100cm x 200cm', '매장 간판', 2, 350000, 1, 0),
(4, 13, '60cm x 120cm', NULL, 1, 280000, 2, 0);

-- 주문서 5: 태극기 150x225 30매 + 차량용 100매
INSERT INTO OrderItems (OrderId, ProductId, Spec, Description, Quantity, UnitPrice, LineNumber, IsDeleted) VALUES
(5, 2, '150cm x 225cm', '3.1절 기념', 30, 18000, 1, 0),
(5, 5, '20cm x 30cm', NULL, 100, 2000, 2, 0);

-- 7. 추가 사용자 3명 (기존 admin, field01 포함 총 5명)
INSERT INTO Users (Id, Username, Password, FullName, Role, IsActive, CreatedAt) VALUES
(3, 'manager01', 'manager123', '김관리', '관리자', 1, datetime('now')),
(4, 'field02', 'user123', '박현장', '사용자', 1, datetime('now')),
(5, 'accounting01', 'account123', '최회계', '관리자', 1, datetime('now'));

-- 더미 데이터 삽입 완료
