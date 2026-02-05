-- MES 시스템 데이터베이스 생성 스크립트
-- SQL Server용

USE master;
GO

-- 데이터베이스 생성 (이미 존재하면 삭제 후 재생성)
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'MESSystem')
BEGIN
    ALTER DATABASE MESSystem SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE MESSystem;
END
GO

CREATE DATABASE MESSystem;
GO

USE MESSystem;
GO

-- Users 테이블
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) UNIQUE NOT NULL,
    Password NVARCHAR(200) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Role NVARCHAR(20) NOT NULL CHECK (Role IN (N'관리자', N'사용자')),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

-- Categories 테이블
CREATE TABLE Categories (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) UNIQUE NOT NULL,
    CardOrder INT UNIQUE NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

-- Products 테이블
CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Code NVARCHAR(50) UNIQUE NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    CategoryId INT NOT NULL,
    DefaultSpec NVARCHAR(100),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

-- Orders 테이블
CREATE TABLE Orders (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderNumber NVARCHAR(20) UNIQUE NOT NULL,
    ClientName NVARCHAR(100) NOT NULL,
    ClientAddress NVARCHAR(200),
    ClientPhone NVARCHAR(20),
    ClientMobile NVARCHAR(20),
    ShippingMethod NVARCHAR(50) NOT NULL CHECK (ShippingMethod IN (
        N'대신택배', N'대신화물', N'한진택배', N'퀵', N'용차', N'방문수령', N'직접배송'
    )),
    PaymentMethod NVARCHAR(20) CHECK (PaymentMethod IN (N'착불', N'선불')),
    ShippingDate DATE NOT NULL,
    ShippingTime TIME,
    FilePath NVARCHAR(500),
    Version INT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50),
    IsDeleted BIT DEFAULT 0,
    DeletedAt DATETIME2,
    DeletedBy NVARCHAR(50),
    DeleteReason NVARCHAR(200),
    ParentOrderId INT,
    OrderType NVARCHAR(20),
    ParentReason NVARCHAR(200),
    FOREIGN KEY (ParentOrderId) REFERENCES Orders(Id)
);

-- OrderItems 테이블
CREATE TABLE OrderItems (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Spec NVARCHAR(100),
    Description NVARCHAR(200),
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2),
    DesignFileName NVARCHAR(200),
    Remark NVARCHAR(200),
    LineNumber INT NOT NULL,
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

-- Cards 테이블
CREATE TABLE Cards (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    CategoryId INT NOT NULL,
    CardNumber NVARCHAR(30) UNIQUE NOT NULL,
    Status NVARCHAR(20) DEFAULT N'대기' CHECK (Status IN (N'대기', N'작업중', N'완료')),
    IsModified BIT DEFAULT 0,
    ModifiedAt DATETIME2,
    ModifiedAcknowledgedAt DATETIME2,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

-- CardItems 테이블
CREATE TABLE CardItems (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CardId INT NOT NULL,
    OrderItemId INT NOT NULL,
    ProductCode NVARCHAR(50) NOT NULL,
    ProductName NVARCHAR(100) NOT NULL,
    Spec NVARCHAR(100),
    Description NVARCHAR(200),
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2),
    DesignFileName NVARCHAR(200),
    Remark NVARCHAR(200),
    LineNumber INT NOT NULL,
    FOREIGN KEY (CardId) REFERENCES Cards(Id) ON DELETE CASCADE,
    FOREIGN KEY (OrderItemId) REFERENCES OrderItems(Id)
);

-- EventLogs 테이블
CREATE TABLE EventLogs (
    Id INT PRIMARY KEY IDENTITY(1,1),
    EventType NVARCHAR(20) NOT NULL,
    CardId INT,
    CardNumber NVARCHAR(30) NOT NULL,
    CollectorId NVARCHAR(50),
    Timestamp DATETIME2 NOT NULL,
    RawJson NVARCHAR(MAX),
    IsProcessed BIT DEFAULT 0,
    ProcessedAt DATETIME2,
    ErrorMessage NVARCHAR(500),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (CardId) REFERENCES Cards(Id) ON DELETE SET NULL
);

-- 인덱스 생성
CREATE INDEX IX_Orders_OrderNumber ON Orders(OrderNumber);
CREATE INDEX IX_Orders_CreatedAt ON Orders(CreatedAt DESC);
CREATE INDEX IX_Orders_IsDeleted ON Orders(IsDeleted);

CREATE INDEX IX_Cards_CardNumber ON Cards(CardNumber);
CREATE INDEX IX_Cards_Status ON Cards(Status);
CREATE INDEX IX_Cards_IsModified ON Cards(IsModified);

CREATE INDEX IX_EventLogs_CardNumber ON EventLogs(CardNumber);
CREATE INDEX IX_EventLogs_Timestamp ON EventLogs(Timestamp DESC);
CREATE INDEX IX_EventLogs_IsProcessed ON EventLogs(IsProcessed);

GO

-- 초기 데이터 삽입
-- 분류
INSERT INTO Categories (Name, CardOrder, IsActive, CreatedAt) VALUES
(N'태극기', 1, 1, GETDATE()),
(N'현수막', 2, 1, GETDATE()),
(N'간판', 3, 1, GETDATE());

-- 사용자 (TODO: 실제 환경에서는 비밀번호 해시 필요)
INSERT INTO Users (Username, Password, FullName, Role, IsActive, CreatedAt) VALUES
('admin', 'admin123', N'관리자', N'관리자', 1, GETDATE()),
('field01', 'user123', N'현장작업자1', N'사용자', 1, GETDATE());

GO

PRINT 'MES 시스템 데이터베이스 생성 완료';
