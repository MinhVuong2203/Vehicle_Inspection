-- =========================
-- 0) DATABASE
-- =========================
CREATE DATABASE VehicleInspectionCenter;
GO
USE VehicleInspectionCenter;
GO

-- =========================
-- 1) AUTH / STAFF
-- =========================
CREATE TABLE dbo.UserAccount (
    UserId          INT IDENTITY(1,1) PRIMARY KEY,
    Username        NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(255) NOT NULL,
    FullName        NVARCHAR(120) NOT NULL,
    Phone           NVARCHAR(20) NOT NULL,
    Email           NVARCHAR(120) NOT NULL,
	Birthday		DATE NULL,
    IsActive        BIT NOT NULL DEFAULT 1,
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE dbo.Role (
    RoleId      INT IDENTITY(1,1) PRIMARY KEY,
    RoleCode    NVARCHAR(30) NOT NULL UNIQUE,   -- ADMIN, RECEPTION, CASHIER, INSPECTOR, SUPERVISOR
    RoleName    NVARCHAR(100) NOT NULL UNIQUE  -- Quản lý, 
);

CREATE TABLE dbo.UserRole (
    UserId  INT NOT NULL,
    RoleId  INT NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES dbo.UserAccount(UserId),
    FOREIGN KEY (RoleId) REFERENCES dbo.Role(RoleId)
);

CREATE TABLE dbo.Staff (
    StaffId     INT IDENTITY(1,1) PRIMARY KEY,
    UserId      INT NOT NULL UNIQUE,
    StaffCode   NVARCHAR(30) NOT NULL UNIQUE,
    Position    NVARCHAR(60) NOT NULL,          -- Tiếp nhận / Thu ngân / KTV / Trưởng dây chuyền...
    IsActive    BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (UserId) REFERENCES dbo.UserAccount(UserId)
);

-- =========================
-- 2) OWNER / VEHICLE
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
-- 3) APPOINTMENT
-- Status: 0 Pending, 1 Confirmed, 2 CheckedIn, 3 Cancelled, 4 NoShow
-- =========================
CREATE TABLE dbo.Appointment (
    AppointmentId   INT IDENTITY(1,1) PRIMARY KEY,
    VehicleId       INT NOT NULL,
    OwnerId         INT NOT NULL,
    ScheduledAt     DATETIME2 NOT NULL,
    Status          SMALLINT NOT NULL DEFAULT 0,
    Note            NVARCHAR(255) NULL,
    CreatedBy       INT NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    FOREIGN KEY (VehicleId) REFERENCES dbo.Vehicle(VehicleId),
    FOREIGN KEY (OwnerId) REFERENCES dbo.Owner(OwnerId),
    FOREIGN KEY (CreatedBy) REFERENCES dbo.UserAccount(UserId),
    CONSTRAINT CK_Appointment_Status CHECK (Status IN (0,1,2,3,4))
);

CREATE INDEX IX_Appointment_ScheduledAt ON dbo.Appointment(ScheduledAt);
CREATE INDEX IX_Appointment_Status ON dbo.Appointment(Status);

-- =========================
-- 4) LANE / STAGE / ITEM
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

CREATE INDEX IX_StageItem_StageId ON dbo.StageItem(StageId);

-- =========================
-- 5) INSPECTION (Hồ sơ kiểm định)
-- Status: 0 Draft, 1 Received, 2 Paid, 3 InProgress, 4 WaitingConclusion, 5 Passed, 6 Failed, 7 Cancelled
-- =========================
CREATE TABLE dbo.Inspection (
    InspectionId        INT IDENTITY(1,1) PRIMARY KEY,
    InspectionCode      NVARCHAR(30) NOT NULL UNIQUE,     -- mã lượt (có thể QR)
    VehicleId           INT NOT NULL,
    OwnerId             INT NOT NULL,
    AppointmentId       INT NULL,
    ParentInspectionId  INT NULL,     -- tái kiểm
    LaneId              INT NULL,     -- dây chuyền gán
    Status              SMALLINT NOT NULL DEFAULT 0,
    CheckInAt           DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    StartedAt           DATETIME2 NULL,
    ConcludedAt         DATETIME2 NULL,
    ConclusionNote      NVARCHAR(255) NULL,
    FinalApprovedBy     INT NULL,     -- user trưởng dây chuyền
    CreatedBy           INT NULL,
    FOREIGN KEY (VehicleId) REFERENCES dbo.Vehicle(VehicleId),
    FOREIGN KEY (OwnerId) REFERENCES dbo.Owner(OwnerId),
    FOREIGN KEY (AppointmentId) REFERENCES dbo.Appointment(AppointmentId),
    FOREIGN KEY (ParentInspectionId) REFERENCES dbo.Inspection(InspectionId),
    FOREIGN KEY (LaneId) REFERENCES dbo.Lane(LaneId),
    FOREIGN KEY (FinalApprovedBy) REFERENCES dbo.UserAccount(UserId),
    FOREIGN KEY (CreatedBy) REFERENCES dbo.UserAccount(UserId),
    CONSTRAINT CK_Inspection_Status CHECK (Status IN (0,1,2,3,4,5,6,7))
);

CREATE INDEX IX_Inspection_VehicleId ON dbo.Inspection(VehicleId);
CREATE INDEX IX_Inspection_Status ON dbo.Inspection(Status);
CREATE INDEX IX_Inspection_CheckInAt ON dbo.Inspection(CheckInAt);

-- =========================
-- 6) STAGE RUN (1 hồ sơ chạy qua nhiều công đoạn)
-- RunStatus: 0 NotStarted, 1 Running, 2 Done, 3 Skipped
-- =========================
CREATE TABLE dbo.StageRun (
    StageRunId      INT IDENTITY(1,1) PRIMARY KEY,
    InspectionId    INT NOT NULL,
    StageId         INT NOT NULL,
    LaneId          INT NULL,
    StartedBy       INT NULL,
    StartedAt       DATETIME2 NULL,
    EndedAt         DATETIME2 NULL,
    RunStatus       SMALLINT NOT NULL DEFAULT 0,
    Note            NVARCHAR(255) NULL,
    FOREIGN KEY (InspectionId) REFERENCES dbo.Inspection(InspectionId),
    FOREIGN KEY (StageId) REFERENCES dbo.Stage(StageId),
    FOREIGN KEY (LaneId) REFERENCES dbo.Lane(LaneId),
    FOREIGN KEY (StartedBy) REFERENCES dbo.UserAccount(UserId),
    CONSTRAINT UQ_StageRun UNIQUE(InspectionId, StageId),
    CONSTRAINT CK_StageRun_Status CHECK (RunStatus IN (0,1,2,3))
);

CREATE INDEX IX_StageRun_InspectionId ON dbo.StageRun(InspectionId);

-- =========================
-- 7) MEASUREMENT (kết quả đo / nhập)
-- Pass: NULL (chưa xét), 0 Fail, 1 Pass
-- =========================
CREATE TABLE dbo.Measurement (
    MeasurementId    INT IDENTITY(1,1) PRIMARY KEY,
    StageRunId       INT NOT NULL,
    ItemId           INT NOT NULL,
    ValueText        NVARCHAR(255) NULL,
    ValueNumber      DECIMAL(18,4) NULL,
    Pass             BIT NULL,
    Source           NVARCHAR(20) NOT NULL DEFAULT N'MANUAL', -- MANUAL / DEVICE
    RecordedBy       INT NULL,
    RecordedAt       DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    FOREIGN KEY (StageRunId) REFERENCES dbo.StageRun(StageRunId),
    FOREIGN KEY (ItemId) REFERENCES dbo.StageItem(ItemId),
    FOREIGN KEY (RecordedBy) REFERENCES dbo.UserAccount(UserId),
    CONSTRAINT UQ_Measurement UNIQUE(StageRunId, ItemId)
);

CREATE INDEX IX_Measurement_StageRunId ON dbo.Measurement(StageRunId);

-- =========================
-- 8) DEFECT (lỗi phát hiện)
-- Severity: 1 Minor, 2 Major, 3 Critical
-- =========================
CREATE TABLE dbo.Defect (
    DefectId        INT IDENTITY(1,1) PRIMARY KEY,
    InspectionId    INT NOT NULL,
    ItemId          INT NULL,
    DefectCode      NVARCHAR(40) NULL,
    Description     NVARCHAR(255) NOT NULL,
    Severity        SMALLINT NOT NULL DEFAULT 2,
    CreatedBy       INT NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    FOREIGN KEY (InspectionId) REFERENCES dbo.Inspection(InspectionId),
    FOREIGN KEY (ItemId) REFERENCES dbo.StageItem(ItemId),
    FOREIGN KEY (CreatedBy) REFERENCES dbo.UserAccount(UserId),
    CONSTRAINT CK_Defect_Severity CHECK (Severity IN (1,2,3))
);

CREATE INDEX IX_Defect_InspectionId ON dbo.Defect(InspectionId);

-- =========================
-- 9) PAYMENT
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
    CreatedBy        INT NULL,
    CreatedAt        DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    FOREIGN KEY (InspectionId) REFERENCES dbo.Inspection(InspectionId),
    FOREIGN KEY (CreatedBy) REFERENCES dbo.UserAccount(UserId),
    CONSTRAINT CK_Payment_Status CHECK (PayStatus IN (0,1,2,3))
);

CREATE INDEX IX_Payment_PayStatus ON dbo.Payment(PayStatus);

-- =========================
-- 10) CERTIFICATE (cấp giấy/tem nếu đạt)
-- =========================
CREATE TABLE dbo.Certificate (
    CertificateId    INT IDENTITY(1,1) PRIMARY KEY,
    InspectionId     INT NOT NULL UNIQUE,
    CertNo           NVARCHAR(40) NOT NULL UNIQUE,
    StickerNo        NVARCHAR(40) NULL UNIQUE,
    IssueDate        DATE NOT NULL,
    ExpiryDate       DATE NOT NULL,
    IssuedBy         INT NULL,
    CreatedAt        DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    FOREIGN KEY (InspectionId) REFERENCES dbo.Inspection(InspectionId),
    FOREIGN KEY (IssuedBy) REFERENCES dbo.UserAccount(UserId)
);

-- =========================
-- 11) DEVICE (tuỳ chọn, để mô phỏng agent)
-- =========================
CREATE TABLE dbo.Device (
    DeviceId     INT IDENTITY(1,1) PRIMARY KEY,
    DeviceCode   NVARCHAR(40) NOT NULL UNIQUE,
    DeviceType   NVARCHAR(50) NOT NULL,    -- BRAKE, EMISSION, HEADLIGHT...
    LaneId       INT NULL,
    Endpoint     NVARCHAR(255) NULL,
    IsActive     BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (LaneId) REFERENCES dbo.Lane(LaneId)
);

-- =========================
-- 12) AUDIT LOG
-- =========================
CREATE TABLE dbo.AuditLog (
    AuditId     BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId      INT NULL,
    Action      NVARCHAR(60) NOT NULL,     -- CREATE_INSPECTION, UPDATE_MEASUREMENT...
    Entity      NVARCHAR(60) NULL,
    EntityId    NVARCHAR(60) NULL,
    Detail      NVARCHAR(4000) NULL,
    CreatedAt   DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    FOREIGN KEY (UserId) REFERENCES dbo.UserAccount(UserId)
);

CREATE INDEX IX_AuditLog_CreatedAt ON dbo.AuditLog(CreatedAt);
