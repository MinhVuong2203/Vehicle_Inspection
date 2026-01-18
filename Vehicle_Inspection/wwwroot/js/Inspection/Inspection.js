// Biến lưu trữ dữ liệu
let inspectionRecords = [];
let filteredRecords = [];

// Load dữ liệu từ server khi trang được tải
document.addEventListener('DOMContentLoaded', function() {
    loadInspectionRecords();
});

// Hàm load dữ liệu từ database
async function loadInspectionRecords() {
    try {
        showLoading(true);
        
        const response = await fetch('/Inspection/GetInspectionRecords', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error('Không thể tải dữ liệu');
        }

        const result = await response.json();
        
        if (result.success) {
            inspectionRecords = result.data || [];
            filteredRecords = [...inspectionRecords];
            displayRecords(filteredRecords);
            
            if (inspectionRecords.length === 0) {
                showNoData();
            }
        } else {
            showError(result.message || 'Có lỗi xảy ra khi tải dữ liệu');
        }
    } catch (error) {
        console.error('Error loading inspection records:', error);
        showError('Không thể kết nối đến server. Vui lòng thử lại sau.');
    } finally {
        showLoading(false);
    }
}

// Hiển thị loading
function showLoading(show) {
    const tbody = document.getElementById('recordsBody');
    if (show) {
        tbody.innerHTML = `
            <tr>
                <td colspan="10" style="text-align: center; padding: 40px;">
                    <i class="fa-solid fa-spinner fa-spin" style="font-size: 32px; color: #667eea;"></i>
                    <p style="margin-top: 10px; color: #6c757d;">Đang tải dữ liệu...</p>
                </td>
            </tr>
        `;
    }
}

// Hiển thị thông báo không có dữ liệu
function showNoData() {
    const tbody = document.getElementById('recordsBody');
    tbody.innerHTML = `
        <tr>
            <td colspan="10" style="text-align: center; padding: 40px;">
                <i class="fa-solid fa-inbox" style="font-size: 48px; color: #ccc;"></i>
                <p style="margin-top: 10px; color: #6c757d;">Chưa có hồ sơ kiểm định nào</p>
            </td>
        </tr>
    `;
}

// Hiển thị lỗi
function showError(message) {
    const tbody = document.getElementById('recordsBody');
    tbody.innerHTML = `
        <tr>
            <td colspan="10" style="text-align: center; padding: 40px;">
                <i class="fa-solid fa-exclamation-triangle" style="font-size: 48px; color: #dc3545;"></i>
                <p style="margin-top: 10px; color: #dc3545; font-weight: bold;">${message}</p>
                <button onclick="loadInspectionRecords()" style="margin-top: 15px; padding: 10px 20px; background: #667eea; color: white; border: none; border-radius: 5px; cursor: pointer;">
                    <i class="fa-solid fa-refresh"></i> Thử lại
                </button>
            </td>
        </tr>
    `;
}

// Hiển thị danh sách hồ sơ
function displayRecords(recordsToDisplay = inspectionRecords) {
    const tbody = document.getElementById('recordsBody');
    
    if (!recordsToDisplay || recordsToDisplay.length === 0) {
        showNoData();
        return;
    }
    
    tbody.innerHTML = '';

    recordsToDisplay.forEach((record, index) => {
        const row = document.createElement('tr');

        // Tạo các nút hành động dựa vào trạng thái
        let actionButtons = '';

        // Nút phân công dây chuyền (cho hồ sơ đã thu phí)
        if (record.status >= 2 && record.status < 7) {
            actionButtons += `
                <button class="btn-action assign" onclick='openAssignLaneById(${record.inspectionId})' title="Phân công dây chuyền">
                    <i class="fa-solid fa-road"></i>
                </button>
            `;
        }

        // Nút bắt đầu kiểm định (cho hồ sơ đã có dây chuyền)
        if (record.laneId && record.status >= 2 && record.status < 7) {
            actionButtons += `
                <button class="btn-action inspect" onclick='openInspectionProcessById(${record.inspectionId})' title="Kiểm định">
                    <i class="fa-solid fa-clipboard-check"></i>
                </button>
            `;
        }

        // Nút xem chi tiết (luôn có)
        actionButtons += `
            <button class="btn-action view" onclick='showDetailById(${record.inspectionId})' title="Xem chi tiết">
                <i class="fa-solid fa-eye"></i>
            </button>
        `;

        row.innerHTML = `
            <td>${index + 1}</td>
            <td><strong>${record.inspectionCode || 'N/A'}</strong></td>
            <td><strong>${record.plateNo || 'N/A'}</strong></td>
            <td>${record.ownerFullName || 'N/A'}</td>
            <td><span class="inspection-type ${getInspectionTypeClass(record.inspectionType)}">${record.inspectionTypeText || 'N/A'}</span></td>
            <td>${record.laneName ? `<i class="fa-solid fa-check-circle" style="color: #28a745;"></i> ${record.laneName}` : '<span style="color: #dc3545;"><i class="fa-solid fa-clock"></i> Chưa gán</span>'}</td>
            <td>${formatDateTime(record.createdAt)}</td>
            <td><span class="status ${getStatusClass(record.status)}">${record.statusText || 'N/A'}</span></td>
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

// Hàm format ngày giờ
function formatDateTime(dateString) {
    if (!dateString) return '--';
    const date = new Date(dateString);
    if (isNaN(date.getTime())) return '--';
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
    if (isNaN(date.getTime())) return '--';
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
    if (result === null || result === undefined) return '<span class="result pending">Chưa có</span>';
    const badges = {
        1: '<span class="result pass">ĐẠT</span>',
        2: '<span class="result fail">KHÔNG ĐẠT</span>',
        3: '<span class="result suspend">TẠM ĐÌNH CHỈ</span>'
    };
    return badges[result] || '<span class="result pending">--</span>';
}

// Hàm tìm record theo ID
function findRecordById(inspectionId) {
    return inspectionRecords.find(r => r.inspectionId === inspectionId);
}

// Load chi tiết hồ sơ từ server
async function loadInspectionDetail(inspectionId) {
    try {
        console.log('Loading detail for inspection:', inspectionId); // LOG 1

        const response = await fetch(`/Inspection/GetInspectionRecord?id=${inspectionId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        console.log('Response status:', response.status); // LOG 2

        if (!response.ok) {
            throw new Error('Không thể tải chi tiết hồ sơ');
        }

        const result = await response.json();

        console.log('API Response:', result); // LOG 3

        if (result.success && result.data) {
            console.log('Detail data:', result.data); // LOG 4
            return result.data;
        } else {
            throw new Error(result.message || 'Không tìm thấy hồ sơ');
        }
    } catch (error) {
        console.error('Error loading inspection detail:', error);
        alert('Không thể tải chi tiết hồ sơ. Vui lòng thử lại.');
        return null;
    }
}

// Hiển thị chi tiết hồ sơ (wrapper function với ID)
async function showDetailById(inspectionId) {
    const record = findRecordById(inspectionId);
    if (!record) {
        alert('Không tìm thấy hồ sơ');
        return;
    }
    
    // Load thông tin chi tiết từ server nếu cần
    const detailRecord = await loadInspectionDetail(inspectionId);
    if (detailRecord) {
        showDetail(detailRecord);
    }
}

// Hiển thị chi tiết hồ sơ
function showDetail(record) {
    // Điền thông tin cơ bản
    document.getElementById('detailSerialNo').textContent = record.certificateNo || record.inspectionCode || 'N/A';

    // Thông số kỹ thuật
    document.getElementById('detailWheelFormula').textContent = record.wheelFormula || '--';
    document.getElementById('detailWheelTread').textContent = record.wheelTread || '--';
    document.getElementById('detailDimensions').textContent = record.overallDimensions || '--';
    document.getElementById('detailTankDimensions').textContent = record.cargoInsideDimensions || 'N/A';
    document.getElementById('detailWheelbase').textContent = record.wheelbase || '--';
    document.getElementById('detailKerbMass').textContent = record.kerbWeight || '--';
    document.getElementById('detailCargoMass').textContent = record.authorizedCargoWeight || '--';
    document.getElementById('detailTowedMass').textContent = record.authorizedTowedWeight || '0';
    document.getElementById('detailTotalMass').textContent = record.authorizedTotalWeight || '--';
    document.getElementById('detailSeating').textContent = record.seatingCapacity || '--';
    document.getElementById('detailStanding').textContent = record.standingCapacity || '0';
    document.getElementById('detailLying').textContent = record.lyingCapacity || '0';
    document.getElementById('detailEngineType').textContent = record.engineType || '--';
    document.getElementById('detailEngineModel').textContent = record.engineModel || '--';
    document.getElementById('detailEngineDisplacement').textContent = record.engineDisplacement || '--';
    document.getElementById('detailMaxOutput').textContent = record.maxOutputRPM || '--';
    document.getElementById('detailFuel').textContent = record.fuelType || '--';
    document.getElementById('detailMotorNumber').textContent = record.numberOfMotors || 'N/A';
    document.getElementById('detailMotorPower').textContent = record.motorPowerInfo || 'N/A';
    document.getElementById('detailBattery').textContent = record.batteryInfo || 'N/A';

    // Thông tin lốp
    const tireInfo = record.tireCount && record.tireSize && record.tireAxleInfo
        ? `${record.tireCount} lốp ${record.tireSize} - ${record.tireAxleInfo}`
        : '--';
    document.getElementById('detailTires').textContent = tireInfo;

    // Ngày kiểm định
    const issueDate = record.issueDate
        ? new Date(record.issueDate)
        : (record.completedAt ? new Date(record.completedAt) : new Date());
    if (!isNaN(issueDate.getTime())) {
        document.getElementById('detailDay').textContent = issueDate.getDate();
        document.getElementById('detailMonth').textContent = issueDate.getMonth() + 1;
        document.getElementById('detailYear').textContent = issueDate.getFullYear();
    }

    // Số phiếu kiểm định
    document.getElementById('detailInspectionNo').textContent = record.inspectionCode || 'N/A';

    // Thông tin đăng ký
    document.getElementById('detailPlate').textContent = record.plateNo || 'N/A';
    document.getElementById('detailVehicleNo').textContent = record.inspectionNo || 'N/A';
    document.getElementById('detailVehicleGroup').textContent = record.vehicleGroup || '--';
    document.getElementById('detailVehicleType').textContent = record.vehicleType || '--';
    document.getElementById('detailTrademark').textContent = record.brand || '--';
    document.getElementById('detailModelCode').textContent = record.model || '--';
    document.getElementById('detailEngineNo').textContent = record.engineNo || '--';
    document.getElementById('detailChassisNo').textContent = record.chassis || '--';
    document.getElementById('detailProductionYear').textContent = record.productionInfo || '--';
    document.getElementById('detailLifetime').textContent = record.lifetimeLimitYear || (record.manufactureYear ? record.manufactureYear + 20 : '--');
    document.getElementById('detailNotes').textContent = record.notes || 'Không có ghi chú';

    // Checkbox
    document.getElementById('detailTachograph').checked = record.hasTachograph || false;
    document.getElementById('detailCamera').checked = record.hasDriverCamera || false;
    document.getElementById('detailNotIssued').checked = record.notIssuedStamp || record.status !== 7;
    document.getElementById('detailGreenEnergy').checked = record.isCleanEnergy || false;

    // Usage permission
    const usagePermission = record.usagePermission || '';
    document.getElementById('detailAutomationPartial').checked = usagePermission.includes('Một phần') || usagePermission.includes('Partially');
    document.getElementById('detailAutomationFull').checked = usagePermission.includes('Toàn phần') || usagePermission.includes('Fully');

    document.getElementById('detailCommercialUse').checked = record.hasCommercialModification || false;
    document.getElementById('detailModification').checked = record.hasModification || false;

    // Hiển thị modal
    document.getElementById('detailModal').style.display = 'block';
}

// Tìm kiếm hồ sơ
function searchRecords() {
    const searchTerm = document.getElementById('searchInput').value.toLowerCase().trim();
    
    if (searchTerm === '') {
        filteredRecords = [...inspectionRecords];
    } else {
        filteredRecords = inspectionRecords.filter(record =>
            (record.inspectionCode && record.inspectionCode.toLowerCase().includes(searchTerm)) ||
            (record.ownerFullName && record.ownerFullName.toLowerCase().includes(searchTerm)) ||
            (record.plateNo && record.plateNo.toLowerCase().includes(searchTerm)) ||
            (record.ownerPhone && record.ownerPhone.includes(searchTerm))
        );
    }

    displayRecords(filteredRecords);
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
            if (!r.createdAt) return false;
            const recordDate = new Date(r.createdAt).toISOString().split('T')[0];
            return recordDate === dateFilter;
        });
    }

    // Áp dụng search term nếu có
    const searchTerm = document.getElementById('searchInput').value.toLowerCase().trim();
    if (searchTerm) {
        filteredRecords = filteredRecords.filter(record =>
            (record.inspectionCode && record.inspectionCode.toLowerCase().includes(searchTerm)) ||
            (record.ownerFullName && record.ownerFullName.toLowerCase().includes(searchTerm)) ||
            (record.plateNo && record.plateNo.toLowerCase().includes(searchTerm))
        );
    }

    displayRecords(filteredRecords);
}

// Đóng modal khi click vào nút X
const closeButtons = document.querySelectorAll('.modal .close');
closeButtons.forEach(btn => {
    btn.addEventListener('click', function () {
        this.closest('.modal').style.display = 'none';
    });
});

// Đóng modal khi click bên ngoài
window.addEventListener('click', function (event) {
    if (event.target.classList.contains('modal')) {
        event.target.style.display = 'none';
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

// ============================================
// PHÂN CÔNG DÂY CHUYỀN
// ============================================

let assigningInspection = null;
let assignedLaneId = null;
let assignedLaneName = '';

// Wrapper function để mở modal phân công bằng ID
function openAssignLaneById(inspectionId) {
    const record = findRecordById(inspectionId);
    if (record) {
        openAssignLane(record);
    }
}

// Mở modal phân công dây chuyền
function openAssignLane(record) {
    assigningInspection = record;
    assignedLaneId = record.laneId;
    assignedLaneName = record.laneName;

    // Điền thông tin hồ sơ
    document.getElementById('assignInspectionCode').textContent = record.inspectionCode || 'N/A';
    document.getElementById('assignPlateNo').textContent = record.plateNo || 'N/A';
    document.getElementById('assignOwnerName').textContent = record.ownerFullName || 'N/A';
    document.getElementById('assignVehicleType').textContent = record.vehicleType || 'N/A';

    // Clear note
    document.getElementById('assignNote').value = '';

    // Reset selected cards
    document.querySelectorAll('.assign-lane-card').forEach(card => {
        card.classList.remove('selected');
    });

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
async function confirmAssignLane() {
    if (!assignedLaneId) {
        alert('Vui lòng chọn dây chuyền!');
        return;
    }

    const note = document.getElementById('assignNote').value;

    try {
        // TODO: Gọi API để cập nhật dây chuyền
        const response = await fetch('/Inspection/AssignLane', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                inspectionId: assigningInspection.inspectionId,
                laneId: assignedLaneId,
                note: note
            })
        });

        if (!response.ok) {
            throw new Error('Không thể phân công dây chuyền');
        }

        const result = await response.json();
        
        if (result.success) {
            alert(`Đã phân công hồ sơ ${assigningInspection.inspectionCode} vào ${assignedLaneName}!`);
            closeAssignLane();
            
            // Reload lại danh sách
            await loadInspectionRecords();
        } else {
            alert(result.message || 'Có lỗi xảy ra');
        }
    } catch (error) {
        console.error('Error assigning lane:', error);
        alert('Không thể phân công dây chuyền. Vui lòng thử lại.');
    }
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

// Wrapper function để mở modal quy trình kiểm định bằng ID
function openInspectionProcessById(inspectionId) {
    const record = findRecordById(inspectionId);
    if (record) {
        openInspectionProcess(record);
    }
}

// Mở modal quy trình kiểm định
function openInspectionProcess(record) {
    currentInspection = record;
    selectedLaneId = record.laneId;
    selectedLaneName = record.laneName;
    currentStageIndex = 0;
    
    // Load stages từ server hoặc khởi tạo mới
    stagesData = [
        { stageId: 1, stageName: "Kiểm tra ngoại thất", status: 0, result: null, assignedUser: null },
        { stageId: 2, stageName: "Kiểm tra động cơ", status: 0, result: null, assignedUser: null },
        { stageId: 3, stageName: "Kiểm tra hệ thống phanh", status: 0, result: null, assignedUser: null },
        { stageId: 4, stageName: "Kiểm tra khí thải", status: 0, result: null, assignedUser: null },
        { stageId: 5, stageName: "Kiểm tra hệ thống đèn", status: 0, result: null, assignedUser: null }
    ];
    
    allDefects = [];

    // Điền thông tin hồ sơ
    document.getElementById('processInspectionCode').textContent = record.inspectionCode || 'N/A';
    document.getElementById('processPlateNo').textContent = record.plateNo || 'N/A';
    document.getElementById('processOwnerName').textContent = record.ownerFullName || 'N/A';
    document.getElementById('processVehicleType').textContent = record.vehicleType || 'N/A';

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

    document.querySelectorAll('.lane-card').forEach(card => {
        card.classList.remove('selected');
    });
    event.target.closest('.lane-card').classList.add('selected');

    document.getElementById('btnStartInspection').disabled = false;
}

// Bắt đầu kiểm định
function startInspection() {
    if (!selectedLaneId) {
        alert('Vui lòng chọn dây chuyền!');
        return;
    }

    document.getElementById('laneSelection').style.display = 'none';
    document.getElementById('inspectionStages').style.display = 'block';

    renderStagesList();
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

    renderStageForm(stage);

    if (stage.result === 2) {
        document.getElementById('defectsSection').style.display = 'block';
        renderDefects(stage.stageId);
    } else {
        document.getElementById('defectsSection').style.display = 'none';
    }

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

    stage.measurements = measurements;
    stage.status = 2;
    stage.result = allPassed ? 1 : 2;

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
    if (!severity || !['1', '2', '3'].includes(severity)) return;

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
async function submitConclusion() {
    const finalResult = document.getElementById('finalResultSelect').value;
    const conclusionNote = document.getElementById('conclusionNote').value;

    if (!finalResult) {
        alert('Vui lòng chọn kết luận cuối cùng!');
        return;
    }

    if (!confirm('Xác nhận hoàn thành kiểm định?')) {
        return;
    }

    try {
        // TODO: Gọi API để lưu kết quả kiểm định
        const response = await fetch('/Inspection/SubmitInspectionResult', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                inspectionId: currentInspection.inspectionId,
                stages: stagesData,
                defects: allDefects,
                finalResult: parseInt(finalResult),
                conclusionNote: conclusionNote,
                laneId: selectedLaneId
            })
        });

        if (!response.ok) {
            throw new Error('Không thể lưu kết quả kiểm định');
        }

        const result = await response.json();
        
        if (result.success) {
            alert('Đã hoàn thành kiểm định!');
            closeInspectionProcess();
            await loadInspectionRecords();
        } else {
            alert(result.message || 'Có lỗi xảy ra');
        }
    } catch (error) {
        console.error('Error submitting inspection:', error);
        alert('Không thể hoàn thành kiểm định. Vui lòng thử lại.');
    }
}

// Đóng modal quy trình kiểm định
function closeInspectionProcess() {
    document.getElementById('inspectionProcessModal').style.display = 'none';
    currentInspection = null;
    selectedLaneId = null;
    selectedLaneName = '';
    currentStageIndex = 0;
    stagesData = [];
    allDefects = [];
}
