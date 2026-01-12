// Dữ liệu mẫu hồ sơ kiểm định theo cấu trúc Database
const inspectionRecords = [
    {
        inspectionId: 1,
        inspectionCode: "KD-2025-001",
        vehicleId: 1,
        plateNo: "29A-12345",
        ownerId: "owner-001",
        ownerName: "Nguyễn Văn An",
        ownerType: "PERSON",
        ownerPhone: "0901234567",
        ownerCCCD: "079080001234",
        ownerAddress: "123 Nguyễn Huệ, Q1, TP.HCM",
        inspectionType: "FIRST",
        laneId: 1,
        laneName: "Dây chuyền 1",
        status: 5, // PASSED
        finalResult: 1, // ĐẠT
        createdAt: "2025-11-15T08:30:00",
        receivedAt: "2025-11-15T09:00:00",
        paidAt: "2025-11-15T09:15:00",
        startedAt: "2025-11-15T10:00:00",
        completedAt: "2025-11-15T14:30:00",
        concludedAt: "2025-11-15T15:00:00",
        receivedBy: "Phạm Ngọc Lan",
        concludedBy: "Hoàng Văn Đức",
        // Thông tin xe
        vehicleGroup: "Xe con",
        vehicleType: "Xe ô tô con",
        brand: "Hyundai i10",
        model: "BA",
        engineNo: "G4FA1234567",
        chassis: "MALBA81AAGA123456",
        manufactureYear: 2020,
        manufactureCountry: "Hàn Quốc",
        // Thông số kỹ thuật
        wheelFormula: "4x2",
        wheelTread: "1520/1520",
        overallDimensions: "4500 x 1750 x 1850",
        wheelbase: "2650",
        kerbWeight: "1250",
        authorizedCargoWeight: "650",
        authorizedTotalWeight: "1900",
        seatingCapacity: "5",
        engineType: "Xăng 4 kỳ",
        engineModel: "G4FA",
        engineDisplacement: "1396",
        maxPower: "73",
        maxPowerRPM: "6300",
        fuelType: "Xăng RON 92",
        tires: "4 lốp 185/65R15 - Trục 1, 2",
        // Các công đoạn kiểm định
        stages: [
            { stageId: 1, stageName: "Kiểm tra ngoại thất", status: 2, result: 1, assignedUser: "Nguyễn Động Cơ" },
            { stageId: 2, stageName: "Kiểm tra động cơ", status: 2, result: 1, assignedUser: "Nguyễn Động Cơ" },
            { stageId: 3, stageName: "Kiểm tra hệ thống phanh", status: 2, result: 1, assignedUser: "Phạm Văn Phanh" },
            { stageId: 4, stageName: "Kiểm tra khí thải", status: 2, result: 1, assignedUser: "Lê Khí Thải" },
            { stageId: 5, stageName: "Kiểm tra hệ thống đèn", status: 2, result: 1, assignedUser: "Huỳnh Văn Đèn" }
        ],
        // Chứng nhận
        certificateNo: "001234567-01/2025",
        stickerNo: "TEM-2025-001234",
        issueDate: "2025-11-15",
        expiryDate: "2026-11-15",
        notes: "Xe đảm bảo an toàn kỹ thuật và bảo vệ môi trường"
    },
    {
        inspectionId: 2,
        inspectionCode: "KD-2025-002",
        vehicleId: 2,
        plateNo: "30F-67890",
        ownerId: "owner-002",
        ownerName: "Trần Thị Bình",
        ownerType: "PERSON",
        ownerPhone: "0907654321",
        ownerCCCD: "079085002345",
        ownerAddress: "456 Lê Lợi, Q1, TP.HCM",
        inspectionType: "PERIODIC",
        laneId: 1,
        laneName: "Dây chuyền 1",
        status: 3, // IN_PROGRESS
        finalResult: null,
        createdAt: "2025-11-20T08:00:00",
        receivedAt: "2025-11-20T08:30:00",
        paidAt: "2025-11-20T08:45:00",
        startedAt: "2025-11-20T09:00:00",
        completedAt: null,
        concludedAt: null,
        receivedBy: "Võ Thanh Tâm",
        concludedBy: null,
        vehicleGroup: "Xe con",
        vehicleType: "Xe ô tô con",
        brand: "Toyota Wigo",
        model: "W1G",
        engineNo: "1KRFE7654321",
        chassis: "MHTW1G3J0JK654321",
        manufactureYear: 2019,
        manufactureCountry: "Indonesia",
        wheelFormula: "4x2",
        wheelTread: "1510/1510",
        overallDimensions: "4400 x 1700 x 1500",
        wheelbase: "2550",
        kerbWeight: "1100",
        authorizedCargoWeight: "500",
        authorizedTotalWeight: "1600",
        seatingCapacity: "5",
        engineType: "Xăng 4 kỳ",
        engineModel: "1KR-FE",
        engineDisplacement: "998",
        maxPower: "51",
        maxPowerRPM: "6000",
        fuelType: "Xăng RON 92",
        tires: "4 lốp 155/80R13 - Trục 1, 2",
        stages: [
            { stageId: 1, stageName: "Kiểm tra ngoại thất", status: 2, result: 1, assignedUser: "Nguyễn Động Cơ" },
            { stageId: 2, stageName: "Kiểm tra động cơ", status: 1, result: null, assignedUser: "Nguyễn Động Cơ" },
            { stageId: 3, stageName: "Kiểm tra hệ thống phanh", status: 0, result: null, assignedUser: "Phạm Văn Phanh" },
            { stageId: 4, stageName: "Kiểm tra khí thải", status: 0, result: null, assignedUser: "Lê Khí Thải" },
            { stageId: 5, stageName: "Kiểm tra hệ thống đèn", status: 0, result: null, assignedUser: "Huỳnh Văn Đèn" }
        ],
        certificateNo: null,
        stickerNo: null,
        issueDate: null,
        expiryDate: null,
        notes: ""
    },
    {
        inspectionId: 3,
        inspectionCode: "KD-2025-003",
        vehicleId: 3,
        plateNo: "51G-11223",
        ownerId: "company-001",
        ownerName: "Lê Minh Cường",
        ownerType: "PERSON",
        ownerPhone: "0909876543",
        ownerCCCD: "079075003456",
        ownerAddress: "789 Võ Văn Tần, Q3, TP.HCM",
        inspectionType: "FIRST",
        laneId: 2,
        laneName: "Dây chuyền 2",
        status: 6, // FAILED
        finalResult: 2, // KHÔNG ĐẠT
        createdAt: "2025-10-25T07:30:00",
        receivedAt: "2025-10-25T08:00:00",
        paidAt: "2025-10-25T08:20:00",
        startedAt: "2025-10-25T09:00:00",
        completedAt: "2025-10-25T15:00:00",
        concludedAt: "2025-10-25T15:30:00",
        receivedBy: "Phạm Ngọc Lan",
        concludedBy: "Hoàng Văn Đức",
        vehicleGroup: "Xe tải",
        vehicleType: "Xe ô tô tải",
        brand: "Hyundai HD240",
        model: "HD240",
        engineNo: "D6CB9876543",
        chassis: "KMFHD2408PA987654",
        manufactureYear: 2018,
        manufactureCountry: "Việt Nam",
        wheelFormula: "6x4",
        wheelTread: "1850/1850",
        overallDimensions: "9500 x 2500 x 3200",
        wheelbase: "4500+1350",
        kerbWeight: "8500",
        authorizedCargoWeight: "15000",
        authorizedTotalWeight: "23500",
        seatingCapacity: "3",
        engineType: "Diesel 4 kỳ",
        engineModel: "D6CB",
        engineDisplacement: "7640",
        maxPower: "191",
        maxPowerRPM: "2200",
        fuelType: "Dầu diesel",
        tires: "10 lốp 10.00R20 - Trục 1,2,3",
        stages: [
            { stageId: 1, stageName: "Kiểm tra ngoại thất", status: 2, result: 1, assignedUser: "Nguyễn Khung Sườn" },
            { stageId: 2, stageName: "Kiểm tra động cơ", status: 2, result: 1, assignedUser: "Nguyễn Động Cơ" },
            { stageId: 3, stageName: "Kiểm tra hệ thống phanh", status: 2, result: 2, assignedUser: "Phạm Văn Phanh" },
            { stageId: 4, stageName: "Kiểm tra khí thải", status: 2, result: 2, assignedUser: "Lê Khí Thải" },
            { stageId: 5, stageName: "Kiểm tra hệ thống đèn", status: 2, result: 1, assignedUser: "Huỳnh Văn Đèn" }
        ],
        defects: [
            { defectId: 1, category: "Hệ thống phanh", description: "Lực phanh trục sau không đạt tiêu chuẩn", severity: 2 },
            { defectId: 2, category: "Khí thải", description: "Nồng độ CO vượt ngưỡng cho phép", severity: 2 }
        ],
        certificateNo: null,
        stickerNo: null,
        issueDate: null,
        expiryDate: null,
        notes: "Xe không đạt, cần sửa chữa hệ thống phanh và khí thải"
    },
    {
        inspectionId: 4,
        inspectionCode: "KD-2025-004",
        vehicleId: 4,
        plateNo: "29B-55667",
        ownerId: "owner-004",
        ownerName: "Phạm Thị Dung",
        ownerType: "PERSON",
        ownerPhone: "0905551234",
        ownerCCCD: "079090004567",
        ownerAddress: "321 Trần Hưng Đạo, Q5, TP.HCM",
        inspectionType: "PERIODIC",
        laneId: 1,
        laneName: "Dây chuyền 1",
        status: 7, // CERTIFIED
        finalResult: 1, // ĐẠT
        createdAt: "2025-09-10T08:00:00",
        receivedAt: "2025-09-10T08:30:00",
        paidAt: "2025-09-10T08:50:00",
        startedAt: "2025-09-10T09:30:00",
        completedAt: "2025-09-10T13:00:00",
        concludedAt: "2025-09-10T13:30:00",
        receivedBy: "Võ Thanh Tâm",
        concludedBy: "Ngô Hải Long",
        vehicleGroup: "Xe con",
        vehicleType: "Xe ô tô con",
        brand: "Honda Civic",
        model: "FC1",
        engineNo: "L15B7123456",
        chassis: "LVHFC16G8ME654987",
        manufactureYear: 2021,
        manufactureCountry: "Thái Lan",
        wheelFormula: "4x2",
        wheelTread: "1540/1540",
        overallDimensions: "4695 x 1810 x 1565",
        wheelbase: "2700",
        kerbWeight: "1520",
        authorizedCargoWeight: "480",
        authorizedTotalWeight: "2000",
        seatingCapacity: "5",
        engineType: "Xăng 4 kỳ",
        engineModel: "L15B7",
        engineDisplacement: "1498",
        maxPower: "89",
        maxPowerRPM: "6000",
        fuelType: "Xăng RON 95",
        tires: "4 lốp 215/55R17 - Trục 1, 2",
        stages: [
            { stageId: 1, stageName: "Kiểm tra ngoại thất", status: 2, result: 1, assignedUser: "Nguyễn Động Cơ" },
            { stageId: 2, stageName: "Kiểm tra động cơ", status: 2, result: 1, assignedUser: "Nguyễn Động Cơ" },
            { stageId: 3, stageName: "Kiểm tra hệ thống phanh", status: 2, result: 1, assignedUser: "Phạm Văn Phanh" },
            { stageId: 4, stageName: "Kiểm tra khí thải", status: 2, result: 1, assignedUser: "Lê Khí Thải" },
            { stageId: 5, stageName: "Kiểm tra hệ thống đèn", status: 2, result: 1, assignedUser: "Huỳnh Văn Đèn" }
        ],
        certificateNo: "001234570-01/2025",
        stickerNo: "TEM-2025-004567",
        issueDate: "2025-09-10",
        expiryDate: "2026-09-10",
        notes: "Xe đảm bảo an toàn kỹ thuật và bảo vệ môi trường"
    },
    {
        inspectionId: 5,
        inspectionCode: "KD-2025-005",
        vehicleId: 5,
        plateNo: "92A-33445",
        ownerId: "owner-005",
        ownerName: "Hoàng Văn Đức",
        ownerType: "PERSON",
        ownerPhone: "0903334455",
        ownerCCCD: "079070005678",
        ownerAddress: "654 Nguyễn Thị Minh Khai, Q3, TP.HCM",
        inspectionType: "RE_INSPECTION",
        laneId: 2,
        laneName: "Dây chuyền 2",
        status: 2, // PAID
        finalResult: null,
        createdAt: "2025-11-22T07:00:00",
        receivedAt: "2025-11-22T07:30:00",
        paidAt: "2025-11-22T07:45:00",
        startedAt: null,
        completedAt: null,
        concludedAt: null,
        receivedBy: "Phạm Ngọc Lan",
        concludedBy: null,
        vehicleGroup: "Xe con",
        vehicleType: "Xe ô tô con",
        brand: "Toyota Fortuner",
        model: "TGN61L",
        engineNo: "1GDFTV987654",
        chassis: "MHFTGN61LLJ123789",
        manufactureYear: 2017,
        manufactureCountry: "Indonesia",
        wheelFormula: "4x4",
        wheelTread: "1580/1580",
        overallDimensions: "4850 x 1855 x 1835",
        wheelbase: "2850",
        kerbWeight: "1985",
        authorizedCargoWeight: "615",
        authorizedTotalWeight: "2600",
        seatingCapacity: "7",
        engineType: "Dầu 4 kỳ tăng áp",
        engineModel: "1GD-FTV",
        engineDisplacement: "2755",
        maxPower: "130",
        maxPowerRPM: "3400",
        fuelType: "Dầu diesel",
        tires: "4 lốp 265/65R17 - Trục 1, 2",
        stages: [
            { stageId: 1, stageName: "Kiểm tra ngoại thất", status: 0, result: null, assignedUser: null },
            { stageId: 2, stageName: "Kiểm tra động cơ", status: 0, result: null, assignedUser: null },
            { stageId: 3, stageName: "Kiểm tra hệ thống phanh", status: 0, result: null, assignedUser: null },
            { stageId: 4, stageName: "Kiểm tra khí thải", status: 0, result: null, assignedUser: null },
            { stageId: 5, stageName: "Kiểm tra hệ thống đèn", status: 0, result: null, assignedUser: null }
        ],
        certificateNo: null,
        stickerNo: null,
        issueDate: null,
        expiryDate: null,
        notes: "Tái kiểm sau khi sửa chữa"
    }
];

let filteredRecords = [...inspectionRecords];

// Hiển thị danh sách hồ sơ
function displayRecords(recordsToDisplay = inspectionRecords) {
    const tbody = document.getElementById('recordsBody');
    tbody.innerHTML = '';

    recordsToDisplay.forEach((record, index) => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${index + 1}</td>
            <td><strong>${record.inspectionCode}</strong></td>
            <td><strong>${record.plateNo}</strong></td>
            <td>${record.ownerName}</td>
            <td><span class="inspection-type ${getInspectionTypeClass(record.inspectionType)}">${getInspectionTypeText(record.inspectionType)}</span></td>
            <td>${record.laneName || 'Chưa gán'}</td>
            <td>${formatDateTime(record.createdAt)}</td>
            <td><span class="status ${getStatusClass(record.status)}">${getStatusText(record.status)}</span></td>
            <td>${getFinalResultBadge(record.finalResult)}</td>
        `;

        // Thêm sự kiện double-click
        row.addEventListener('dblclick', () => showDetail(record));

        tbody.appendChild(row);
    });
}

// Hàm format ngày giờ
function formatDateTime(dateString) {
    if (!dateString) return '--';
    const date = new Date(dateString);
    return date.toLocaleString('vi-VN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    });
}

function formatDate(dateString) {
    if (!dateString) return '--';
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN');
}

// Lấy class CSS cho loại kiểm định
function getInspectionTypeClass(type) {
    const classes = {
        'FIRST': 'first',
        'PERIODIC': 'periodic',
        'RE_INSPECTION': 're-inspection'
    };
    return classes[type] || '';
}

function getInspectionTypeText(type) {
    const texts = {
        'FIRST': 'Lần đầu',
        'PERIODIC': 'Định kỳ',
        'RE_INSPECTION': 'Tái kiểm'
    };
    return texts[type] || type;
}

// Lấy class CSS cho trạng thái
function getStatusClass(status) {
    const classes = {
        0: 'draft',
        1: 'received',
        2: 'paid',
        3: 'in-progress',
        4: 'completed',
        5: 'passed',
        6: 'failed',
        7: 'certified'
    };
    return classes[status] || '';
}

function getStatusText(status) {
    const texts = {
        0: 'Nháp',
        1: 'Đã tiếp nhận',
        2: 'Đã thu phí',
        3: 'Đang kiểm định',
        4: 'Hoàn thành',
        5: 'Đạt',
        6: 'Không đạt',
        7: 'Đã cấp CN'
    };
    return texts[status] || 'Không xác định';
}

function getFinalResultBadge(result) {
    if (result === null) return '<span class="result pending">Chưa có</span>';
    const badges = {
        1: '<span class="result pass">ĐẠT</span>',
        2: '<span class="result fail">KHÔNG ĐẠT</span>',
        3: '<span class="result suspend">TẠM ĐÌNH CHỈ</span>'
    };
    return badges[result] || '<span class="result pending">--</span>';
}

// Hiển thị chi tiết hồ sơ
function showDetail(record) {
    // Điền thông tin cơ bản
    document.getElementById('detailSerialNo').textContent = record.certificateNo || record.inspectionCode;

    // Thông số kỹ thuật
    document.getElementById('detailWheelFormula').textContent = record.wheelFormula || '--';
    document.getElementById('detailWheelTread').textContent = record.wheelTread || '--';
    document.getElementById('detailDimensions').textContent = record.overallDimensions || '--';
    document.getElementById('detailTankDimensions').textContent = 'N/A';
    document.getElementById('detailWheelbase').textContent = record.wheelbase || '--';
    document.getElementById('detailKerbMass').textContent = record.kerbWeight || '--';
    document.getElementById('detailCargoMass').textContent = record.authorizedCargoWeight || '--';
    document.getElementById('detailTowedMass').textContent = '0';
    document.getElementById('detailTotalMass').textContent = record.authorizedTotalWeight || '--';
    document.getElementById('detailSeating').textContent = record.seatingCapacity || '--';
    document.getElementById('detailStanding').textContent = '0';
    document.getElementById('detailLying').textContent = '0';
    document.getElementById('detailEngineType').textContent = record.engineType || '--';
    document.getElementById('detailEngineModel').textContent = record.engineModel || '--';
    document.getElementById('detailEngineDisplacement').textContent = record.engineDisplacement || '--';
    document.getElementById('detailMaxOutput').textContent = record.maxPower && record.maxPowerRPM
        ? `${record.maxPower}/${record.maxPowerRPM}` : '--';
    document.getElementById('detailFuel').textContent = record.fuelType || '--';
    document.getElementById('detailMotorNumber').textContent = 'N/A';
    document.getElementById('detailMotorPower').textContent = 'N/A';
    document.getElementById('detailBattery').textContent = 'N/A';
    document.getElementById('detailTires').textContent = record.tires || '--';

    // Ngày kiểm định
    const issueDate = record.issueDate ? new Date(record.issueDate) : (record.completedAt ? new Date(record.completedAt) : new Date());
    document.getElementById('detailDay').textContent = issueDate.getDate();
    document.getElementById('detailMonth').textContent = issueDate.getMonth() + 1;
    document.getElementById('detailYear').textContent = issueDate.getFullYear();

    // Số phiếu kiểm định
    document.getElementById('detailInspectionNo').textContent = record.inspectionCode;

    // Thông tin đăng ký
    document.getElementById('detailPlate').textContent = record.plateNo;
    document.getElementById('detailVehicleNo').textContent = `VN-${String(record.vehicleId).padStart(9, '0')}`;
    document.getElementById('detailVehicleGroup').textContent = record.vehicleGroup || '--';
    document.getElementById('detailVehicleType').textContent = record.vehicleType || '--';
    document.getElementById('detailTrademark').textContent = record.brand || '--';
    document.getElementById('detailModelCode').textContent = record.model || '--';
    document.getElementById('detailEngineNo').textContent = record.engineNo || '--';
    document.getElementById('detailChassisNo').textContent = record.chassis || '--';
    document.getElementById('detailProductionYear').textContent = record.manufactureYear && record.manufactureCountry
        ? `${record.manufactureYear}/${record.manufactureCountry}` : '--';
    document.getElementById('detailLifetime').textContent = record.manufactureYear ? record.manufactureYear + 20 : '--';
    document.getElementById('detailNotes').textContent = record.notes || 'Không có ghi chú';

    // Checkbox
    document.getElementById('detailTachograph').checked = false;
    document.getElementById('detailCamera').checked = false;
    document.getElementById('detailNotIssued').checked = record.status !== 7;
    document.getElementById('detailGreenEnergy').checked = false;
    document.getElementById('detailAutomationPartial').checked = false;
    document.getElementById('detailAutomationFull').checked = false;
    document.getElementById('detailCommercialUse').checked = false;
    document.getElementById('detailModification').checked = false;

    // Hiển thị modal
    document.getElementById('detailModal').style.display = 'block';
}

// Tìm kiếm hồ sơ
function searchRecords() {
    const searchTerm = document.getElementById('searchInput').value.toLowerCase();

    if (searchTerm === '') {
        filteredRecords = [...inspectionRecords];
    } else {
        filteredRecords = inspectionRecords.filter(record =>
            record.inspectionCode.toLowerCase().includes(searchTerm) ||
            record.ownerName.toLowerCase().includes(searchTerm) ||
            record.plateNo.toLowerCase().includes(searchTerm)
        );
    }

    applyFilters();
}

// Lọc theo filter
function filterRecords() {
    const statusFilter = document.getElementById('filterStatus').value;
    const typeFilter = document.getElementById('filterType').value;
    const dateFilter = document.getElementById('filterDate').value;

    filteredRecords = [...inspectionRecords];

    if (statusFilter) {
        filteredRecords = filteredRecords.filter(r => r.status == statusFilter);
    }

    if (typeFilter) {
        filteredRecords = filteredRecords.filter(r => r.inspectionType === typeFilter);
    }

    if (dateFilter) {
        filteredRecords = filteredRecords.filter(r => {
            const recordDate = new Date(r.createdAt).toISOString().split('T')[0];
            return recordDate === dateFilter;
        });
    }

    // Áp dụng search term nếu có
    const searchTerm = document.getElementById('searchInput').value.toLowerCase();
    if (searchTerm) {
        filteredRecords = filteredRecords.filter(record =>
            record.inspectionCode.toLowerCase().includes(searchTerm) ||
            record.ownerName.toLowerCase().includes(searchTerm) ||
            record.plateNo.toLowerCase().includes(searchTerm)
        );
    }

    displayRecords(filteredRecords);
}

function applyFilters() {
    filterRecords();
}

// Đóng modal khi click vào nút X
document.querySelector('.close').addEventListener('click', function () {
    document.getElementById('detailModal').style.display = 'none';
});

// Đóng modal khi click bên ngoài
window.addEventListener('click', function (event) {
    const modal = document.getElementById('detailModal');
    if (event.target === modal) {
        modal.style.display = 'none';
    }
});

// Tìm kiếm khi nhấn Enter
document.getElementById('searchInput').addEventListener('keypress', function (event) {
    if (event.key === 'Enter') {
        searchRecords();
    }
});

// Tắt double-click trên checkbox trong modal
document.querySelectorAll('.modal input[type="checkbox"]').forEach(checkbox => {
    checkbox.addEventListener('dblclick', function (event) {
        event.stopPropagation();
    });
});

// Khởi tạo hiển thị danh sách
displayRecords();

// ============================================
// PHÂN CÔNG DÂY CHUYỀN
// ============================================

let assigningInspection = null;
let assignedLaneId = null;
let assignedLaneName = '';

// Mở modal phân công dây chuyền
function openAssignLane(record) {
    assigningInspection = record;
    assignedLaneId = record.laneId;
    assignedLaneName = record.laneName;

    // Điền thông tin hồ sơ
    document.getElementById('assignInspectionCode').textContent = record.inspectionCode;
    document.getElementById('assignPlateNo').textContent = record.plateNo;
    document.getElementById('assignOwnerName').textContent = record.ownerName;
    document.getElementById('assignVehicleType').textContent = record.vehicleType;

    // Clear note
    document.getElementById('assignNote').value = '';

    // Nếu đã có dây chuyền, highlight card
    if (assignedLaneId) {
        setTimeout(() => {
            const cards = document.querySelectorAll('.assign-lane-card');
            cards.forEach((card, index) => {
                if (index + 1 === assignedLaneId) {
                    card.classList.add('selected');
                }
            });
            document.getElementById('btnConfirmAssign').disabled = false;
        }, 100);
    }

    document.getElementById('assignLaneModal').style.display = 'block';
}

// Chọn dây chuyền trong modal phân công
function selectAssignLane(laneId, laneName) {
    assignedLaneId = laneId;
    assignedLaneName = laneName;

    // Highlight card được chọn
    document.querySelectorAll('.assign-lane-card').forEach(card => {
        card.classList.remove('selected');
    });
    event.target.closest('.assign-lane-card').classList.add('selected');

    // Enable nút xác nhận
    document.getElementById('btnConfirmAssign').disabled = false;
}

// Xác nhận phân công dây chuyền
function confirmAssignLane() {
    if (!assignedLaneId) {
        alert('Vui lòng chọn dây chuyền!');
        return;
    }

    const note = document.getElementById('assignNote').value;

    // Cập nhật hồ sơ
    assigningInspection.laneId = assignedLaneId;
    assigningInspection.laneName = assignedLaneName;

    if (note) {
        assigningInspection.notes = note;
    }

    // Nếu hồ sơ đang ở trạng thái "Đã thu phí", chuyển sang "Chờ kiểm định"
    if (assigningInspection.status === 2) {
        assigningInspection.status = 2; // Giữ nguyên trạng thái đã thu phí
    }

    alert(`Đã phân công hồ sơ ${assigningInspection.inspectionCode} vào ${assignedLaneName}!`);

    closeAssignLane();
    displayRecords();
}

// Đóng modal phân công
function closeAssignLane() {
    document.getElementById('assignLaneModal').style.display = 'none';
    assigningInspection = null;
    assignedLaneId = null;
    assignedLaneName = '';

    // Xóa selected
    document.querySelectorAll('.assign-lane-card').forEach(card => {
        card.classList.remove('selected');
    });

    document.getElementById('btnConfirmAssign').disabled = true;
}

// ============================================
// QUY TRÌNH KIỂM ĐỊNH
// ============================================

let currentInspection = null;
let selectedLaneId = null;
let selectedLaneName = '';
let currentStageIndex = 0;
let stagesData = [];
let allDefects = [];

// Định nghĩa các thông số cần đo cho mỗi giai đoạn
const stageItems = {
    1: [ // Kiểm tra ngoại thất
        { id: 1, name: 'Tình trạng khung xe', type: 'select', options: ['Tốt', 'Khuyết điểm', 'Hư hỏng'], standard: 'Tốt' },
        { id: 2, name: 'Tình trạng sơn', type: 'select', options: ['Tốt', 'Phai màu', 'Bong tróc'], standard: 'Tốt' },
        { id: 3, name: 'Kính chắn gió', type: 'select', options: ['Tốt', 'Nứt nhỏ', 'Vỡ'], standard: 'Tốt' },
        { id: 4, name: 'Gương chiếu hậu', type: 'select', options: ['Đầy đủ', 'Thiếu', 'Hư hỏng'], standard: 'Đầy đủ' }
    ],
    2: [ // Kiểm tra động cơ
        { id: 5, name: 'Tiếng động cơ', type: 'select', options: ['Bình thường', 'Ồn bất thường', 'Kêu lạ'], standard: 'Bình thường' },
        { id: 6, name: 'Rò rỉ dầu động cơ', type: 'select', options: ['Không', 'Nhẹ', 'Nghiêm trọng'], standard: 'Không' },
        { id: 7, name: 'Nhiệt độ động cơ (°C)', type: 'number', min: 80, max: 95, standard: '80-95' },
        { id: 8, name: 'Áp suất dầu (bar)', type: 'number', min: 2, max: 6, standard: '2-6' }
    ],
    3: [ // Kiểm tra hệ thống phanh
        { id: 9, name: 'Lực phanh trục trước (%)', type: 'number', min: 50, max: 100, standard: '≥ 50' },
        { id: 10, name: 'Lực phanh trục sau (%)', type: 'number', min: 50, max: 100, standard: '≥ 50' },
        { id: 11, name: 'Độ chênh lệch phanh (%)', type: 'number', min: 0, max: 30, standard: '≤ 30' },
        { id: 12, name: 'Phanh đỗ', type: 'select', options: ['Đạt', 'Không đạt'], standard: 'Đạt' }
    ],
    4: [ // Kiểm tra khí thải
        { id: 13, name: 'Nồng độ CO (%)', type: 'number', min: 0, max: 4.5, standard: '≤ 4.5' },
        { id: 14, name: 'Nồng độ HC (ppm)', type: 'number', min: 0, max: 1200, standard: '≤ 1200' },
        { id: 15, name: 'Độ khói (HSU)', type: 'number', min: 0, max: 40, standard: '≤ 40' },
        { id: 16, name: 'Độ ồn (dB)', type: 'number', min: 0, max: 95, standard: '≤ 95' }
    ],
    5: [ // Kiểm tra hệ thống đèn
        { id: 17, name: 'Đèn pha', type: 'select', options: ['Đạt', 'Không đạt'], standard: 'Đạt' },
        { id: 18, name: 'Đèn cos', type: 'select', options: ['Đạt', 'Không đạt'], standard: 'Đạt' },
        { id: 19, name: 'Đèn xi nhan', type: 'select', options: ['Đạt', 'Không đạt'], standard: 'Đạt' },
        { id: 20, name: 'Đèn phanh', type: 'select', options: ['Đạt', 'Không đạt'], standard: 'Đạt' }
    ]
};

// Mở modal quy trình kiểm định
function openInspectionProcess(record) {
    currentInspection = record;
    selectedLaneId = record.laneId;
    selectedLaneName = record.laneName;
    currentStageIndex = 0;
    stagesData = JSON.parse(JSON.stringify(record.stages)); // Deep copy
    allDefects = record.defects ? [...record.defects] : [];

    // Điền thông tin hồ sơ
    document.getElementById('processInspectionCode').textContent = record.inspectionCode;
    document.getElementById('processPlateNo').textContent = record.plateNo;
    document.getElementById('processOwnerName').textContent = record.ownerName;
    document.getElementById('processVehicleType').textContent = record.vehicleType;

    // Kiểm tra trạng thái
    if (record.status < 2) {
        alert('Hồ sơ chưa được thu phí. Vui lòng thu phí trước khi kiểm định.');
        return;
    }

    // Nếu đã có dây chuyền, bỏ qua bước chọn
    if (selectedLaneId) {
        document.getElementById('laneSelection').style.display = 'none';
        document.getElementById('inspectionStages').style.display = 'block';
        renderStagesList();
        showStage(0);
    } else {
        document.getElementById('laneSelection').style.display = 'block';
        document.getElementById('inspectionStages').style.display = 'none';
    }

    document.getElementById('inspectionProcessModal').style.display = 'block';
}

// Chọn dây chuyền
function selectLane(laneId, laneName) {
    selectedLaneId = laneId;
    selectedLaneName = laneName;

    // Highlight card được chọn
    document.querySelectorAll('.lane-card').forEach(card => {
        card.classList.remove('selected');
    });
    event.target.closest('.lane-card').classList.add('selected');

    // Enable nút bắt đầu
    document.getElementById('btnStartInspection').disabled = false;
}

// Bắt đầu kiểm định
function startInspection() {
    if (!selectedLaneId) {
        alert('Vui lòng chọn dây chuyền!');
        return;
    }

    // Ẩn phần chọn dây chuyền, hiện các giai đoạn
    document.getElementById('laneSelection').style.display = 'none';
    document.getElementById('inspectionStages').style.display = 'block';

    // Render danh sách giai đoạn
    renderStagesList();

    // Hiển thị giai đoạn đầu tiên
    showStage(0);
}

// Render danh sách các giai đoạn
function renderStagesList() {
    const stagesList = document.getElementById('stagesList');
    stagesList.innerHTML = '';

    stagesData.forEach((stage, index) => {
        const stageItem = document.createElement('div');
        stageItem.className = 'stage-item';

        if (stage.status === 2) stageItem.classList.add('completed');
        if (stage.result === 2) stageItem.classList.add('failed');
        if (index === currentStageIndex) stageItem.classList.add('active');

        let statusText = 'Chờ thực hiện';
        let statusClass = 'pending';
        if (stage.status === 2) {
            statusText = stage.result === 1 ? 'Đã hoàn thành' : 'Không đạt';
            statusClass = stage.result === 1 ? 'completed' : 'failed';
        } else if (stage.status === 1) {
            statusText = 'Đang thực hiện';
            statusClass = 'pending';
        }

        stageItem.innerHTML = `
            <div class="stage-icon">
                <i class="fa-solid ${stage.status === 2 ? (stage.result === 1 ? 'fa-check' : 'fa-xmark') : 'fa-clock'}"></i>
            </div>
            <div class="stage-info">
                <h4>${index + 1}. ${stage.stageName}</h4>
                <p>KTV: ${stage.assignedUser || 'Chưa phân công'}</p>
            </div>
            <div class="stage-status ${statusClass}">${statusText}</div>
        `;

        stageItem.onclick = () => showStage(index);
        stagesList.appendChild(stageItem);
    });

    updateProgress();
}

// Hiển thị chi tiết một giai đoạn
function showStage(index) {
    currentStageIndex = index;
    const stage = stagesData[index];

    document.getElementById('stageDetail').style.display = 'block';
    document.getElementById('inspectionConclusion').style.display = 'none';

    document.getElementById('stageName').textContent = stage.stageName;
    document.getElementById('stageDescription').textContent = `Công đoạn ${index + 1}/${stagesData.length}`;

    // Render form nhập liệu
    renderStageForm(stage);

    // Hiển thị lỗi nếu có
    if (stage.result === 2) {
        document.getElementById('defectsSection').style.display = 'block';
        renderDefects(stage.stageId);
    } else {
        document.getElementById('defectsSection').style.display = 'none';
    }

    // Cập nhật nút
    document.querySelector('.stage-actions button:first-child').style.display = index === 0 ? 'none' : 'inline-flex';
    document.getElementById('btnNextStage').textContent = index === stagesData.length - 1 ? 'Kết Luận' : 'Tiếp Theo';

    renderStagesList();
}

// Render form nhập liệu cho giai đoạn
function renderStageForm(stage) {
    const form = document.getElementById('stageForm');
    form.innerHTML = '';

    const items = stageItems[stage.stageId] || [];

    items.forEach(item => {
        const formGroup = document.createElement('div');
        formGroup.className = 'form-group';

        let inputHtml = '';
        if (item.type === 'select') {
            inputHtml = `
                <select id="item_${item.id}" onchange="checkItemStandard(${item.id})">
                    <option value="">-- Chọn --</option>
                    ${item.options.map(opt => `<option value="${opt}">${opt}</option>`).join('')}
                </select>
            `;
        } else if (item.type === 'number') {
            inputHtml = `
                <input type="number" id="item_${item.id}" 
                       step="0.1" 
                       placeholder="Nhập giá trị đo"
                       onchange="checkItemStandard(${item.id})">
            `;
        }

        formGroup.innerHTML = `
            <label>${item.name} <span style="color: #dc3545;">*</span></label>
            ${inputHtml}
            <div class="help-text">Tiêu chuẩn: ${item.standard}</div>
            <div id="result_${item.id}" style="margin-top: 5px;"></div>
        `;

        form.appendChild(formGroup);

        // Load dữ liệu đã lưu (nếu có)
        if (stage.measurements && stage.measurements[item.id]) {
            document.getElementById(`item_${item.id}`).value = stage.measurements[item.id];
            checkItemStandard(item.id);
        }
    });
}

// Kiểm tra thông số có đạt chuẩn không
function checkItemStandard(itemId) {
    const stage = stagesData[currentStageIndex];
    const items = stageItems[stage.stageId] || [];
    const item = items.find(i => i.id === itemId);

    if (!item) return;

    const value = document.getElementById(`item_${itemId}`).value;
    const resultDiv = document.getElementById(`result_${itemId}`);

    if (!value) {
        resultDiv.innerHTML = '';
        return;
    }

    let isPassed = false;

    if (item.type === 'select') {
        isPassed = value === item.standard;
    } else if (item.type === 'number') {
        const numValue = parseFloat(value);
        isPassed = numValue >= item.min && numValue <= item.max;
    }

    if (isPassed) {
        resultDiv.innerHTML = '<span style="color: #28a745; font-weight: bold;"><i class="fa-solid fa-check-circle"></i> Đạt chuẩn</span>';
    } else {
        resultDiv.innerHTML = '<span style="color: #dc3545; font-weight: bold;"><i class="fa-solid fa-times-circle"></i> Không đạt chuẩn</span>';
    }
}

// Lưu kết quả giai đoạn
function saveStageResult() {
    const stage = stagesData[currentStageIndex];
    const items = stageItems[stage.stageId] || [];

    // Validate tất cả các trường
    let allFilled = true;
    let allPassed = true;
    const measurements = {};

    items.forEach(item => {
        const value = document.getElementById(`item_${item.id}`).value;
        if (!value) {
            allFilled = false;
            return;
        }

        measurements[item.id] = value;

        // Kiểm tra đạt chuẩn
        let isPassed = false;
        if (item.type === 'select') {
            isPassed = value === item.standard;
        } else if (item.type === 'number') {
            const numValue = parseFloat(value);
            isPassed = numValue >= item.min && numValue <= item.max;
        }

        if (!isPassed) allPassed = false;
    });

    if (!allFilled) {
        alert('Vui lòng nhập đầy đủ tất cả các thông số!');
        return;
    }

    // Lưu measurements
    stage.measurements = measurements;
    stage.status = 2; // COMPLETED
    stage.result = allPassed ? 1 : 2; // 1: ĐẠT, 2: KHÔNG ĐẠT

    if (!allPassed) {
        if (confirm('Công đoạn không đạt. Bạn có muốn ghi nhận lỗi chi tiết?')) {
            document.getElementById('defectsSection').style.display = 'block';
        }
    }

    alert('Đã lưu kết quả công đoạn!');
    renderStagesList();
}

// Render danh sách lỗi
function renderDefects(stageId) {
    const defectsList = document.getElementById('defectsList');
    const stageDefects = allDefects.filter(d => d.stageId === stageId);

    defectsList.innerHTML = '';

    stageDefects.forEach((defect, index) => {
        const defectItem = document.createElement('div');
        defectItem.className = 'defect-item';
        defectItem.innerHTML = `
            <div class="defect-header">
                <strong>${defect.category}</strong>
                <div>
                    <span class="defect-severity ${defect.severity === 3 ? 'critical' : defect.severity === 2 ? 'major' : 'minor'}">
                        ${defect.severity === 3 ? 'Nghiêm trọng' : defect.severity === 2 ? 'Hư hỏng' : 'Khuyết điểm'}
                    </span>
                    <button onclick="removeDefect(${stageId}, ${index})" style="margin-left: 10px; color: #dc3545; background: none; border: none; cursor: pointer;">
                        <i class="fa-solid fa-trash"></i>
                    </button>
                </div>
            </div>
            <p>${defect.description}</p>
        `;
        defectsList.appendChild(defectItem);
    });
}

// Thêm lỗi mới
function addDefect() {
    const stage = stagesData[currentStageIndex];

    const category = prompt('Danh mục lỗi:');
    if (!category) return;

    const description = prompt('Mô tả chi tiết lỗi:');
    if (!description) return;

    const severity = prompt('Mức độ nghiêm trọng:\n1: Khuyết điểm\n2: Hư hỏng\n3: Nguy hiểm');
    if (!severity || ![' 1', '2', '3'].includes(severity)) return;

    const defect = {
        defectId: Date.now(),
        stageId: stage.stageId,
        category: category,
        description: description,
        severity: parseInt(severity)
    };

    allDefects.push(defect);
    renderDefects(stage.stageId);
}

// Xóa lỗi
function removeDefect(stageId, index) {
    const stageDefects = allDefects.filter(d => d.stageId === stageId);
    const defect = stageDefects[index];
    const globalIndex = allDefects.indexOf(defect);
    allDefects.splice(globalIndex, 1);
    renderDefects(stageId);
}

// Cập nhật progress bar
function updateProgress() {
    const completedCount = stagesData.filter(s => s.status === 2).length;
    const totalCount = stagesData.length;
    const percentage = (completedCount / totalCount) * 100;

    document.getElementById('progressFill').style.width = percentage + '%';
    document.getElementById('currentStageNum').textContent = completedCount;
    document.getElementById('totalStages').textContent = totalCount;
}

// Giai đoạn trước
function previousStage() {
    if (currentStageIndex > 0) {
        showStage(currentStageIndex - 1);
    }
}

// Giai đoạn tiếp theo
function nextStage() {
    const stage = stagesData[currentStageIndex];

    if (stage.status !== 2) {
        if (!confirm('Bạn chưa lưu kết quả công đoạn này. Tiếp tục?')) {
            return;
        }
    }

    if (currentStageIndex < stagesData.length - 1) {
        showStage(currentStageIndex + 1);
    } else {
        showConclusion();
    }
}

// Hiển thị phần kết luận
function showConclusion() {
    document.getElementById('stageDetail').style.display = 'none';
    document.getElementById('inspectionConclusion').style.display = 'block';

    const passedCount = stagesData.filter(s => s.result === 1).length;
    const failedCount = stagesData.filter(s => s.result === 2).length;
    const defectsCount = allDefects.length;

    document.getElementById('passedCount').textContent = passedCount;
    document.getElementById('failedCount').textContent = failedCount;
    document.getElementById('defectsCount').textContent = defectsCount;

    // Hiển thị tất cả lỗi
    if (defectsCount > 0) {
        document.getElementById('allDefectsSection').style.display = 'block';
        const allDefectsList = document.getElementById('allDefectsList');
        allDefectsList.innerHTML = '';

        allDefects.forEach(defect => {
            const stage = stagesData.find(s => s.stageId === defect.stageId);
            const defectItem = document.createElement('div');
            defectItem.className = 'defect-item';
            defectItem.innerHTML = `
                <div class="defect-header">
                    <strong>${stage.stageName} - ${defect.category}</strong>
                    <span class="defect-severity ${defect.severity === 3 ? 'critical' : defect.severity === 2 ? 'major' : 'minor'}">
                        ${defect.severity === 3 ? 'Nghiêm trọng' : defect.severity === 2 ? 'Hư hỏng' : 'Khuyết điểm'}
                    </span>
                </div>
                <p>${defect.description}</p>
            `;
            allDefectsList.appendChild(defectItem);
        });
    }
}

// Cập nhật kết luận cuối cùng
function updateFinalConclusion() {
    const finalResult = document.getElementById('finalResultSelect').value;
    // Logic xử lý khi chọn kết luận
}

// Quay lại danh sách giai đoạn
function backToStages() {
    showStage(stagesData.length - 1);
}

// Hoàn thành kiểm định
function submitConclusion() {
    const finalResult = document.getElementById('finalResultSelect').value;
    const conclusionNote = document.getElementById('conclusionNote').value;

    if (!finalResult) {
        alert('Vui lòng chọn kết luận cuối cùng!');
        return;
    }

    if (confirm('Xác nhận hoàn thành kiểm định?')) {
        // Cập nhật hồ sơ
        currentInspection.stages = stagesData;
        currentInspection.defects = allDefects;
        currentInspection.finalResult = parseInt(finalResult);
        currentInspection.notes = conclusionNote;
        currentInspection.status = finalResult === '1' ? 5 : 6; // 5: PASSED, 6: FAILED
        currentInspection.completedAt = new Date().toISOString();
        currentInspection.laneId = selectedLaneId;
        currentInspection.laneName = selectedLaneName;

        alert('Đã hoàn thành kiểm định!');
        closeInspectionProcess();
        displayRecords();
    }
}

// Đóng modal quy trình kiểm định
function closeInspectionProcess() {
    document.getElementById('inspectionProcessModal').style.display = 'none';
    currentInspection = null;
    selectedLaneId = null;
    selectedLaneName = '';
    currentStageIndex = 0;
}

// Cập nhật hàm hiển thị danh sách để thêm nút "Kiểm định"
function displayRecords(recordsToDisplay = inspectionRecords) {
    const tbody = document.getElementById('recordsBody');
    tbody.innerHTML = '';

    recordsToDisplay.forEach((record, index) => {
        const row = document.createElement('tr');

        // Tạo các nút hành động dựa vào trạng thái
        let actionButtons = '';

        // Nút phân công dây chuyền (cho hồ sơ đã thu phí nhưng chưa có dây chuyền hoặc muốn đổi dây chuyền)
        if (record.status >= 2 && record.status < 7) {
            actionButtons += `
                <button class="btn-action assign" onclick="openAssignLane(inspectionRecords[${index}])" title="Phân công dây chuyền">
                    <i class="fa-solid fa-road"></i>
                </button>
            `;
        }

        // Nút bắt đầu kiểm định (cho hồ sơ đã có dây chuyền và chưa hoàn thành)
        if (record.laneId && record.status >= 2 && record.status < 7) {
            actionButtons += `
                <button class="btn-action inspect" onclick="openInspectionProcess(inspectionRecords[${index}])" title="Kiểm định">
                    <i class="fa-solid fa-clipboard-check"></i>
                </button>
            `;
        }

        // Nút xem chi tiết (luôn có)
        actionButtons += `
            <button class="btn-action view" onclick="showDetail(inspectionRecords[${index}])" title="Xem chi tiết">
                <i class="fa-solid fa-eye"></i>
            </button>
        `;

        row.innerHTML = `
            <td>${index + 1}</td>
            <td><strong>${record.inspectionCode}</strong></td>
            <td><strong>${record.plateNo}</strong></td>
            <td>${record.ownerName}</td>
            <td><span class="inspection-type ${getInspectionTypeClass(record.inspectionType)}">${getInspectionTypeText(record.inspectionType)}</span></td>
            <td>${record.laneName ? `<i class="fa-solid fa-check-circle" style="color: #28a745;"></i> ${record.laneName}` : '<span style="color: #dc3545;"><i class="fa-solid fa-clock"></i> Chưa gán</span>'}</td>
            <td>${formatDateTime(record.createdAt)}</td>
            <td><span class="status ${getStatusClass(record.status)}">${getStatusText(record.status)}</span></td>
            <td>${getFinalResultBadge(record.finalResult)}</td>
            <td>
                <div class="action-buttons">
                    ${actionButtons}
                </div>
            </td>
        `;

        tbody.appendChild(row);
    });
}
