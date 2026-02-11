-- ERP 매출 관리 시스템 마이그레이션 스크립트
-- 실행 방법: SQLite DB에 직접 실행 또는 Entity Framework Migration 사용

-- 1. Client 테이블에 세금계산서 발행용 컬럼 추가
ALTER TABLE Clients ADD COLUMN BusinessNumber TEXT NULL;
ALTER TABLE Clients ADD COLUMN CeoName TEXT NULL;
ALTER TABLE Clients ADD COLUMN BusinessType TEXT NULL;
ALTER TABLE Clients ADD COLUMN BusinessItem TEXT NULL;

-- 2. Orders 테이블에 매출 마감 관련 컬럼 추가
ALTER TABLE Orders ADD COLUMN IsSalesClosed INTEGER NOT NULL DEFAULT 0;
ALTER TABLE Orders ADD COLUMN SalesClosingItemId INTEGER NULL;

-- 3. SalesClosings 테이블 생성
CREATE TABLE IF NOT EXISTS SalesClosings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ClosingNumber TEXT NOT NULL,
    ClientId INTEGER NOT NULL,
    StartDate TEXT NOT NULL,
    EndDate TEXT NOT NULL,
    SupplyAmount REAL NOT NULL,
    DiscountAmount REAL NOT NULL,
    AdditionalAmount REAL NOT NULL,
    AdjustedSupplyAmount REAL NOT NULL,
    VatAmount REAL NOT NULL,
    TotalAmount REAL NOT NULL,
    PaymentType TEXT NOT NULL,
    CardFeeRate REAL NULL,
    CardFeeAmount REAL NULL,
    Memo TEXT NULL,
    Status TEXT NOT NULL,
    ConfirmedAt TEXT NULL,
    ConfirmedBy TEXT NULL,
    CreatedAt TEXT NOT NULL,
    CreatedBy TEXT NULL,
    UpdatedAt TEXT NOT NULL,
    UpdatedBy TEXT NULL,
    FOREIGN KEY (ClientId) REFERENCES Clients(Id)
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_SalesClosings_ClosingNumber ON SalesClosings(ClosingNumber);
CREATE INDEX IF NOT EXISTS IX_SalesClosings_ClientId ON SalesClosings(ClientId);
CREATE INDEX IF NOT EXISTS IX_SalesClosings_Status ON SalesClosings(Status);
CREATE INDEX IF NOT EXISTS IX_SalesClosings_StartDate ON SalesClosings(StartDate);

-- 4. SalesClosingItems 테이블 생성
CREATE TABLE IF NOT EXISTS SalesClosingItems (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SalesClosingId INTEGER NOT NULL,
    OrderId INTEGER NOT NULL,
    OrderNumber TEXT NOT NULL,
    OrderDate TEXT NOT NULL,
    SupplyAmount REAL NOT NULL,
    DiscountAmount REAL NOT NULL,
    Memo TEXT NULL,
    FOREIGN KEY (SalesClosingId) REFERENCES SalesClosings(Id) ON DELETE CASCADE,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id)
);

CREATE INDEX IF NOT EXISTS IX_SalesClosingItems_SalesClosingId ON SalesClosingItems(SalesClosingId);
CREATE INDEX IF NOT EXISTS IX_SalesClosingItems_OrderId ON SalesClosingItems(OrderId);

-- 5. TaxInvoices 테이블 생성
CREATE TABLE IF NOT EXISTS TaxInvoices (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SalesClosingId INTEGER NOT NULL,
    ApprovalNumber TEXT NULL,
    IssueDate TEXT NOT NULL,
    IssueType TEXT NOT NULL,
    PurposeType TEXT NOT NULL,
    SupplyAmount REAL NOT NULL,
    TaxAmount REAL NOT NULL,
    TotalAmount REAL NOT NULL,
    SupplierBusinessNumber TEXT NOT NULL,
    SupplierName TEXT NOT NULL,
    SupplierCeoName TEXT NOT NULL,
    SupplierAddress TEXT NULL,
    SupplierBusinessType TEXT NULL,
    SupplierBusinessItem TEXT NULL,
    SupplierEmail TEXT NULL,
    BuyerBusinessNumber TEXT NOT NULL,
    BuyerName TEXT NOT NULL,
    BuyerCeoName TEXT NOT NULL,
    BuyerAddress TEXT NULL,
    BuyerBusinessType TEXT NULL,
    BuyerBusinessItem TEXT NULL,
    BuyerEmail TEXT NULL,
    Memo TEXT NULL,
    Status TEXT NOT NULL,
    IssuedAt TEXT NULL,
    SentAt TEXT NULL,
    IsSubmittedToHometax INTEGER NOT NULL DEFAULT 0,
    SubmittedToHometaxAt TEXT NULL,
    HometaxResponse TEXT NULL,
    XmlFilePath TEXT NULL,
    PdfFilePath TEXT NULL,
    CreatedAt TEXT NOT NULL,
    CreatedBy TEXT NULL,
    UpdatedAt TEXT NOT NULL,
    FOREIGN KEY (SalesClosingId) REFERENCES SalesClosings(Id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_TaxInvoices_SalesClosingId ON TaxInvoices(SalesClosingId);
CREATE INDEX IF NOT EXISTS IX_TaxInvoices_ApprovalNumber ON TaxInvoices(ApprovalNumber);
CREATE INDEX IF NOT EXISTS IX_TaxInvoices_IssueDate ON TaxInvoices(IssueDate);
CREATE INDEX IF NOT EXISTS IX_TaxInvoices_Status ON TaxInvoices(Status);

-- 6. TaxInvoiceItems 테이블 생성
CREATE TABLE IF NOT EXISTS TaxInvoiceItems (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TaxInvoiceId INTEGER NOT NULL,
    ItemDate TEXT NULL,
    ItemName TEXT NOT NULL,
    Specification TEXT NULL,
    Quantity REAL NOT NULL,
    UnitPrice REAL NOT NULL,
    SupplyAmount REAL NOT NULL,
    TaxAmount REAL NOT NULL,
    Memo TEXT NULL,
    FOREIGN KEY (TaxInvoiceId) REFERENCES TaxInvoices(Id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_TaxInvoiceItems_TaxInvoiceId ON TaxInvoiceItems(TaxInvoiceId);

-- 7. Payments 테이블 생성
CREATE TABLE IF NOT EXISTS Payments (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PaymentNumber TEXT NOT NULL,
    SalesClosingId INTEGER NULL,
    ClientId INTEGER NOT NULL,
    PaymentDate TEXT NOT NULL,
    Amount REAL NOT NULL,
    PaymentMethod TEXT NOT NULL,
    BankAccount TEXT NULL,
    DepositorName TEXT NULL,
    Memo TEXT NULL,
    BankTransactionId INTEGER NULL,
    IsMatched INTEGER NOT NULL DEFAULT 0,
    MatchedAt TEXT NULL,
    MatchedBy TEXT NULL,
    CreatedAt TEXT NOT NULL,
    CreatedBy TEXT NULL,
    FOREIGN KEY (SalesClosingId) REFERENCES SalesClosings(Id),
    FOREIGN KEY (ClientId) REFERENCES Clients(Id),
    FOREIGN KEY (BankTransactionId) REFERENCES BankTransactions(Id)
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_Payments_PaymentNumber ON Payments(PaymentNumber);
CREATE INDEX IF NOT EXISTS IX_Payments_SalesClosingId ON Payments(SalesClosingId);
CREATE INDEX IF NOT EXISTS IX_Payments_ClientId ON Payments(ClientId);
CREATE INDEX IF NOT EXISTS IX_Payments_PaymentDate ON Payments(PaymentDate);
CREATE INDEX IF NOT EXISTS IX_Payments_IsMatched ON Payments(IsMatched);

-- 8. BankTransactions 테이블 생성
CREATE TABLE IF NOT EXISTS BankTransactions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    BankCode TEXT NOT NULL,
    BankName TEXT NULL,
    AccountNumber TEXT NOT NULL,
    TransactionDateTime TEXT NOT NULL,
    TransactionType TEXT NOT NULL,
    Amount REAL NOT NULL,
    Balance REAL NOT NULL,
    Description TEXT NULL,
    CounterpartyName TEXT NULL,
    ExternalTransactionId TEXT NULL,
    IsProcessed INTEGER NOT NULL DEFAULT 0,
    ProcessedAt TEXT NULL,
    CreatedAt TEXT NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_BankTransactions_ExternalTransactionId ON BankTransactions(ExternalTransactionId);
CREATE INDEX IF NOT EXISTS IX_BankTransactions_TransactionDateTime ON BankTransactions(TransactionDateTime);
CREATE INDEX IF NOT EXISTS IX_BankTransactions_IsProcessed ON BankTransactions(IsProcessed);

-- 마이그레이션 완료
