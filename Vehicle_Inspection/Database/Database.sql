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

INSERT INTO dbo.Position (PoitionCode, PositionName)
VALUES  
		('BV',   N'Bảo vệ / an ninh'),	
		('CSKH', N'Lễ tân / CSKH'),
		('GSV',  N'Giám sát viên'),
		('TTDC', N'Tổ trưởng dây chuyền'),
		('KTV',  N'Kỹ thuật viên'),
		('HS',   N'Nhân viên hồ sơ'),
		('TN',   N'Thu ngân'),
		('KT',   N'Kế toán'),
		('TB',   N'Nhân viên thiết bị'),
		('IT',   N'Quản trị hệ thống'),
		('GD',   N'Giám đốc'),
		('PGD',  N'Phó Giám đốc');
		

CREATE TABLE Team(
	TeamId  INT IDENTITY(1,1) PRIMARY KEY,
	TeamCode NVARCHAR(100) NOT NULL,
	TeamName NVARCHAR(100) NOT NULL,
)

INSERT INTO dbo.Team (TeamCode, TeamName)
VALUES
	('AN',   N'Tổ an ninh'),
	('DC1',  N'Dây chuyền 1'),
	('DC2',  N'Dây chuyền 2'),
	('HS',   N'Tổ hồ sơ - tiếp nhận'),
	('TC',   N'Tổ thu ngân'),
	('KT',   N'Tổ kế toán - tài chính'),
	('TBHC', N'Tổ thiết bị - hiệu chuẩn'),
	('IT',   N'Tổ CNTT'),
	('BGD',  N'Ban giám đốc');
	


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
	[Level]			NVARCHAR(50),
    IsActive        BIT NOT NULL DEFAULT 1,
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE(),
	FOREIGN KEY (PositionId) REFERENCES Position(PositionId),
	FOREIGN KEY (TeamId) REFERENCES Team(TeamId)
);

DROP TABLE User_Role
DROP TABLE PasswordRecovery
DROP TABLE Account
DROP TABLE [User]


-- Insert USERS theo PositionCode + TeamCode (an toàn, không phụ thuộc PositionId/TeamId mặc định)

INSERT INTO [User](FullName, Phone, Email, BirthDate, CCCD, Address, Gender, ImageUrl, PositionId, TeamId, [Level], IsActive)
SELECT v.FullName, v.Phone, v.Email, v.BirthDate, v.CCCD, v.Address, v.Gender, v.ImageUrl,
       p.PositionId, t.TeamId, v.[Level], v.IsActive
FROM (VALUES
-- =========================
-- Ban giám đốc (BGD)
-- =========================
(N'Nguyễn Văn Hùng',  N'0901000001', N'hung.gd@ttdk.local',  CONVERT(date,'1980-05-12'), N'079080000001', N'TP.HCM', N'Nam', NULL, N'GD',  N'BGD', N'Senior', 1),
(N'Trần Thị Mai',     N'0901000002', N'mai.pgd@ttdk.local',  CONVERT(date,'1985-09-21'), N'079085000002', N'TP.HCM', N'Nữ',  NULL, N'PGD', N'BGD', N'Senior', 1),

-- =========================
-- CSKH / Lễ tân
-- =========================
(N'Phạm Thu Ngân',    N'0901100001', N'ngan.cskh@ttdk.local',CONVERT(date,'1999-03-10'), N'079099000001', N'TP.HCM', N'Nữ',  NULL, N'CSKH', N'HS', N'Junior', 1),

-- =========================
-- Hồ sơ - tiếp nhận (HS)
-- =========================
(N'Phạm Ngọc Lan',    N'0902000001', N'lan.hoso@ttdk.local', CONVERT(date,'1996-03-08'), N'079096000001', N'TP.HCM', N'Nữ',  NULL, N'HS', N'HS', N'Mid', 1),
(N'Võ Thanh Tâm',     N'0902000002', N'tam.hoso@ttdk.local', CONVERT(date,'1995-07-18'), N'079095000002', N'TP.HCM', N'Nam', NULL, N'HS', N'HS', N'Mid', 1),

-- =========================
-- Thu ngân (TC)
-- =========================
(N'Đặng Thị Hòa',     N'0903000001', N'hoa.thungan@ttdk.local',CONVERT(date,'1994-12-02'),N'079094000001', N'TP.HCM', N'Nữ', NULL, N'TN', N'TC', N'Mid', 1),

-- =========================
-- Kế toán (KT)
-- =========================
(N'Nguyễn Quốc Bảo',  N'0903000002', N'bao.ketoan@ttdk.local',CONVERT(date,'1992-06-25'),N'079092000002', N'TP.HCM', N'Nam',NULL, N'KT', N'KT', N'Senior', 1),

-- =========================
-- Giám sát / Tổ trưởng (DC1)
-- =========================
(N'Hoàng Văn Đức',    N'0904000001', N'duc.gsv@ttdk.local',   CONVERT(date,'1988-04-14'),N'079088000001', N'TP.HCM', N'Nam',NULL, N'GSV',  N'DC1', N'Senior', 1),
(N'Ngô Hải Long',     N'0904000002', N'long.ttdc@ttdk.local', CONVERT(date,'1990-01-09'),N'079090000002', N'TP.HCM', N'Nam',NULL, N'TTDC', N'DC1', N'Senior', 1),

-- =========================
-- Kỹ thuật viên - Dây chuyền 1 (DC1)
-- =========================
(N'Nguyễn Động Cơ',   N'0905000001', N'dongco.ktv@ttdk.local',CONVERT(date,'1993-08-16'),N'079093000001', N'TP.HCM', N'Nam',NULL, N'KTV', N'DC1', N'Mid', 1),
(N'Lê Khí Thải',      N'0905000002', N'khithai.ktv@ttdk.local',CONVERT(date,'1997-02-20'),N'079097000002',N'TP.HCM', N'Nam',NULL, N'KTV', N'DC1', N'Junior', 1),
(N'Phạm Văn Phanh',   N'0905000003', N'phanh.ktv@ttdk.local', CONVERT(date,'1994-09-30'),N'079094000003', N'TP.HCM', N'Nam',NULL, N'KTV', N'DC1', N'Mid', 1),

-- =========================
-- Kỹ thuật viên - Dây chuyền 2 (DC2)
-- =========================
(N'Nguyễn Khung Sườn',N'0905000004', N'khungsuon.ktv@ttdk.local',CONVERT(date,'1995-10-10'),N'079095000004',N'TP.HCM', N'Nam',NULL, N'KTV', N'DC2', N'Mid', 1),
(N'Huỳnh Văn Đèn',    N'0905000005', N'den.ktv@ttdk.local',   CONVERT(date,'1996-01-05'),N'079096000005', N'TP.HCM', N'Nam',NULL, N'KTV', N'DC2', N'Junior', 1),
(N'Võ Khung Gầm',     N'0905000006', N'khunggam.ktv@ttdk.local',CONVERT(date,'1992-03-19'),N'079092000006',N'TP.HCM', N'Nam',NULL, N'KTV', N'DC2', N'Senior', 1),

-- =========================
-- Thiết bị - hiệu chuẩn (TBHC)
-- =========================
(N'Trần Quốc Thiết',  N'0906000001', N'thietbi@ttdk.local',   CONVERT(date,'1989-07-07'),N'079089000001', N'TP.HCM', N'Nam',NULL, N'TB', N'TBHC', N'Mid', 1),

-- =========================
-- CNTT (IT)
-- =========================
(N'Phan Minh IT',     N'0906000002', N'itadmin@ttdk.local',   CONVERT(date,'1998-05-29'),N'079098000002', N'TP.HCM', N'Nam',NULL, N'IT', N'IT', N'Senior', 1),

-- =========================
-- An ninh / bảo vệ (AN)
-- =========================
(N'Bùi Văn Bảo Vệ',   N'0906000003', N'baove@ttdk.local',     CONVERT(date,'1979-12-12'),N'079079000003', N'TP.HCM', N'Nam',NULL, N'BV', N'AN', N'Mid', 1)
) v(FullName, Phone, Email, BirthDate, CCCD, Address, Gender, ImageUrl, PositionCode, TeamCode, [Level], IsActive)
JOIN dbo.Position p ON p.PoitionCode = v.PositionCode
JOIN dbo.Team t     ON t.TeamCode    = v.TeamCode;



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

CREATE TABLE dbo.User_Role (
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
    VehicleId               INT IDENTITY(1,1) PRIMARY KEY,
    
    -- THÔNG TIN CƠ BẢN
    PlateNo                 NVARCHAR(20) NOT NULL,              -- Biển đăng ký (Registration plate)
    InspectionNo            NVARCHAR(50) NULL,                         -- Số quản lý phương tiện (Vehicle inspection N°)
    
    -- PHÂN LOẠI
    VehicleGroup            NVARCHAR(100) NULL,                        -- Nhóm phương tiện (Vehicle's group)
    VehicleType             NVARCHAR(100) NULL,                        -- Loại phương tiện (Vehicle's type)
    
    -- NĂNG LƯỢNG & MỤC ĐÍCH SỬ DỤNG
    EnergyType              NVARCHAR(50) NULL,                         -- Sử dụng năng lượng sạch, xanh, thân thiện môi trường
    IsCleanEnergy           BIT NULL DEFAULT 0,                        -- Clean, green energy vehicle
    UsagePermission         NVARCHAR(20) NULL,                         -- Cho phép tự động lái / Một phần / Toàn phần
                                                                       -- (Drive automation: Partially / Fully)
    
    -- THƯƠNG HIỆU & MODEL
    Brand                   NVARCHAR(100) NULL,                        -- Nhãn hiệu, tên thương mại (Trademark, Commercial name)
    Model                   NVARCHAR(100) NULL,                        -- Mã kiểu loại (Model code)
    
    -- THÔNG SỐ ĐỘNG CƠ & KHUNG XE
    EngineNo                NVARCHAR(50) NULL,                         -- Số động cơ (Engine N°)
    Chassis                 NVARCHAR(50) NULL,                         -- Số khung (Chassis N°)
    
    -- XUẤT XỨ
    ManufactureYear         INT NULL,                                  -- Năm (Production year)
    ManufactureCountry      NVARCHAR(100) NULL,                        -- Nước sản xuất (Country)
    LifetimeLimitYear       INT NULL,                                  -- Niên hạn sử dụng (Lifetime Limit in)
    
    -- CẢI TẠO
    HasCommercialModification BIT NULL DEFAULT 0,                      -- Có kinh doanh vận tải (Commercial use)
    HasModification         BIT NULL DEFAULT 0,                        -- Có cải tạo (Modification)
    
    -- QUAN HỆ CHỦ XE
    OwnerId                 INT NOT NULL,
    
    -- TIMESTAMPS
    CreatedAt               DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt               DATETIME2 NULL,
    CreatedBy               UNIQUEIDENTIFIER NULL,
    UpdatedBy               UNIQUEIDENTIFIER NULL,
    
    FOREIGN KEY (OwnerId) REFERENCES dbo.Owner(OwnerId),
    FOREIGN KEY (CreatedBy) REFERENCES dbo.[User](UserId),
    FOREIGN KEY (UpdatedBy) REFERENCES dbo.[User](UserId),
	CONSTRAINT UQ_Vehicle_PlateNo UNIQUE (PlateNo)
);

CREATE INDEX IX_Vehicle_PlateNo ON dbo.Vehicle(PlateNo);
CREATE INDEX IX_Vehicle_OwnerId ON dbo.Vehicle(OwnerId);
CREATE INDEX IX_Vehicle_InspectionNo ON dbo.Vehicle(InspectionNo);
-- Thêm bảng specification {}
CREATE TABLE dbo.Specification (
    SpecificationId         INT IDENTITY(1,1) PRIMARY KEY,
    PlateNo                 NVARCHAR(20) NOT NULL UNIQUE,  -- Tham chiếu đến Vehicle.PlateNo
    
    -- KÍCH THƯỚC - THÔNG SỐ KỸ THUẬT (SPECIFICATIONS)
    WheelFormula            NVARCHAR(50) NULL,             -- Công thức bánh xe: 4x2, 6x4
    WheelTread              INT NULL,                      -- Vết bánh xe (mm)
    
    -- Kích thước bao (Overall dimensions)
    OverallLength           INT NULL,                      -- Chiều dài (mm)
    OverallWidth            INT NULL,                      -- Chiều rộng (mm)
    OverallHeight           INT NULL,                      -- Chiều cao (mm)
    
    -- Kích thước lòng bao thùng xe
    CargoInsideLength       INT NULL,                      -- Dài (mm)
    CargoInsideWidth        INT NULL,                      -- Rộng (mm)
    CargoInsideHeight       INT NULL,                      -- Cao (mm)
    
    -- Khoảng cách trục (Wheel base)
    Wheelbase               INT NULL,                      -- (mm)
    
    -- KHỐI LƯỢNG (WEIGHT)
    KerbWeight              DECIMAL(10,2) NULL,            -- Khối lượng bản thân (kg)
    AuthorizedCargoWeight   DECIMAL(10,2) NULL,            -- Khối lượng hàng CC theo TK/CP-N (kg)
    AuthorizedTowedWeight   DECIMAL(10,2) NULL,            -- Khối lượng kéo theo TK/CP-N (kg)
    AuthorizedTotalWeight   DECIMAL(10,2) NULL,            -- Khối lượng toàn bộ theo TK/CP-N (kg)
    
    -- Số người cho phép chở
    SeatingCapacity         INT NULL,                      -- Chở ngồi
    StandingCapacity        INT NULL,                      -- Chở đứng
    LyingCapacity           INT NULL,                      -- Chở nằm
    
    -- ĐỘNG CƠ (ENGINE)
    EngineType              NVARCHAR(100) NULL,            -- Loại động cơ
    EnginePosition          NVARCHAR(50) NULL,             -- Vị trí đặt động cơ
    EngineModel             NVARCHAR(50) NULL,             -- Ký hiệu động cơ
    EngineDisplacement      INT NULL,                      -- Thể tích làm việc (cm³)
    MaxPower                DECIMAL(10,2) NULL,            -- Công suất lớn nhất (kW)
    MaxPowerRPM             INT NULL,                      -- Tốc độ quay tại công suất max (rpm)
    FuelType                NVARCHAR(50) NULL,             -- Loại nhiên liệu
    
    -- ĐỘNG CƠ ĐIỆN (MOTOR)
    MotorType               NVARCHAR(100) NULL,            -- Loại động cơ điện
    NumberOfMotors          INT NULL,                      -- Số lượng động cơ điện
    MotorModel              NVARCHAR(50) NULL,             -- Ký hiệu động cơ điện
    TotalMotorPower         DECIMAL(10,2) NULL,            -- Tổng công suất (kW)
    MotorVoltage            DECIMAL(10,2) NULL,            -- Điện áp (V)
    
    -- ẮC QUY (BATTERY)
    BatteryType             NVARCHAR(100) NULL,            -- Loại ắc quy
    BatteryVoltage          DECIMAL(10,2) NULL,            -- Điện áp (V)
    BatteryCapacity         DECIMAL(10,2) NULL,            -- Dung lượng (kWh)
    
    -- LỐP XE (TIRES)
    TireCount               INT NULL,                      -- Số lượng lốp
    TireSize                NVARCHAR(50) NULL,             -- Cỡ lốp
    TireAxleInfo            NVARCHAR(100) NULL,            -- Thông tin trục
    
    -- THÔNG TIN KIỂM ĐỊNH
    InspectionReportNo      NVARCHAR(50) NULL,             -- Số phiếu kiểm định
    IssuedDate              DATE NULL,                     -- Ngày cấp
    InspectionCenter        NVARCHAR(200) NULL,            -- Cơ sở đăng kiểm
    
    -- VỊ TRÍ THIẾT BỊ
    ImagePosition           NVARCHAR(100) NULL,            -- Vị trí hình ảnh
    
    -- TRANG THIẾT BỊ
    HasTachograph           BIT NULL DEFAULT 0,            -- Có thiết bị giám sát hành trình
    HasDriverCamera         BIT NULL DEFAULT 0,            -- Có camera ghi nhận lái xe
    NotIssuedStamp          BIT NULL DEFAULT 0,            -- PT không được cấp tem
    
    -- GHI CHÚ
    Notes                   NVARCHAR(1000) NULL,
    
    -- TIMESTAMPS
    CreatedAt               DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt               DATETIME2 NULL,
    CreatedBy               UNIQUEIDENTIFIER NULL,
    UpdatedBy               UNIQUEIDENTIFIER NULL,
    
    FOREIGN KEY (PlateNo) REFERENCES dbo.Vehicle(PlateNo) ON DELETE CASCADE,
    FOREIGN KEY (CreatedBy) REFERENCES dbo.[User](UserId),
    FOREIGN KEY (UpdatedBy) REFERENCES dbo.[User](UserId)
);

CREATE INDEX IX_Specification_PlateNo ON dbo.Specification(PlateNo);
CREATE INDEX IX_Specification_InspectionReportNo ON dbo.Specification(InspectionReportNo);

DROP TABLE Specification
DROP TABLE Vehicle

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

