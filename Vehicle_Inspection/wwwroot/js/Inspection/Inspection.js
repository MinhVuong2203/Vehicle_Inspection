// Biến lưu trữ dữ liệu
let inspectionRecords = [];
let filteredRecords = [];
let allLanes = [];

// Load dữ liệu từ server khi trang được tải
document.addEventListener('DOMContentLoaded', function() {
    loadInspectionRecords();
    loadAllLanes();
});


// ✅ THÊM: Load tất cả dây chuyền (fallback)
async function loadAllLanes() {
    try {
        const response = await fetch('/Inspection/GetInspectionLanes', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            const result = await response.json();
            if (result.success && result.data) {
                allLanes = result.data;
                console.log(`Loaded ${allLanes.length} lanes`);
            }
        }
    } catch (error) {
        console.error('Error loading all lanes:', error);
    }
}

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

    showFullScreenLoading('Đang tải chi tiết hồ sơ...');
    
    try {
        // Load thông tin chi tiết từ server nếu cần
        const detailRecord = await loadInspectionDetail(inspectionId);
        if (detailRecord) {
            showDetail(detailRecord);
        }
    } finally {
        // ✅ HIDE LOADING
        hideFullScreenLoading();
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
let suitableLanes = []; // ✅ THÊM

// Wrapper function để mở modal phân công bằng ID
async function openAssignLaneById(inspectionId) {
    showFullScreenLoading('Đang tải thông tin phân công...');

    try {
        const record = findRecordById(inspectionId);
        if (record) {
            await openAssignLane(record);
        }
    } finally {
        hideFullScreenLoading();
    }
}

// Mở modal phân công dây chuyền
async function openAssignLane(record) {
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

    // ✅ LOAD DÂY CHUYỀN PHÙ HỢP TỪ DATABASE
    if (record.vehicleTypeId) {
        await loadSuitableLanes(record.vehicleTypeId);
    } else {
        console.warn('VehicleTypeId not found, showing all lanes');
        suitableLanes = [];
    }

    // Render lại danh sách dây chuyền
    renderLaneCards();

    // Nếu đã có dây chuyền, highlight card
    if (assignedLaneId) {
        setTimeout(() => {
            highlightSelectedLane(assignedLaneId);
            document.getElementById('btnConfirmAssign').disabled = false;
        }, 100);
    }

    document.getElementById('assignLaneModal').style.display = 'block';
}

// ✅ THÊM: Load dây chuyền phù hợp từ database
async function loadSuitableLanes(vehicleTypeId) {
    try {
        console.log(`Loading suitable lanes for VehicleTypeId: ${vehicleTypeId}`);

        const response = await fetch(`/Inspection/GetSuitableLanes?vehicleTypeId=${vehicleTypeId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error('Không thể tải danh sách dây chuyền');
        }

        const result = await response.json();

        if (result.success && result.data) {
            suitableLanes = result.data;
            console.log(`Loaded ${suitableLanes.length} suitable lanes:`, suitableLanes);
        } else {
            console.warn('No suitable lanes found');
            suitableLanes = [];
        }
    } catch (error) {
        console.error('Error loading suitable lanes:', error);
        alert('Không thể tải danh sách dây chuyền phù hợp. Hiển thị tất cả dây chuyền.');
        suitableLanes = [];
    }
}

// ✅ THÊM: Render lại lane cards
function renderLaneCards() {
    const laneCardsContainer = document.querySelector('.lane-cards');
    
    if (!laneCardsContainer) {
        console.error('Lane cards container not found');
        return;
    }

    laneCardsContainer.innerHTML = '';

    const lanesToShow = suitableLanes.length > 0 ? suitableLanes : allLanes;

    if (lanesToShow.length === 0) {
        laneCardsContainer.innerHTML = `
            <div style="grid-column: 1/-1; text-align: center; padding: 40px; color: #dc3545;">
                <i class="fa-solid fa-exclamation-triangle" style="font-size: 48px;"></i>
                <p style="margin-top: 15px; font-size: 16px;">Không có dây chuyền phù hợp với loại xe này</p>
            </div>
        `;
        return;
    }

    lanesToShow.forEach(lane => {
        const isSuitable = suitableLanes.length === 0 || suitableLanes.some(l => l.laneId === lane.laneId);
        
        const card = document.createElement('div');
        card.className = `assign-lane-card ${!isSuitable ? 'disabled' : ''}`;
        card.setAttribute('data-lane-id', lane.laneId);
        
        if (isSuitable) {
            card.onclick = () => selectAssignLane(lane.laneId, lane.laneName);
        }

        card.innerHTML = `
            <div class="lane-number">${lane.laneId}</div>
            <h4>${lane.laneCode}</h4>
            <p class="lane-desc">${lane.laneName}</p>
            <div class="lane-status ${isSuitable ? 'available' : 'unavailable'}">
                <i class="fa-solid fa-${isSuitable ? 'circle-check' : 'circle-xmark'}"></i> 
                ${isSuitable ? 'Phù hợp' : 'Không phù hợp'}
            </div>
        `;

        laneCardsContainer.appendChild(card);
    });
}

// ✅ CẬP NHẬT: Highlight lane đã chọn
function highlightSelectedLane(laneId) {
    document.querySelectorAll('.assign-lane-card').forEach(card => {
        card.classList.remove('selected');
        if (parseInt(card.getAttribute('data-lane-id')) === laneId) {
            card.classList.add('selected');
        }
    });
}

// Chọn dây chuyền trong modal phân công
function selectAssignLane(laneId, laneName) {
    console.log('=== selectAssignLane ===');
    console.log('LaneId:', laneId);
    console.log('LaneName:', laneName);

    assignedLaneId = laneId;
    assignedLaneName = laneName;

    // Highlight card được chọn
    highlightSelectedLane(laneId);

    // Enable nút xác nhận
    document.getElementById('btnConfirmAssign').disabled = false;

    console.log('✅ Lane selected successfully');
}

// Xác nhận phân công dây chuyền
async function confirmAssignLane() {
    if (!assignedLaneId) {
        alert('Vui lòng chọn dây chuyền!');
        return;
    }

    if (!assigningInspection) {
        alert('Không tìm thấy thông tin hồ sơ!');
        return;
    }

    const note = document.getElementById('assignNote').value;

    //SHOW LOADING
    showFullScreenLoading('Đang phân công dây chuyền...');

    try {
        console.log('=== Assigning Lane ===');
        console.log('InspectionId:', assigningInspection.inspectionId);
        console.log('LaneId:', assignedLaneId);

        const response = await fetch('/Inspection/AssignLane', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify({
                inspectionId: assigningInspection.inspectionId,
                laneId: assignedLaneId,
                note: note
            })
        });

        console.log('Response status:', response.status);

        if (!response.ok) {
            throw new Error('Không thể phân công dây chuyền');
        }

        const result = await response.json();
        console.log('Assign result:', result);

        if (result.success) {
            alert(`Đã phân công hồ sơ ${assigningInspection.inspectionCode} vào ${assignedLaneName}!`);
            closeAssignLane();

            // Reload lại danh sách
            await loadInspectionRecords();
        } else {
            alert((result.message || 'Có lỗi xảy ra'));
        }
    } catch (error) {
        console.error('Error assigning lane:', error);
        alert('Không thể phân công dây chuyền. Vui lòng thử lại.\n\nLỗi: ' + error.message);
    } finally {
        //HIDE LOADING
        hideFullScreenLoading();
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
async function openInspectionProcessById(inspectionId) {
    //SHOW LOADING
    showFullScreenLoading('Đang tải quy trình kiểm định...');

    try {
        const record = findRecordById(inspectionId);
        if (record) {
            await openInspectionProcess(record);
        }
    } finally {
        //HIDE LOADING
        hideFullScreenLoading();
    }
}

// Mở modal quy trình kiểm định
async function openInspectionProcess(record) {
    currentInspection = record;
    selectedLaneId = record.laneId;
    selectedLaneName = record.laneName;
    currentStageIndex = 0;

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

    // ✅ LOAD STAGES TỪ DATABASE
    console.log('Loading stages from database...');

    const dbStages = await window.InspectionStageLoader.loadStages(record.inspectionId);

    if (!dbStages || dbStages.length === 0) {
        alert('Không tìm thấy quy trình kiểm định cho dây chuyền này.');
        return;
    }

    // Convert sang UI format
    stagesData = window.InspectionStageLoader.convertToUIFormat(dbStages);

    // Build stage items config
    Object.assign(stageItems, window.InspectionStageLoader.buildItemsConfig(dbStages));

    console.log('Loaded stages:', stagesData);
    console.log('Stage items config:', stageItems);

    allDefects = [];

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

        // ✅ KIỂM TRA STAGE CÓ ITEM APPLICABLE KHÔNG
        const isDisabled = !stage.hasApplicableItems;

        stageItem.className = 'stage-item';

        if (stage.status === 2) stageItem.classList.add('completed');
        if (stage.result === 2) stageItem.classList.add('failed');
        if (index === currentStageIndex) stageItem.classList.add('active');

        // ✅ THÊM CLASS DISABLED
        if (isDisabled) stageItem.classList.add('stage-disabled');

        let statusText = 'Chờ thực hiện';
        let statusClass = 'pending';

        if (isDisabled) {
            statusText = 'Không áp dụng';
            statusClass = 'not-applicable';
        } else if (stage.status === 2) {
            statusText = stage.result === 1 ? 'Đã hoàn thành' : 'Không đạt';
            statusClass = stage.result === 1 ? 'completed' : 'failed';
        } else if (stage.status === 1) {
            statusText = 'Đang thực hiện';
            statusClass = 'pending';
        }

        stageItem.innerHTML = `
            <div class="stage-icon">
                <i class="fa-solid ${isDisabled ? 'fa-ban' : (stage.status === 2 ? (stage.result === 1 ? 'fa-check' : 'fa-xmark') : 'fa-clock')}"></i>
            </div>
            <div class="stage-info">
                <h4>${index + 1}. ${stage.stageName}</h4>
                <p>
                    ${isDisabled
                ? `<i class="fa-solid fa-info-circle"></i> Không có item áp dụng cho loại xe này`
                : `KTV: ${stage.assignedUser || 'Chưa phân công'}`
            }
                </p>
            </div>
            <div class="stage-status ${statusClass}">${statusText}</div>
        `;

        // ✅ DISABLE CLICK NẾU KHÔNG CÓ ITEM
        if (!isDisabled) {
            stageItem.onclick = () => showStage(index);
        } else {
            stageItem.style.cursor = 'not-allowed';
            stageItem.style.opacity = '0.6';
            //stageItem.onclick = () => {
            //    alert(`⚠️ Công đoạn "${stage.stageName}" không có mục kiểm tra nào áp dụng cho loại xe này.`);
            //};
        }

        stagesList.appendChild(stageItem);
    });

    updateProgress();
}

// Render form nhập liệu cho giai đoạn
function renderStageForm(stage) {
    const form = document.getElementById('stageForm');
    form.innerHTML = '';

    const items = stageItems[stage.stageId] || [];

    items.forEach(item => {
        const formGroup = document.createElement('div');
        formGroup.className = 'form-group';

        // ✅ KIỂM TRA DISABLED
        const isDisabled = item.disabled === true;
        const disabledAttr = isDisabled ? 'disabled' : '';
        const disabledStyle = isDisabled ? 'background-color: #f0f0f0; cursor: not-allowed;' : '';

        let inputHtml = '';

        if (item.type === 'select') {
            inputHtml = `
                <select id="item_${item.id}" 
                        onchange="checkItemStandard(${item.id})"
                        ${disabledAttr}
                        style="${disabledStyle}">
                    <option value="">-- ${isDisabled ? 'Không áp dụng' : 'Chọn'} --</option>
                    ${!isDisabled && item.options ? item.options.map(opt => `<option value="${opt}">${opt}</option>`).join('') : ''}
                </select>
            `;
        } else if (item.type === 'number') {
            inputHtml = `
                <input type="number" 
                       id="item_${item.id}" 
                       step="0.1" 
                       placeholder="${isDisabled ? 'Không áp dụng cho loại xe này' : 'Nhập giá trị đo'}"
                       onchange="checkItemStandard(${item.id})"
                       ${disabledAttr}
                       style="${disabledStyle}">
            `;
        }

        formGroup.innerHTML = `
            <label>
                ${item.name} 
                ${!isDisabled ? '<span style="color: #dc3545;">*</span>' : '<span style="color: #6c757d;">(Không áp dụng)</span>'}
            </label>
            ${inputHtml}
            <div class="help-text">Tiêu chuẩn: ${item.standard}</div>
            <div id="result_${item.id}" style="margin-top: 5px;"></div>
        `;

        form.appendChild(formGroup);

        // ✅ LOAD GIÁ TRỊ ĐÃ LƯU (nếu có)
        if (stage.measurements && stage.measurements[item.id]) {
            const input = document.getElementById(`item_${item.id}`);
            if (input && !isDisabled) {
                input.value = stage.measurements[item.id];
                checkItemStandard(item.id);
            }
        }
    });

    // ✅ ENABLE NÚT LƯU
    const saveButton = document.querySelector('.stage-actions button:nth-child(2)');
    if (saveButton) {
        saveButton.disabled = false;
        saveButton.style.opacity = '1';
        saveButton.style.cursor = 'pointer';
    }
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


// Hiển thị chi tiết một giai đoạn
async function showStage(index) {
    currentStageIndex = index;
    const stage = stagesData[index];

    // ✅ KIỂM TRA STAGE CÓ ITEM APPLICABLE KHÔNG
    if (!stage.hasApplicableItems) {
        //alert(`⚠️ Công đoạn "${stage.stageName}" không có mục kiểm tra nào áp dụng cho loại xe này.\n\nVui lòng chuyển sang công đoạn khác.`);
        return;
    }

    document.getElementById('stageDetail').style.display = 'block';
    document.getElementById('inspectionConclusion').style.display = 'none';

    document.getElementById('stageName').textContent = stage.stageName;
    document.getElementById('stageDescription').textContent = `Công đoạn ${index + 1}/${stagesData.length}`;

    // ✅ KIỂM TRA QUYỀN TRƯỚC KHI RENDER FORM
    const hasPermission = await checkStagePermission(currentInspection.inspectionId, stage.stageId);

    if (!hasPermission) {
        renderStageFormDisabled(stage);
        showPermissionWarning();
    } else {
        renderStageForm(stage);
    }

    if (stage.result === 2) {
        document.getElementById('defectsSection').style.display = 'block';
        await loadStageDefects(stage.stageId);
    } else {
        document.getElementById('defectsSection').style.display = 'none';
    }

    document.querySelector('.stage-actions button:first-child').style.display = index === 0 ? 'none' : 'inline-flex';
    document.getElementById('btnNextStage').textContent = index === stagesData.length - 1 ? 'Kết Luận' : 'Tiếp Theo';

    renderStagesList();
}

// ✅ KIỂM TRA QUYỀN QUA API
async function checkStagePermission(inspectionId, stageId) {
    try {
        const response = await fetch(`/Inspection/CheckStagePermission?inspectionId=${inspectionId}&stageId=${stageId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            console.error('Cannot check permission');
            return false;
        }

        const result = await response.json();
        return result.success && result.hasPermission;
    } catch (error) {
        console.error('Error checking permission:', error);
        return false;
    }
}

// ✅ RENDER FORM Ở CHẾ ĐỘ DISABLED
function renderStageFormDisabled(stage) {
    const form = document.getElementById('stageForm');
    form.innerHTML = '';

    const items = stageItems[stage.stageId] || [];

    items.forEach(item => {
        const formGroup = document.createElement('div');
        formGroup.className = 'form-group';

        let inputHtml = '';
        const savedValue = stage.measurements && stage.measurements[item.id] 
            ? stage.measurements[item.id] 
            : '';

        if (item.type === 'select') {
            inputHtml = `
                <select id="item_${item.id}" disabled style="background-color: #f0f0f0; cursor: not-allowed;">
                    <option value="">-- Chọn --</option>
                    ${item.options.map(opt => `<option value="${opt}" ${savedValue === opt ? 'selected' : ''}>${opt}</option>`).join('')}
                </select>
            `;
        } else if (item.type === 'number') {
            inputHtml = `
                <input type="number" id="item_${item.id}" 
                       value="${savedValue}"
                       disabled
                       style="background-color: #f0f0f0; cursor: not-allowed;"
                       placeholder="Bạn không có quyền nhập liệu">
            `;
        }

        formGroup.innerHTML = `
            <label>${item.name} <span style="color: #dc3545;">*</span></label>
            ${inputHtml}
            <div class="help-text">Tiêu chuẩn: ${item.standard}</div>
            <div id="result_${item.id}" style="margin-top: 5px;"></div>
        `;

        form.appendChild(formGroup);
    });

    // ✅ DISABLE NÚT LƯU
    const saveButton = document.querySelector('.stage-actions button:nth-child(2)');
    if (saveButton) {
        saveButton.disabled = true;
        saveButton.style.opacity = '0.5';
        saveButton.style.cursor = 'not-allowed';
    }
}

// ✅ HIỂN THỊ CẢNH BÁO
function showPermissionWarning() {
    const form = document.getElementById('stageForm');
    
    const warningDiv = document.createElement('div');
    warningDiv.style.cssText = `
        background: #fff3cd;
        border: 2px solid #ffc107;
        border-radius: 8px;
        padding: 15px;
        margin-bottom: 20px;
        display: flex;
        align-items: center;
        gap: 10px;
    `;
    
    warningDiv.innerHTML = `
        <i class="fa-solid fa-lock" style="font-size: 24px; color: #856404;"></i>
        <div>
            <strong style="color: #856404;">Bạn không có quyền nhập liệu cho công đoạn này</strong>
            <p style="margin: 5px 0 0 0; color: #856404;">
                Chỉ nhân viên được phân công mới có thể nhập dữ liệu.
            </p>
        </div>
    `;
    
    form.insertBefore(warningDiv, form.firstChild);
}

// ✅ CẬP NHẬT HÀM LƯU ĐỂ HIỂN THỊ THÔNG BÁO RÕ RÀNG
async function saveStageResult() {
    const stage = stagesData[currentStageIndex];
    const items = stageItems[stage.stageId] || [];

    if (!stage.inspStageId) {
        alert('❌ Lỗi: Không tìm thấy InspStageId. Vui lòng load lại trang.');
        return;
    }

    let allFilled = true;
    const measurements = [];

    for (const item of items) {
        // ✅ BỎ QUA ITEM DISABLED
        if (item.disabled === true) {
            console.log(`⏭️ Skipping disabled item: ${item.name}`);
            continue;
        }

        const inputElement = document.getElementById(`item_${item.id}`);
        const value = inputElement?.value;

        if (!value) {
            allFilled = false;
            console.warn(`⚠️ Item ${item.name} is not filled`);
            continue;
        }

        let actualValue = null;
        let actualText = null;

        if (item.type === 'number') {
            actualValue = parseFloat(value);
        } else {
            actualText = value;
        }

        let isPassed = false;

        if (item.type === 'select') {
            isPassed = value === item.standard || value === item.options[0];
        } else if (item.type === 'number') {
            const numValue = parseFloat(value);
            isPassed = numValue >= (item.min || 0) && numValue <= (item.max || 999999);
        }

        const measurement = {
            itemId: item.id,
            itemCode: item.itemCode || `ITEM_${item.id}`,
            itemName: item.name,
            unit: item.unit,
            dataType: item.type === 'number' ? 'NUMBER' : 'TEXT',
            standardMin: item.min || null,
            standardMax: item.max || null,
            standardText: item.standard,
            actualValue: actualValue,
            actualText: actualText,
            isPassed: isPassed
        };

        if (!isPassed) {
            measurement.defectCategory = stage.stageName;
            measurement.defectDescription = `${item.name}: Giá trị đo ${value} ${item.unit || ''} không đạt tiêu chuẩn ${item.standard}`;
            measurement.defectSeverity = 2;
        }

        measurements.push(measurement);
    }

    if (!allFilled) {
        alert('⚠️ Vui lòng nhập đầy đủ tất cả các thông số bắt buộc!');
        return;
    }

    const requestData = {
        inspectionId: currentInspection.inspectionId,
        inspStageId: stage.inspStageId,
        stageId: stage.stageId,
        measurements: measurements,
        notes: null
    };

    showFullScreenLoading('Đang lưu kết quả...');

    try {
        const response = await fetch('/Inspection/SaveStageResult', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify(requestData)
        });

        if (!response.ok) {
            throw new Error('Không thể lưu kết quả');
        }

        const result = await response.json();

        if (result.success) {
            stage.status = 2;
            stage.result = measurements.every(m => m.isPassed) ? 1 : 2;
            stage.measurements = {};
            measurements.forEach(m => {
                stage.measurements[m.itemId] = m.actualValue || m.actualText;
            });

            alert('✅ Đã lưu kết quả công đoạn thành công!');

            if (stage.result === 2) {
                document.getElementById('defectsSection').style.display = 'block';
                await loadStageDefects(stage.stageId);
            }

            renderStagesList();
        } else {
            // ✅ HIỂN THỊ THÔNG BÁO CHI TIẾT
            if (result.message.includes('không có quyền')) {
                alert('🚫 BẠN KHÔNG CÓ QUYỀN NHẬP LIỆU!\n\n' +
                      '❌ Chỉ nhân viên được phân công cho công đoạn này mới có thể nhập dữ liệu.\n\n' +
                      '📞 Vui lòng liên hệ quản lý để được phân quyền.');
            } else if (result.message.includes('chưa đăng nhập')) {
                alert('🔒 BẠN CHƯA ĐĂNG NHẬP!\n\nVui lòng đăng nhập để tiếp tục.');
            } else {
                alert('❌ Lưu thất bại: ' + (result.message || 'Lỗi không xác định'));
            }
        }
    } catch (error) {
        console.error('Error saving stage result:', error);
        alert('❌ Không thể lưu kết quả. Vui lòng thử lại.\n\nLỗi: ' + error.message);
    } finally {
        hideFullScreenLoading();
    }
}

// ✅ THÊM HÀM LOAD DEFECTS TỪ DATABASE
async function loadStageDefects(stageId) {
    try {
        console.log(`Loading defects for InspectionId: ${currentInspection.inspectionId}, StageId: ${stageId}`);

        const response = await fetch(`/Inspection/GetStageDefects?inspectionId=${currentInspection.inspectionId}&stageId=${stageId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            console.error('Cannot load defects');
            return;
        }

        const result = await response.json();
        console.log('Defects result:', result);

        if (result.success && result.data) {
            // ✅ Xóa defects cũ của stage này
            allDefects = allDefects.filter(d => d.stageId !== stageId);

            // ✅ Thêm defects mới từ database
            result.data.forEach(defect => {
                allDefects.push({
                    defectId: defect.defectId,
                    stageId: stageId,
                    category: defect.defectCategory,
                    description: defect.defectDescription,
                    severity: defect.severity,
                    isFixed: defect.isFixed
                });
            });

            console.log(`✅ Loaded ${result.data.length} defects for stage ${stageId}`);

            // ✅ Render lại danh sách lỗi
            renderDefects(stageId);
        }
    } catch (error) {
        console.error('Error loading defects:', error);
    }
}

// Render danh sách lỗi
function renderDefects(stageId) {
    const defectsList = document.getElementById('defectsList');
    const stageDefects = allDefects.filter(d => d.stageId === stageId);

    defectsList.innerHTML = '';

    if (stageDefects.length === 0) {
        defectsList.innerHTML = `
            <div style="text-align: center; padding: 20px; color: #6c757d;">
                <i class="fa-solid fa-check-circle" style="font-size: 32px; color: #28a745;"></i>
                <p style="margin-top: 10px;">Không có lỗi nào được phát hiện</p>
            </div>
        `;
        return;
    }

    stageDefects.forEach((defect, index) => {
        const defectItem = document.createElement('div');
        defectItem.className = 'defect-item';

        const severityText = defect.severity === 3 ? 'Nghiêm trọng' :
            defect.severity === 2 ? 'Hư hỏng' : 'Khuyết điểm';
        const severityClass = defect.severity === 3 ? 'critical' :
            defect.severity === 2 ? 'major' : 'minor';

        defectItem.innerHTML = `
            <div class="defect-header">
                <strong>${defect.category}</strong>
                <div>
                    <span class="defect-severity ${severityClass}">
                        <i class="fa-solid fa-${defect.severity === 3 ? 'exclamation-triangle' : 'exclamation-circle'}"></i>
                        ${severityText}
                    </span>
                </div>
            </div>
            <p>${defect.description}</p>
            ${defect.isFixed ? '<div class="defect-fixed"><i class="fa-solid fa-wrench"></i> Đã sửa chữa</div>' : ''}
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
    // ✅ CHỈ ĐẾM STAGE CÓ ITEM APPLICABLE
    const applicableStages = stagesData.filter(s => s.hasApplicableItems);
    const completedCount = applicableStages.filter(s => s.status === 2).length;
    const totalCount = applicableStages.length;

    const percentage = totalCount > 0 ? (completedCount / totalCount) * 100 : 0;

    document.getElementById('progressFill').style.width = percentage + '%';
    document.getElementById('currentStageNum').textContent = completedCount;
    document.getElementById('totalStages').textContent = totalCount;
}

// Giai đoạn trước
function previousStage() {
    // ✅ TÌM STAGE TRƯỚC ĐÓ CÓ ITEM APPLICABLE
    let prevIndex = currentStageIndex - 1;

    while (prevIndex >= 0) {
        if (stagesData[prevIndex].hasApplicableItems) {
            showStage(prevIndex);
            return;
        }
        console.log(`⏮️ Skipping stage ${prevIndex} (${stagesData[prevIndex].stageName}) - No applicable items`);
        prevIndex--;
    }

    // ✅ NẾU KHÔNG CÓ STAGE NÀO TRƯỚC ĐÓ
    alert('⚠️ Đây là công đoạn đầu tiên có thể thực hiện.');
}

// Giai đoạn tiếp theo
function nextStage() {
    const stage = stagesData[currentStageIndex];

    // ✅ NẾU STAGE HIỆN TẠI CÓ ITEM VÀ CHƯA LƯU → CẢNH BÁO
    if (stage.hasApplicableItems && stage.status !== 2) {
        if (!confirm('Bạn chưa lưu kết quả công đoạn này. Tiếp tục?')) {
            return;
        }
    }

    // ✅ TÌM STAGE TIẾP THEO CÓ ITEM APPLICABLE
    let nextIndex = currentStageIndex + 1;

    while (nextIndex < stagesData.length) {
        if (stagesData[nextIndex].hasApplicableItems) {
            showStage(nextIndex);
            return;
        }
        console.log(`⏭️ Skipping stage ${nextIndex} (${stagesData[nextIndex].stageName}) - No applicable items`);
        nextIndex++;
    }

    // ✅ NẾU KHÔNG CÒN STAGE NÀO → HIỂN THỊ KẾT LUẬN
    showConclusion();
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
                        ${defect.severity === 3 ? 'Nghiêm trọng' : defect.severity === 2 ? 'Hư hỏng' : 'Khuyết điểm' }
                    </span>
                </div>
                <p>${defect.description}</p>
            `;
            allDefectsList.appendChild(defectItem);
        });
    }
}

function createFormField(item, existingValue) {
    const value = existingValue || '' ;

    if (item.type === 'select') {
        return `
            <select class="form-control" 
                    name="item_${item.id}" 
                    data-item-id="${item.id}"
                    data-item-code="${item.itemCode}"
                    data-item-name="${item.name}"
                    ${item.isRequired ? 'required' : ''}>
                ${item.options.map(opt => `
                    <option value="${opt}" 
                            ${value === opt ? 'selected' : ''}>
                        ${opt}
                    </option>
                `).join('')}
            </select>
        `;
    } else if (item.type === 'number') {
        return `
            <input type="number" 
                   class="form-control" 
                   name="item_${item.id}" 
                   data-item-id="${item.id}"
                   data-item-code="${item.itemCode}"
                   data-item-name="${item.name}"
                   value="${value}"
                   min="${item.min || 0}"
                   max="${item.max || 999999}"
                   step="0.01"
                   ${item.isRequired ? 'required' : ''}>
        `;
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
    const conclusionNote = document.getElementById('conclusionNote')?.value || '' ;

    // ✅ KIỂM TRA finalResult có được chọn không
    if (!finalResult) {
        alert('Vui lòng chọn kết luận cuối cùng!');
        return;
    }

    if (!confirm('Xác nhận hoàn thành kiểm định?')) {
        return;
    }

    showFullScreenLoading('Đang hoàn thành kiểm định...');

    try {
        console.log('=== Submitting Conclusion ===');
        console.log('InspectionId:', currentInspection.inspectionId);
        console.log('FinalResult:', finalResult);

        const response = await fetch('/Inspection/SubmitInspectionResult', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify({
                inspectionId: currentInspection.inspectionId,
                finalResult: parseInt(finalResult),
                conclusionNote: conclusionNote
            })
        });

        console.log('Response status:', response.status);

        if (!response.ok) {
            throw new Error('Không thể lưu kết quả kiểm định');
        }

        const result = await response.json();
        console.log('Submit result:', result);

        if (result.success) {
            alert('✅ Đã hoàn thành kiểm định!');
            closeInspectionProcess();
            await loadInspectionRecords();
        } else {
            alert('❌ ' + (result.message || 'Có lỗi xảy ra'));
        }
    } catch (error) {
        console.error('Error submitting inspection:', error);
        alert('❌ Không thể hoàn thành kiểm định. Vui lòng thử lại.\n\nLỗi: ' + error.message);
    } finally {
        hideFullScreenLoading();
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
