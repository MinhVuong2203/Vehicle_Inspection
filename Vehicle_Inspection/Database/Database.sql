-- =========================
-- 0) DATABASE
-- =========================
CREATE DATABASE VehicleInspectionCenter;
GO
USE VehicleInspectionCenter;
GO

-- =========================
-- 1) NHÓM NGƯỜI DÙNG - NHÂN SỰ - PHÂN QUYỀN
-- =========================
CREATE TABLE dbo.Role(
    RoleId      INT IDENTITY(1,1) PRIMARY KEY,
    RoleCode    NVARCHAR(50) NOT NULL UNIQUE,   
	RoleAcronym NVARCHAR(50) NOT NULL,
    RoleName    NVARCHAR(255) NOT NULL,
	RoleIcon    NVARCHAR(255),
	RoleHref    NVARCHAR(255)
);


INSERT INTO Role(RoleCode, RoleAcronym, RoleName, RoleIcon)
VALUES ('LOGIN', N'Đăng nhập', N'Đăng nhập hệ thống', 'fa-solid fa-arrow-right-to-bracket'),
	   ('EMPLOYEE', N'Nhân sự', N'Quản lý nhân sự', 'fa-regular fa-address-book'),
	   ('RECEIVE PROFILE', N'Hồ sơ', N'Tiếp nhận hồ sơ', 'fa-regular fa-address-card'),
	   ('INSPECTION', N'Kiểm định', N'Tạo lượt kiểm định', 'fa-solid fa-magnifying-glass'),
	   ('TOLL', N'Thu phí', N'Thu phí, in biên nhận', 'fa-solid fa-coins'),
	   ('RESULT', N'Kết quả', N'Nhập kết quả kiểm định theo công đoạn', 'fa-regular fa-pen-to-square'),
	   ('CONCLUSION', N'Kết luận', N'Chốt kết luận đạt/không đạt', 'fa-regular fa-handshake'),
	   ('REPORT', N'Xem báo cáo', N'Xem báo cáo', 'fa-solid fa-chart-pie')


CREATE TABLE Position(
	PositionId  INT IDENTITY(1,1) PRIMARY KEY,
	PoitionCode NVARCHAR(100) NOT NULL,
	PositionName NVARCHAR(100) NOT NULL,
)

CREATE TABLE Team(
	TeamId  INT IDENTITY(1,1) PRIMARY KEY,
	TeamCode NVARCHAR(100) NOT NULL,
	TeamName NVARCHAR(100) NOT NULL,
)


CREATE TABLE dbo.[User] (
    UserId          UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    FullName        NVARCHAR(120) NOT NULL,
    Phone           NVARCHAR(20) UNIQUE,
    Email           NVARCHAR(120) UNIQUE,
	BirthDate		DATE NULL,
	CCCD			NVARCHAR(20) UNIQUE,
	Address			NVARCHAR(255),
	Gender			NVARCHAR(10),
	ImageUrl		NVARCHAR(255),  -- Sau này cần bổ sung thêm default
	PositionId		INT DEFAULT 1,          -- Giám đốc / Phó / KTV / NV nghiệp vụ / Kế toán...
	TeamId			INT DEFAULT 1,         -- Ban giám đốc / Tổ kiểm định / Tổ nghiệp vụ / Tổ kế toán (nếu bạn muốn lưu)
    IsActive        BIT NOT NULL DEFAULT 1,
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE(),
	FOREIGN KEY (PositionId) REFERENCES Position(PositionId)
);

CREATE TABLE User_Role(
	EmployeeID UNIQUEIDENTIFIER,
	RoleId INT,
	PRIMARY KEY (EmployeeID, OperationId),
	FOREIGN KEY (EmployeeID) REFERENCES Employee(EmployeeID) ON DELETE CASCADE,
	FOREIGN KEY (RoleId) REFERENCES Role(RoleId) ON DELETE CASCADE
)

CREATE TABLE dbo.Account (
    UserId          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Username        NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(255) NOT NULL,
    IsLocked        BIT NOT NULL DEFAULT 0,
    FailedCount     INT NOT NULL DEFAULT 0,  -- Số lần thất bại
    LastLoginAt     DATETIME2 NULL,  -- Lần đăng nhập cuối cùng
    CONSTRAINT FK_Account_User FOREIGN KEY (UserId) REFERENCES dbo.[User](UserId) ON DELETE CASCADE
);

CREATE TABLE PasswordRecovery(
	PasswordRecoveryId INT PRIMARY KEY IDENTITY(1,1),
	UserId          UNIQUEIDENTIFIER NOT NULL,
	ResetOtpHash    NVARCHAR(200) NULL,
    ResetOtpExpiresAt DATETIME2 NULL,
    ResetOtpAttemptCount INT NOT NULL CONSTRAINT DF_Users_ResetOtpAttemptCount DEFAULT(0),
	FOREIGN KEY (UserId) REFERENCES [User](UserId) ON DELETE CASCADE
)

CREATE TABLE dbo.UserRole (
    UserId  UNIQUEIDENTIFIER NOT NULL,
    RoleId  INT NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES dbo.[User](UserId) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES dbo.Role(RoleId) ON DELETE CASCADE
);

-- Gán KTV được phép làm công đoạn nào
CREATE TABLE dbo.UserStage (
    UserId  UNIQUEIDENTIFIER NOT NULL,
    StageId INT NOT NULL,
    PRIMARY KEY (UserId, StageId),
    FOREIGN KEY (UserId) REFERENCES dbo.[User](UserId) ON DELETE CASCADE,
    FOREIGN KEY (StageId) REFERENCES dbo.Stage(StageId) ON DELETE CASCADE
);

-- =========================
-- 2) NHÓM CHỦ XE PHƯƠNG TIỆN
-- =========================
CREATE TABLE dbo.Owner (
    OwnerId         INT IDENTITY(1,1) PRIMARY KEY,
    OwnerType       NVARCHAR(20) NOT NULL DEFAULT N'PERSON',  -- PERSON / COMPANY
    FullName        NVARCHAR(150) NOT NULL,
    CCCD            NVARCHAR(30) NULL,  
    Phone           NVARCHAR(20) NULL,
    Email           NVARCHAR(120) NULL,
    Address         NVARCHAR(255) NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

CREATE TABLE dbo.Vehicle(
    VehicleId       INT IDENTITY(1,1) PRIMARY KEY,
    PlateNo         NVARCHAR(20) NOT NULL UNIQUE,     -- biển số
	-- Nhóm phương tiện 
    Classis             NVARCHAR(50) NULL, -- Số khung
    EngineNo        NVARCHAR(50) NULL,  -- Số máy
    VehicleType     NVARCHAR(50) NOT NULL,            -- ô tô con, tải, khách...
    Brand           NVARCHAR(60) NULL,
    Model           NVARCHAR(60) NULL,
    ManufactureYear INT NULL,
    FuelType        NVARCHAR(30) NULL,                -- Xăng/Dầu/Điện/Hybrid...
    Seats           INT NULL,
    GrossWeightKg   INT NULL,
    OwnerId         INT NOT NULL,
	-- Quốc gia
    -- IsActive        BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (OwnerId) REFERENCES dbo.Owner(OwnerId)
);

-- Thêm bảng specification {}



-- Thêm bảng cải tạo

CREATE INDEX IX_Vehicle_OwnerId ON dbo.Vehicle(OwnerId);


-- =========================
-- 3) NHÓM CẤU HÌNH DÂY TRUYỀN - CÔNG ĐOẠN - CHỈ TIÊU
-- =========================
CREATE TABLE dbo.Lane (
    LaneId      INT IDENTITY(1,1) PRIMARY KEY,
    LaneCode    NVARCHAR(20) NOT NULL UNIQUE,
    LaneName    NVARCHAR(100) NOT NULL UNIQUE,
    IsActive    BIT NOT NULL DEFAULT 1
);

-- Danh mục công đoạn
CREATE TABLE dbo.Stage (
    StageId     INT IDENTITY(1,1) PRIMARY KEY,
    StageCode   NVARCHAR(30) NOT NULL UNIQUE,  -- EXTERIOR, BRAKE, EMISSION...
    StageName   NVARCHAR(120) NOT NULL UNIQUE,
    SortOrder   INT NOT NULL
);

-- Danh mục chỉ tiêu trong từng công đoạn
CREATE TABLE dbo.StageItem (
    ItemId          INT IDENTITY(1,1) PRIMARY KEY,
    StageId         INT NOT NULL,
    ItemCode        NVARCHAR(40) NOT NULL,
    ItemName        NVARCHAR(160) NOT NULL,
    Unit            NVARCHAR(20) NULL,
    DataType        NVARCHAR(20) NOT NULL DEFAULT N'NUMBER', -- NUMBER / TEXT / BOOL
    MinValue        DECIMAL(18,4) NULL,
    MaxValue        DECIMAL(18,4) NULL,
    IsRequired      BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (StageId) REFERENCES dbo.Stage(StageId),
    CONSTRAINT UQ_StageItem UNIQUE(StageId, ItemCode)
);

-- =========================
-- 4) NHÓM HỒ SƠ KIỂM ĐỊNH (LÕI NGHIỆP VỤ)
-- Status: 0 Draft, 1 Received, 2 Paid, 3 InProgress, 4 WaitingConclusion, 5 Passed, 6 Failed, 7 Cancelled
-- =========================
CREATE TABLE dbo.Inspection (
    InspectionId        INT IDENTITY(1,1) PRIMARY KEY,
    InspectionCode      NVARCHAR(30) NOT NULL UNIQUE,     -- mã lượt (có thể QR)
    VehicleId           INT NOT NULL,
    OwnerId             INT NOT NULL,
    ParentInspectionId  INT NULL,     -- tái kiểm
    LaneId              INT NULL,     -- dây chuyền gán
    Status              SMALLINT NOT NULL DEFAULT 0,
    CheckInAt           DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    StartedAt           DATETIME2 NULL,
    ConcludedAt         DATETIME2 NULL,
    ConclusionNote      NVARCHAR(255) NULL,
    FinalApprovedBy     UNIQUEIDENTIFIER NULL,     -- user trưởng dây chuyền
    CreatedBy           UNIQUEIDENTIFIER NULL,
    FOREIGN KEY (VehicleId) REFERENCES dbo.Vehicle(VehicleId),
    FOREIGN KEY (OwnerId) REFERENCES dbo.Owner(OwnerId),
    FOREIGN KEY (ParentInspectionId) REFERENCES dbo.Inspection(InspectionId),
    FOREIGN KEY (LaneId) REFERENCES dbo.Lane(LaneId),
    FOREIGN KEY (FinalApprovedBy) REFERENCES dbo.[User](UserId),
    FOREIGN KEY (CreatedBy) REFERENCES dbo.[User](UserId),
    CONSTRAINT CK_Inspection_Status CHECK (Status IN (0,1,2,3,4,5,6,7))
);

CREATE INDEX IX_Inspection_VehicleId ON dbo.Inspection(VehicleId);
CREATE INDEX IX_Inspection_Status ON dbo.Inspection(Status);
CREATE INDEX IX_Inspection_CheckInAt ON dbo.Inspection(CheckInAt);

-- STAGE RUN (1 hồ sơ chạy qua nhiều công đoạn)
-- RunStatus: 0 NotStarted, 1 Running, 2 Done, 3 Skipped
CREATE TABLE dbo.StageRun (
    StageRunId      INT IDENTITY(1,1) PRIMARY KEY,
    InspectionId    INT NOT NULL,
    StageId         INT NOT NULL,
    LaneId          INT NULL,
    StartedBy       UNIQUEIDENTIFIER NULL,
    StartedAt       DATETIME2 NULL,
    EndedAt         DATETIME2 NULL,
    RunStatus       SMALLINT NOT NULL DEFAULT 0,
    Note            NVARCHAR(255) NULL,
    FOREIGN KEY (InspectionId) REFERENCES dbo.Inspection(InspectionId),
    FOREIGN KEY (StageId) REFERENCES dbo.Stage(StageId),
    FOREIGN KEY (LaneId) REFERENCES dbo.Lane(LaneId),
    FOREIGN KEY (StartedBy) REFERENCES dbo.[User](UserId),
    CONSTRAINT UQ_StageRun UNIQUE(InspectionId, StageId),
    CONSTRAINT CK_StageRun_Status CHECK (RunStatus IN (0,1,2,3))
);

CREATE INDEX IX_StageRun_InspectionId ON dbo.StageRun(InspectionId);

--MEASUREMENT (kết quả đo / nhập)
-- Pass: NULL (chưa xét), 0 Fail, 1 Pass
CREATE TABLE dbo.Measurement (
    MeasurementId    INT IDENTITY(1,1) PRIMARY KEY,
    StageRunId       INT NOT NULL,
    ItemId           INT NOT NULL,
    ValueText        NVARCHAR(255) NULL,
    ValueNumber      DECIMAL(18,4) NULL,
    Pass             BIT NULL,
    Source           NVARCHAR(20) NOT NULL DEFAULT N'MANUAL', -- MANUAL / DEVICE
    RecordedBy       UNIQUEIDENTIFIER NULL,
    RecordedAt       DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    FOREIGN KEY (StageRunId) REFERENCES dbo.StageRun(StageRunId),
    FOREIGN KEY (ItemId) REFERENCES dbo.StageItem(ItemId),
    FOREIGN KEY (RecordedBy) REFERENCES dbo.[User](UserId),
    CONSTRAINT UQ_Measurement UNIQUE(StageRunId, ItemId)
);

CREATE INDEX IX_Measurement_StageRunId ON dbo.Measurement(StageRunId);

-- DEFECT (lỗi phát hiện)
-- Severity: 1 Minor, 2 Major, 3 Critical
CREATE TABLE dbo.Defect (
    DefectId        INT IDENTITY(1,1) PRIMARY KEY,
    InspectionId    INT NOT NULL,
    ItemId          INT NULL,
    DefectCode      NVARCHAR(40) NULL,
    Description     NVARCHAR(255) NOT NULL,
    Severity        SMALLINT NOT NULL DEFAULT 2,
    CreatedBy       UNIQUEIDENTIFIER NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    FOREIGN KEY (InspectionId) REFERENCES dbo.Inspection(InspectionId),
    FOREIGN KEY (ItemId) REFERENCES dbo.StageItem(ItemId),
    FOREIGN KEY (CreatedBy) REFERENCES dbo.[User](UserId),
    CONSTRAINT CK_Defect_Severity CHECK (Severity IN (1,2,3))
);

CREATE INDEX IX_Defect_InspectionId ON dbo.Defect(InspectionId);

-- =========================
-- 5) PAYMENT
-- PayStatus: 0 Pending, 1 Paid, 2 Refunded, 3 Cancelled
-- =========================
CREATE TABLE dbo.Payment (
    PaymentId        INT IDENTITY(1,1) PRIMARY KEY,
    InspectionId     INT NOT NULL UNIQUE,
    Amount           DECIMAL(18,2) NOT NULL,
    Method           NVARCHAR(30) NOT NULL,        -- CASH, TRANSFER, QR, POS
    PayStatus        SMALLINT NOT NULL DEFAULT 0,
    ReceiptNo        NVARCHAR(40) NULL,
    PaidAt           DATETIME2 NULL,
    CreatedBy        UNIQUEIDENTIFIER NULL,
    CreatedAt        DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    FOREIGN KEY (InspectionId) REFERENCES dbo.Inspection(InspectionId),
    FOREIGN KEY (CreatedBy) REFERENCES dbo.[User](UserId),
    CONSTRAINT CK_Payment_Status CHECK (PayStatus IN (0,1,2,3))
);

CREATE INDEX IX_Payment_PayStatus ON dbo.Payment(PayStatus);

-- CERTIFICATE (cấp giấy/tem nếu đạt)
CREATE TABLE dbo.Certificate (
    CertificateId    INT IDENTITY(1,1) PRIMARY KEY,
    InspectionId     INT NOT NULL UNIQUE,
    CertNo           NVARCHAR(40) NOT NULL UNIQUE,
    StickerNo        NVARCHAR(40) NULL UNIQUE,
    IssueDate        DATE NOT NULL,
    ExpiryDate       DATE NOT NULL,
    IssuedBy         UNIQUEIDENTIFIER NULL,
    CreatedAt        DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    FOREIGN KEY (InspectionId) REFERENCES dbo.Inspection(InspectionId),
    FOREIGN KEY (IssuedBy) REFERENCES dbo.[User](UserId)
);





-- =========================
-- BONUS: AUDIT LOG
-- =========================
CREATE TABLE dbo.AuditLog (
    AuditId     BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId      UNIQUEIDENTIFIER NULL,
    Action      NVARCHAR(60) NOT NULL,     -- CREATE_INSPECTION, UPDATE_MEASUREMENT...
    Entity      NVARCHAR(60) NULL,
    EntityId    NVARCHAR(60) NULL,
    Detail      NVARCHAR(4000) NULL,
    CreatedAt   DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    FOREIGN KEY (UserId) REFERENCES dbo.[User](UserId)
);

