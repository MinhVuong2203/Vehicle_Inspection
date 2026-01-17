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

INSERT INTO Role(RoleCode, RoleAcronym, RoleName, RoleIcon, RoleHref)
VALUES ('LOGIN', N'Đăng nhập', N'Đăng nhập hệ thống', 'fa-solid fa-arrow-right-to-bracket', ''),
	   ('EMPLOYEE', N'Nhân sự', N'Quản lý nhân sự', 'fa-regular fa-address-book', 'employee'),
	   ('RECEIVE PROFILE', N'Hồ sơ', N'Tiếp nhận hồ sơ', 'fa-regular fa-address-card', 'receive-profile'),
	   ('INSPECTION', N'Kiểm định', N'Tạo lượt kiểm định', 'fa-solid fa-magnifying-glass', 'inspection'),
	   ('TOLL', N'Thu phí', N'Thu phí, in biên nhận', 'fa-solid fa-coins', 'toll'),
	   ('RESULT', N'Kết quả', N'Nhập kết quả kiểm định theo công đoạn', 'fa-regular fa-pen-to-square', 'result'),
	   ('CONCLUSION', N'Kết luận', N'Chốt kết luận đạt/không đạt', 'fa-regular fa-handshake', 'conclusion'),
	   ('REPORT', N'Xem báo cáo', N'Xem báo cáo', 'fa-solid fa-chart-pie', 'report')


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
    Phone           NVARCHAR(20)  NOT NULL,
    Email           NVARCHAR(120) NOT NULL,
	BirthDate		DATE NULL,
	CCCD			NVARCHAR(20)  NOT NULL,
	[Address] NVARCHAR(100),
	Ward NVARCHAR(100),
	Province NVARCHAR(100),
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
ALTER TABLE [User] ADD CONSTRAINT UQ_User_Phone UNIQUE(Phone);
ALTER TABLE [User] ADD CONSTRAINT UQ_User_CCCD UNIQUE(CCCD);
ALTER TABLE [User] ADD CONSTRAINT UQ_User_Email UNIQUE(Email);

ALTER TABLE [User] ADD Address NVARCHAR(100);
ALTER TABLE [User] ADD Ward NVARCHAR(100);
ALTER TABLE [User] ADD Province NVARCHAR(100);



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
    Username        NVARCHAR(50) NULL,
    PasswordHash    NVARCHAR(255) NULL,
    IsLocked        BIT NOT NULL DEFAULT 0,
    FailedCount     INT NOT NULL DEFAULT 0,  -- Số lần thất bại
    LastLoginAt     DATETIME2 NULL,  -- Lần đăng nhập cuối cùng
    CONSTRAINT FK_Account_User FOREIGN KEY (UserId) REFERENCES dbo.[User](UserId) ON DELETE CASCADE
);
-- Tạo unique index có điều kiện
CREATE UNIQUE INDEX UX_Account_Username_NotNull
ON dbo.Account (Username)
WHERE Username IS NOT NULL;

--- Lấy ID thực tế của thằng này nhé !!!
INSERT INTO Account(UserId, Username, PasswordHash)
VALUES ('C957DE92-03F6-4DFF-A90F-948A75956684', 'PhanMinh', 'PhanMinh@123')



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
    OwnerId         UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    OwnerType       NVARCHAR(20) NOT NULL DEFAULT N'PERSON',  -- PERSON / COMPANY
    FullName        NVARCHAR(150) NOT NULL,
	CompanyName         NVARCHAR(200) NULL,
    TaxCode             NVARCHAR(30)  NULL, -- MST
    CCCD            NVARCHAR(30) NULL UNIQUE,  
    Phone           NVARCHAR(20) NULL UNIQUE,
    Email           NVARCHAR(120) NULL,
    Address         NVARCHAR(255) NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
-- MST chỉ unique với công ty
CREATE UNIQUE INDEX UX_Owner_TaxCode_Company
ON dbo.Owner(TaxCode)
WHERE OwnerType = N'COMPANY' AND TaxCode IS NOT NULL;
GO



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
    OwnerId                 UNIQUEIDENTIFIER NOT NULL,
    
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
   -- InspectionReportNo      NVARCHAR(50) NULL,             -- Số phiếu kiểm định
    --IssuedDate              DATE NULL,                     -- Ngày cấp
    --InspectionCenter        NVARCHAR(200) NULL,            -- Cơ sở đăng kiểm
    
    -- VỊ TRÍ THIẾT BỊ
    ImagePosition           NVARCHAR(100) NULL,            -- Vị trí hình ảnh
    
    -- TRANG THIẾT BỊ đẩy xuống kia 5.3
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
-- 1. Bảng Lane
CREATE TABLE dbo.Lane (
    LaneId      INT IDENTITY(1,1) PRIMARY KEY,
    LaneCode    NVARCHAR(20) NOT NULL UNIQUE,
    LaneName    NVARCHAR(100) NOT NULL,
    IsActive    BIT NOT NULL DEFAULT 1
);

-- 2.Bảng Stage 
CREATE TABLE dbo.Stage (
    StageId     INT IDENTITY(1,1) PRIMARY KEY,
    StageCode   NVARCHAR(30) NOT NULL UNIQUE,
    StageName   NVARCHAR(120) NOT NULL, 
    -- SortOrder   INT NOT NULL,
    IsActive    BIT DEFAULT 1         
);

-- 3.Bảng LaneStage
CREATE TABLE dbo.LaneStage (
    LaneStageId INT IDENTITY(1,1) PRIMARY KEY,
    LaneId      INT NOT NULL,
    StageId     INT NOT NULL,
    SortOrder   INT NOT NULL,   
    IsRequired  BIT DEFAULT 1,        -- Bắt buộc hay tùy chọn
    IsActive    BIT DEFAULT 1,
    
    FOREIGN KEY (LaneId) REFERENCES dbo.Lane(LaneId),
    FOREIGN KEY (StageId) REFERENCES dbo.Stage(StageId),
    CONSTRAINT UQ_LaneStage UNIQUE (LaneId, StageId)
);
CREATE INDEX IX_LaneStage_LaneId ON dbo.LaneStage(LaneId);

-- 4.Bảng StageItem
CREATE TABLE dbo.StageItem (
    ItemId      INT IDENTITY(1,1) PRIMARY KEY,
    StageId     INT NOT NULL,
    ItemCode    NVARCHAR(40) NOT NULL,
    ItemName    NVARCHAR(160) NOT NULL,
    Unit        NVARCHAR(20) NULL,
    DataType    NVARCHAR(20) NOT NULL 
        CHECK (DataType IN ('NUMBER','TEXT','BOOL')),
    SortOrder   INT NULL,           
    Description NVARCHAR(500) NULL,  
    IsRequired  BIT NOT NULL DEFAULT 1,
    
    FOREIGN KEY (StageId) REFERENCES dbo.Stage(StageId),
    CONSTRAINT UQ_StageItem UNIQUE(StageId, ItemCode)
);

-- 5.Bảng VehicleType (Loại xe đề sắp tiêu chuẩn đánh già)
CREATE TABLE dbo.VehicleType (
    VehicleTypeId INT IDENTITY(1,1) PRIMARY KEY,
    TypeCode      NVARCHAR(20) NOT NULL UNIQUE,
    TypeName      NVARCHAR(100) NOT NULL,
    Description   NVARCHAR(500) NULL,
    IsActive      BIT DEFAULT 1
);

-- 6.Bảng StageItemThreshold (Tiêu chuẩn đánh giá)
CREATE TABLE dbo.StageItemThreshold (
    ThresholdId     INT IDENTITY(1,1) PRIMARY KEY,
    ItemId          INT NOT NULL,
    VehicleTypeId   INT NOT NULL,
    MinValue        DECIMAL(18,4) NULL,
    MaxValue        DECIMAL(18,4) NULL,
    PassCondition   NVARCHAR(200) NULL,  -- VD: "> 50 AND < 100"
    AllowedValues   NVARCHAR(500) NULL,  -- VD: "ĐẠT;KHÔNG ĐẠT;N/A"
    FailAction      NVARCHAR(20) NULL    -- STOP / WARN / CONTINUE
        CHECK (FailAction IN ('STOP','WARN','CONTINUE')),
    IsActive        BIT DEFAULT 1,
    EffectiveDate   DATE DEFAULT GETDATE(),
    
    FOREIGN KEY (ItemId) REFERENCES dbo.StageItem(ItemId),
    FOREIGN KEY (VehicleTypeId) REFERENCES dbo.VehicleType(VehicleTypeId),
    CONSTRAINT UQ_ItemVehicleDate 
    UNIQUE (ItemId, VehicleTypeId, EffectiveDate)
);

-- =========================
-- 4) NHÓM HỒ SƠ KIỂM ĐỊNH (LÕI NGHIỆP VỤ)
-- Status: 0 Draft, 1 Received, 2 Paid, 3 InProgress, 4 WaitingConclusion, 5 Passed, 6 Failed, 7 Cancelled
-- =========================
-- 4.1) Bảng Inspection (Hồ sơ kiểm định chính)
-- Trạng thái chính của hồ sơ theo quy trình thực tế
CREATE TABLE dbo.Inspection (
    InspectionId INT IDENTITY(1,1) PRIMARY KEY,
    InspectionCode NVARCHAR(30) NOT NULL UNIQUE,-- Mã lượt kiểm định
    
    -- THÔNG TIN PHƯƠNG TIỆN & CHỦ XE
    VehicleId INT NOT NULL,
    OwnerId UNIQUEIDENTIFIER NOT NULL,
    
    -- PHÂN LOẠI KIỂM ĐỊNH
    InspectionType NVARCHAR(20) NOT NULL DEFAULT N'FIRST',  
                        -- FIRST: Đăng kiểm lần đầu
                        -- PERIODIC: Định kỳ
                        -- RE_INSPECTION: Tái kiểm (sau khi sửa)
    --ParentInspectionId INT NULL,-- Liên kết tái kiểm với lượt trước
    
    -- PHÂN DÂY CHUYỀN
     LaneId INT NULL,-- Dây chuyền được gán
    
    -- TRẠNG THÁI QUY TRÌNH 
    Status SMALLINT NOT NULL DEFAULT 0,
    /*
        0: DRAFT           - Nháp (chưa tiếp nhận)
        1: RECEIVED        - Đã tiếp nhận (chờ thu phí)
        2: PAID            - Đã thu phí (chờ vào dây chuyền)
        3: IN_PROGRESS     - Đang kiểm định
        4: COMPLETED       - Hoàn thành kiểm định (chờ kết luận)
        5: PASSED          - Đạt (chờ cấp giấy)
        6: FAILED          - Không đạt (cần sửa chữa)
        7: CERTIFIED       - Đã cấp chứng nhận
        8: CANCELLED       - Hủy bỏ
    */
    
    -- KẾT LUẬN CUỐI CÙNG
    FinalResult INT NULL,                    
    /*
        NULL: Chưa có kết luận
        1: ĐẠT - Tất cả công đoạn đạt
        2: KHÔNG ĐẠT - Có công đoạn không đạt
        3: TẠM ĐÌNH CHỈ - Vi phạm nghiêm trọng
    */
    ConclusionNote NVARCHAR(1000) NULL,-- Ghi chú kết luận
    ConcludedBy UNIQUEIDENTIFIER NULL,-- Giám sát viên/Tổ trưởng kết luận
    ConcludedAt DATETIME2 NULL,
    
    -- THỜI GIAN QUY TRÌNH
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),  -- Tạo hồ sơ
    ReceivedAt DATETIME2 NULL,                   -- Tiếp nhận
    PaidAt DATETIME2 NULL,                   -- Thu phí xong
    StartedAt DATETIME2 NULL,                   -- Bắt đầu kiểm định
    CompletedAt DATETIME2 NULL,                   -- Hoàn thành kiểm định
    CertifiedAt DATETIME2 NULL,                   -- Cấp chứng nhận
    
    -- NGƯỜI THỰC HIỆN
    CreatedBy UNIQUEIDENTIFIER NULL, -- Người tạo (có thể là hệ thống hoặc NV tiếp nhận)
    ReceivedBy UNIQUEIDENTIFIER NULL, -- NV tiếp nhận hồ sơ
    
    -- GHI CHÚ & METADATA
    Notes               NVARCHAR(1000) NULL,-- Ghi chú chung
    -- Priority            SMALLINT DEFAULT 1,-- Mức ưu tiên (1: Thường, 2: Cao, 3: Khẩn cấp)
    IsDeleted           BIT NOT NULL DEFAULT 0,
    
    FOREIGN KEY (VehicleId) REFERENCES dbo.Vehicle(VehicleId),
    FOREIGN KEY (OwnerId) REFERENCES dbo.Owner(OwnerId),
    --FOREIGN KEY (ParentInspectionId) REFERENCES dbo.Inspection(InspectionId),
    FOREIGN KEY (LaneId) REFERENCES dbo.Lane(LaneId),
    FOREIGN KEY (ConcludedBy) REFERENCES dbo.[User](UserId),
    FOREIGN KEY (CreatedBy) REFERENCES dbo.[User](UserId),
    FOREIGN KEY (ReceivedBy) REFERENCES dbo.[User](UserId),
    CONSTRAINT CK_Inspection_Status CHECK (Status BETWEEN 0 AND 8),
    CONSTRAINT CK_Inspection_FinalResult CHECK (FinalResult IN (1,2,3) OR FinalResult IS NULL),
   -- CONSTRAINT CK_Inspection_Priority CHECK (Priority BETWEEN 1 AND 3)
);

CREATE INDEX IX_Inspection_VehicleId ON dbo.Inspection(VehicleId);
CREATE INDEX IX_Inspection_Status ON dbo.Inspection(Status) WHERE IsDeleted = 0;
CREATE INDEX IX_Inspection_ReceivedAt ON dbo.Inspection(ReceivedAt);
CREATE INDEX IX_Inspection_LaneId ON dbo.Inspection(LaneId) WHERE Status IN (3,4);
CREATE INDEX IX_Inspection_InspectionType ON dbo.Inspection(InspectionType);

-- 4.2) Bảng InspectionStage (Chi tiết công đoạn kiểm định)
-- Mỗi hồ sơ sẽ có nhiều công đoạn tương ứng với dây chuyền
CREATE TABLE dbo.InspectionStage (
    InspStageId BIGINT IDENTITY(1,1) PRIMARY KEY,
    InspectionId INT NOT NULL,
    StageId INT NOT NULL,-- Công đoạn (Động cơ, Phanh, Đèn...)
    
    -- PHÂN CÔNG KỸ THUẬT VIÊN
    AssignedUserId UNIQUEIDENTIFIER NULL,-- KTV thực hiện (mapping UserStage)
    
    -- TRẠNG THÁI CÔNG ĐOẠN
    Status INT NOT NULL DEFAULT 0,
    /*
        0: PENDING     - Chờ thực hiện
        1: IN_PROGRESS - Đang thực hiện
        2: COMPLETED   - Hoàn thành
        3: ON_HOLD     - Tạm dừng (chờ thiết bị/sửa chữa)
        4: SKIPPED     - Bỏ qua (không áp dụng)
    */
    
    -- KẾT QUẢ CÔNG ĐOẠN
    StageResult INT NULL,
    /*
        NULL: Chưa có kết quả
        1: ĐẠT - Tất cả chỉ tiêu đạt
        2: KHÔNG ĐẠT - Có chỉ tiêu không đạt
        3: KHUYẾT ĐIỂM - Đạt nhưng có lỗi nhỏ cần lưu ý
    */
    
    
    -- GHI CHÚ
    Notes NVARCHAR(500) NULL,-- Ghi chú của KTV
    
    -- METADATA
    SortOrder INT NOT NULL DEFAULT 0,-- Thứ tự thực hiện
    IsRequired BIT NOT NULL DEFAULT 1,-- Bắt buộc hay không
    
    FOREIGN KEY (InspectionId) REFERENCES dbo.Inspection(InspectionId) ON DELETE CASCADE,
    FOREIGN KEY (StageId) REFERENCES dbo.Stage(StageId),
    FOREIGN KEY (AssignedUserId) REFERENCES dbo.[User](UserId),
    CONSTRAINT CK_InspStage_Status CHECK (Status BETWEEN 0 AND 4),
    CONSTRAINT CK_InspStage_Result CHECK (StageResult IN (1,2,3) OR StageResult IS NULL),
    CONSTRAINT UQ_InspStage UNIQUE (InspectionId, StageId)
);

CREATE INDEX IX_InspStage_InspectionId ON dbo.InspectionStage(InspectionId);
CREATE INDEX IX_InspStage_Status ON dbo.InspectionStage(Status);
CREATE INDEX IX_InspStage_AssignedUserId ON dbo.InspectionStage(AssignedUserId) WHERE Status IN (0,1);

-- 4.3) Bảng InspectionDetail (Kết quả đo chi tiết từng chỉ tiêu)
CREATE TABLE dbo.InspectionDetail (
    DetailId INT IDENTITY(1,1) PRIMARY KEY,
    InspStageId BIGINT NOT NULL,-- Thuộc công đoạn nào
    ItemId INT NOT NULL,-- Chỉ tiêu nào
    
    -- TIÊU CHUẨN (Lấy từ StageItemThreshold theo VehicleType)
    StandardMin DECIMAL(18,4) NULL, -- Giá trị min theo tiêu chuẩn
    StandardMax DECIMAL(18,4) NULL, -- Giá trị max theo tiêu chuẩn
    StandardText NVARCHAR(100) NULL, -- Tiêu chuẩn dạng text (VD: "Bình thường")
    
    -- GIÁ TRỊ ĐO ĐƯỢC
    ActualValue DECIMAL(18,4) NULL,-- Giá trị đo (số)
    ActualText NVARCHAR(100) NULL, -- Giá trị đo (text)
    Unit NVARCHAR(20) NULL,-- Đơn vị (kg, N, lux, %)
    
    -- KẾT QUẢ ĐÁNH GIÁ
    IsPassed BIT NULL,-- Đạt chỉ tiêu này?
    /*
        NULL: Chưa đánh giá
        0: KHÔNG ĐẠT
        1: ĐẠT
    */
    DeviationPercent DECIMAL(10,2) NULL,-- % chênh lệch so với tiêu chuẩn
    
    -- NGUỒN DỮ LIỆU
    DataSource NVARCHAR(20) NOT NULL DEFAULT N'MANUAL',
    /*
        MANUAL: Nhập tay
        DEVICE: Từ thiết bị đo
        VISUAL: Kiểm tra mắt thường
        CALCULATED: Tính toán
    */
    DeviceId NVARCHAR(50) NULL,-- ID thiết bị đo (nếu có)
    
    -- THÔNG TIN GHI NHẬN
    --RecordedBy UNIQUEIDENTIFIER NULL,-- KTV ghi nhận
    RecordedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    
    -- ẢNH CHỤP 
    ImageUrls NVARCHAR(1000) NULL,
    
    -- GHI CHÚ
    Notes NVARCHAR(500) NULL,
    
    FOREIGN KEY (InspStageId) REFERENCES dbo.InspectionStage(InspStageId) ON DELETE CASCADE,
    FOREIGN KEY (ItemId) REFERENCES dbo.StageItem(ItemId),
    --FOREIGN KEY (RecordedBy) REFERENCES dbo.[User](UserId),
    CONSTRAINT UQ_InspDetail UNIQUE (InspStageId, ItemId)
);

CREATE INDEX IX_InspDetail_InspStageId ON dbo.InspectionDetail(InspStageId);
CREATE INDEX IX_InspDetail_IsPassed ON dbo.InspectionDetail(IsPassed) WHERE IsPassed = 0;

-- 4.4) Bảng InspectionDefect (Danh sách lỗi phát hiện)
-- Ghi nhận các lỗi/hư hỏng phát hiện trong quá trình kiểm định
CREATE TABLE dbo.InspectionDefect (
    DefectId BIGINT IDENTITY(1,1) PRIMARY KEY,
    InspectionId INT NOT NULL,
    InspStageId BIGINT NULL,-- Lỗi phát hiện ở công đoạn nào
    ItemId INT NULL,-- Lỗi liên quan chỉ tiêu nào
    
    -- PHÂN LOẠI LỖI
    DefectCategory NVARCHAR(50) NOT NULL,-- Danh mục (VD: "Hệ thống phanh", "Hệ thống đèn")
    DefectCode NVARCHAR(40) NULL,-- Mã lỗi chuẩn (VD: "HTP", "HTD")
    
    -- MÔ TẢ LỖI
    DefectDescription NVARCHAR(1000) NOT NULL,          -- Mô tả chi tiết lỗi
    
    -- MỨC ĐỘ NGHIÊM TRỌNG
    Severity INT NOT NULL DEFAULT 2,
    /*
        1: KHUYẾT ĐIỂM - Nhắc nhở, không ảnh hưởng kết quả
        2: HƯ HỎNG - Không đạt, cần sửa chữa
        3: NGUY HIỂM - Nghiêm trọng, cấm lưu hành
    */
    
    -- HÌNH ẢNH MINH HỌA
    ImageUrls NVARCHAR(1000) NULL,
    
    -- TRẠNG THÁI XỬ LÝ (cho tái kiểm)
    IsFixed BIT NOT NULL DEFAULT 0,-- Đã sửa chữa?
    FixedNote NVARCHAR(500) NULL,-- Ghi chú về việc sửa chữa
    VerifiedBy UNIQUEIDENTIFIER NULL,-- KTV xác nhận đã sửa (tái kiểm)
    
    -- NGƯỜI PHÁT HIỆN
    --CreatedBy UNIQUEIDENTIFIER NULL,-- KTV phát hiện lỗi
    
    FOREIGN KEY (InspectionId) REFERENCES dbo.Inspection(InspectionId) ON DELETE CASCADE,
    FOREIGN KEY (InspStageId) REFERENCES dbo.InspectionStage(InspStageId),
    FOREIGN KEY (ItemId) REFERENCES dbo.StageItem(ItemId),
    --FOREIGN KEY (CreatedBy) REFERENCES dbo.[User](UserId),
    FOREIGN KEY (VerifiedBy) REFERENCES dbo.[User](UserId),
    CONSTRAINT CK_Defect_Severity CHECK (Severity BETWEEN 1 AND 3)
);

CREATE INDEX IX_Defect_InspectionId ON dbo.InspectionDefect(InspectionId);
CREATE INDEX IX_Defect_Severity ON dbo.InspectionDefect(Severity);
CREATE INDEX IX_Defect_IsFixed ON dbo.InspectionDefect(IsFixed) WHERE Severity >= 2;

-- 4.5) Bảng InspectionHistory (Lịch sử thay đổi trạng thái)
-- Audit trail cho việc chuyển trạng thái hồ sơ
--CREATE TABLE dbo.InspectionHistory (
--    HistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
--    InspectionId INT NOT NULL,
    
    -- THAY ĐỔI TRẠNG THÁI
--    FromStatus SMALLINT NULL, -- Trạng thái cũ
--    ToStatus SMALLINT NOT NULL, -- Trạng thái mới
    
    -- NGƯỜI THỰC HIỆN VÀ THỜI GIAN
--    ChangedBy UNIQUEIDENTIFIER NULL,
--    ChangedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    
    -- GHI CHÚ
--    Notes NVARCHAR(500) NULL,-- Lý do thay đổi
    
--    FOREIGN KEY (InspectionId) REFERENCES dbo.Inspection(InspectionId) ON DELETE CASCADE,
--    FOREIGN KEY (ChangedBy) REFERENCES dbo.[User](UserId)
--);

--CREATE INDEX IX_History_InspectionId ON dbo.InspectionHistory(InspectionId);
--CREATE INDEX IX_History_ChangedAt ON dbo.InspectionHistory(ChangedAt);

-- 5) NHÓM THU PHÍ - CHỨNG NHẬN - THIẾT KẾ LẠI

-- 5.1) Bảng FeeSchedule (Bảng giá dịch vụ)
-- Quản lý giá theo loại xe và loại kiểm định
CREATE TABLE dbo.FeeSchedule (
    FeeId INT IDENTITY(1,1) PRIMARY KEY,
    
    -- PHÂN LOẠI PHÍ
    ServiceType NVARCHAR(30) NOT NULL,
    /*
        FIRST_INSPECTION    - Kiểm định lần đầu
        PERIODIC           - Định kỳ
        RE_INSPECTION      - Tái kiểm
    */
    
    VehicleTypeId       INT NULL,-- Loại xe áp dụng hoăc tất cả xe
    
    -- GIÁ PHÍ
    BaseFee DECIMAL(18,2) NOT NULL,-- Phí cơ bản
    CertificateFee DECIMAL(18,2) DEFAULT 0,-- Phí giấy chứng nhận
    StickerFee DECIMAL(18,2) DEFAULT 0,-- Phí tem kiểm định
    TotalFee DECIMAL(18,2) NOT NULL,-- Tổng phí
    
    -- THỜI GIAN ÁP DỤNG
    EffectiveFrom DATE NOT NULL,
    EffectiveTo DATE NULL,-- NULL = vô thời hạn
    
    -- TRẠNG THÁI
    IsActive BIT NOT NULL DEFAULT 1,
    
    CreatedBy UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIME2 NULL,
    
    FOREIGN KEY (VehicleTypeId) REFERENCES dbo.VehicleType(VehicleTypeId),
    CONSTRAINT CK_Fee_Dates CHECK (EffectiveTo IS NULL OR EffectiveTo >= EffectiveFrom),
    CONSTRAINT CK_Fee_Amount CHECK (TotalFee >= 0)
);

CREATE INDEX IX_FeeSchedule_ServiceType ON dbo.FeeSchedule(ServiceType);
CREATE INDEX IX_FeeSchedule_Effective ON dbo.FeeSchedule(EffectiveFrom, EffectiveTo) WHERE IsActive = 1;

-- 5.2) Bảng Payment (Thanh toán)
CREATE TABLE dbo.Payment (
    PaymentId INT IDENTITY(1,1) PRIMARY KEY,
    InspectionId INT NOT NULL,
     
    -- CHI TIẾT PHÍ
    FeeScheduleId INT NULL,-- Tham chiếu bảng giá
    BaseFee DECIMAL(18,2) NOT NULL,-- Phí cơ bản
    CertificateFee DECIMAL(18,2) DEFAULT 0,
    StickerFee DECIMAL(18,2) DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL, -- Tổng tiền
    
    -- PHƯƠNG THỨC THANH TOÁN
    PaymentMethod NVARCHAR(30) NOT NULL,
    /*
        - Tiền mặt
        - Chuyển khoản
    */
    
    -- TRẠNG THÁI THANH TOÁN
    PaymentStatus       SMALLINT NOT NULL DEFAULT 0,
    /*
        0: PENDING   - Chờ thanh toán
        1: PAID      - Đã thanh toán
        2: CANCELLED - Đã hủy
    */
    
    -- THÔNG TIN BIÊN NHẬN
    ReceiptNo NVARCHAR(40) NULL UNIQUE, -- Số biên nhận
    ReceiptPrintCount INT DEFAULT 0, -- Số lần in biên nhận

    -- THỜI GIAN
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    PaidAt DATETIME2 NULL, -- Thời điểm thanh toán
    -- NGƯỜI THỰC HIỆN
    CreatedBy UNIQUEIDENTIFIER NULL,  -- Thu ngân tạo phiếu
    PaidBy UNIQUEIDENTIFIER NULL,  -- Thu ngân nhận tiền
   
    
    -- GHI CHÚ
    Notes NVARCHAR(500) NULL,
    
    
    FOREIGN KEY (InspectionId) REFERENCES dbo.Inspection(InspectionId),
    FOREIGN KEY (FeeScheduleId) REFERENCES dbo.FeeSchedule(FeeId),
    FOREIGN KEY (CreatedBy) REFERENCES dbo.[User](UserId),
    FOREIGN KEY (PaidBy) REFERENCES dbo.[User](UserId),
    CONSTRAINT CK_Payment_Status CHECK (PaymentStatus BETWEEN 0 AND 3),
    CONSTRAINT CK_Payment_Amount CHECK (TotalAmount >= 0),
    CONSTRAINT UQ_Payment_Inspection UNIQUE (InspectionId)  -- Mỗi hồ sơ 1 phiếu thu
);

CREATE INDEX IX_Payment_Status ON dbo.Payment(PaymentStatus); 
CREATE INDEX IX_Payment_PaidAt ON dbo.Payment(PaidAt);
CREATE INDEX IX_Payment_ReceiptNo ON dbo.Payment(ReceiptNo);

-- 5.3) Bảng Certificate (Chứng nhận kiểm định)
-- Cấp cho xe ĐẠT kiểm định
CREATE TABLE dbo.Certificate (
    CertificateId       INT IDENTITY(1,1) PRIMARY KEY,
    InspectionId        INT NOT NULL,
    
    -- SỐ CHỨNG NHẬN
    CertificateNo NVARCHAR(40) NOT NULL UNIQUE, -- Số GCN (VD: 1234567890-01/2024)
    StickerNo NVARCHAR(40) NULL UNIQUE, -- Số tem kiểm định
    
    -- THỜI HẠN
    IssueDate DATE NOT NULL,-- Ngày cấp
    ExpiryDate DATE NOT NULL,-- Ngày hết hạn
    ValidityMonths INT NOT NULL DEFAULT 12,-- Số tháng có hiệu lực
    
    -- TRẠNG THÁI
    Status  SMALLINT NOT NULL DEFAULT 1,
    /*
        1: ACTIVE    - Còn hiệu lực
        2: EXPIRED   - Hết hạn
        3: REVOKED   - Thu hồi
        4: REPLACED  - Thay thế (cấp lại)
    */
    
    -- THÔNG TIN IN ẤN
    PrintTemplate       NVARCHAR(50) DEFAULT N'STANDARD', -- Template in
    PrintCount INT DEFAULT 0,  -- Số lần in
    LastPrintedAt DATETIME2 NULL, -- Lần in cuối
    
    -- THÔNG TIN CẤP PHÁT
    IssuedBy UNIQUEIDENTIFIER NULL,  -- Cán bộ cấp
    IssuedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    
    -- FILE ĐÍNH KÈM
    PdfUrl NVARCHAR(500) NULL,  -- File PDF chứng nhận
    
    -- GHI CHÚ
    Notes NVARCHAR(500) NULL,
    
    FOREIGN KEY (InspectionId) REFERENCES dbo.Inspection(InspectionId),
    FOREIGN KEY (IssuedBy) REFERENCES dbo.[User](UserId),
    CONSTRAINT CK_Certificate_Status CHECK (Status BETWEEN 1 AND 4),
    CONSTRAINT CK_Certificate_Dates CHECK (ExpiryDate > IssueDate),
    CONSTRAINT UQ_Certificate_Inspection UNIQUE (InspectionId)
);
  
    -- THÔNG TIN KIỂM ĐỊNH
   -- InspectionReportNo      NVARCHAR(50) NULL,             -- Số phiếu kiểm định
    --IssuedDate              DATE NULL,                     -- Ngày cấp
    --InspectionCenter        NVARCHAR(200) NULL,            -- Cơ sở đăng kiểm

    ALTER TABLE dbo.Certificate 
	ADD InspectionReportNo  NVARCHAR(50) NULL,					-- Số phiếu kiểm định
		 IssuedDate DATE NULL,									-- Ngày cấp
		 InspectionCenter   NVARCHAR(200) NULL;					-- Cơ sở đăng kiểm


CREATE INDEX IX_Certificate_Status ON dbo.Certificate(Status);
CREATE INDEX IX_Certificate_ExpiryDate ON dbo.Certificate(ExpiryDate) WHERE Status = 1;
CREATE INDEX IX_Certificate_IssueDate ON dbo.Certificate(IssueDate);






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

