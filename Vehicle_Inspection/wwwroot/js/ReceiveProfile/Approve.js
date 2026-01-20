// ========================================
// FILE: Approve.js
// MỤC ĐÍCH: Xử lý xét duyệt hồ sơ
// ========================================

// ========== GLOBAL VARIABLES ==========
let ownerId = null;
let vehicleId = null;

// ========== KHỞI TẠO TRANG ==========
document.addEventListener('DOMContentLoaded', async function () {
    console.log('🚀 Initializing Approve page');

    // Load data từ URL params
    await loadApprovalData();

    // Load danh sách dây chuyền
    await loadLanes();

    // Auto-generate inspection code
    generateInspectionCode();
});

// ========== LOAD DATA TỪ URL ==========
async function loadApprovalData() {
    const urlParams = new URLSearchParams(window.location.search);
    const cccd = urlParams.get('cccd');
    const plateNo = urlParams.get('plateNo');

    if (!cccd && !plateNo) {
        showNotification('error', 'Thiếu thông tin để xét duyệt');
        setTimeout(() => {
            window.location.href = '/receive-profile';
        }, 2000);
        return;
    }

    const loadingState = document.getElementById('loading-state');
    const formContent = document.getElementById('form-content');

    if (loadingState) loadingState.style.display = 'flex';
    if (formContent) formContent.style.display = 'none';

    try {
        const url = `/api/receive-profile/search?cccd=${encodeURIComponent(cccd || '')}&plateNo=${encodeURIComponent(plateNo || '')}`;
        console.log('📡 Loading data from:', url);

        const response = await fetch(url);
        const data = await response.json();

        if (loadingState) loadingState.style.display = 'none';
        if (formContent) formContent.style.display = 'block';

        if (data.success) {
            ownerId = data.data.owner.ownerId;
            vehicleId = data.data.vehicle.vehicleId;

            // Populate summary fields
            setFieldValue('owner-id', ownerId);
            setFieldValue('vehicle-id', vehicleId);
            setFieldValue('owner-fullname', data.data.owner.fullName);
            setFieldValue('owner-cccd', data.data.owner.cccd || data.data.owner.taxCode || '');
            setFieldValue('vehicle-plate', data.data.vehicle.plateNo);
            setFieldValue('vehicle-type', data.data.vehicle.vehicleType || '');

            console.log('✅ Data loaded:', { ownerId, vehicleId });
        } else {
            showNotification('error', data.message);
            setTimeout(() => {
                window.location.href = '/receive-profile';
            }, 2000);
        }
    } catch (error) {
        console.error('❌ Load data error:', error);
        if (loadingState) loadingState.style.display = 'none';
        showNotification('error', 'Không thể tải dữ liệu');
    }
}

// ========== LOAD DANH SÁCH DÂY CHUYỀN ==========
async function loadLanes() {
    try {
        // ✅ SỬA: Đổi từ /api/inspection/lanes → /api/approve/lanes
        const response = await fetch('/api/approve/lanes');
        const data = await response.json();

        if (data.success && data.data) {
            const laneSelect = document.getElementById('lane-id');
            if (laneSelect) {
                laneSelect.innerHTML = '<option value="">-- Chọn dây chuyền --</option>';
                data.data.forEach(lane => {
                    const option = document.createElement('option');
                    option.value = lane.laneId;
                    option.textContent = `${lane.laneCode} - ${lane.laneName}`;
                    laneSelect.appendChild(option);
                });
                console.log(`✅ Loaded ${data.data.length} lanes`);
            }
        }
    } catch (error) {
        console.error('❌ Load lanes error:', error);
        showNotification('warning', 'Không thể tải danh sách dây chuyền');
    }
}

// ========== GENERATE INSPECTION CODE ==========
function generateInspectionCode() {
    const now = new Date();
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');
    const hours = String(now.getHours()).padStart(2, '0');
    const minutes = String(now.getMinutes()).padStart(2, '0');
    const seconds = String(now.getSeconds()).padStart(2, '0');

    const code = `INS-${year}${month}${day}-${hours}${minutes}${seconds}`;
    setFieldValue('inspection-code', code);
    console.log('✅ Generated inspection code:', code);
}

// ========== VALIDATE FORM ==========
function validateForm() {
    const errors = [];

    const inspectionCode = getFieldValue('inspection-code');
    const inspectionType = getFieldValue('inspection-type');
    const laneId = getFieldValue('lane-id');

    if (!inspectionCode) {
        errors.push('Vui lòng nhập mã lượt kiểm định');
    }

    if (!inspectionType) {
        errors.push('Vui lòng chọn loại kiểm định');
    }

    if (!laneId) {
        errors.push('Vui lòng chọn dây chuyền');
    }

    if (!ownerId || !vehicleId) {
        errors.push('Thiếu thông tin chủ xe hoặc phương tiện');
    }

    return errors;
}

// ========== SUBMIT APPROVAL ==========
async function submitApproval() {
    try {
        console.log('💾 ========== BẮT ĐẦU XÉT DUYỆT ==========');

        // Validate
        const errors = validateForm();
        if (errors.length > 0) {
            showNotification('error', errors.join('<br>'));
            return;
        }

        // Collect data
        const requestData = {
            InspectionCode: getFieldValue('inspection-code'),
            VehicleId: parseInt(vehicleId),
            OwnerId: ownerId,
            InspectionType: getFieldValue('inspection-type'),
            LaneId: parseInt(getFieldValue('lane-id')),
            Status: 1, // RECEIVED
            Notes: getFieldValue('inspection-notes'),
            CreatedAt: new Date().toISOString(),
            IsDeleted: false
        };

        console.log('📤 Request data:', requestData);

        // Show loading
        const submitBtn = document.querySelector('.btn-save');
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<i class="bi bi-hourglass-split"></i> Đang xử lý...';
        }

        // ✅ SỬA: Đổi từ /api/inspection/approve → /api/approve/approve
        const response = await fetch('/api/approve/approve', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(requestData)
        });

        const data = await response.json();
        console.log('📊 Response:', data);

        // Reset button
        if (submitBtn) {
            submitBtn.disabled = false;
            submitBtn.innerHTML = '<i class="bi bi-check-circle"></i> Xác nhận xét duyệt';
        }

        if (data.success) {
            showNotification('success', data.message || 'Xét duyệt thành công');

            // Redirect về trang inspection hoặc index sau 2 giây
            setTimeout(() => {
                window.location.href = '/receive-profile';
            }, 2000);
        } else {
            showNotification('error', data.message || 'Xét duyệt thất bại');
        }

    } catch (error) {
        console.error('❌ Submit error:', error);
        showNotification('error', 'Có lỗi xảy ra khi xét duyệt');

        // Reset button
        const submitBtn = document.querySelector('.btn-save');
        if (submitBtn) {
            submitBtn.disabled = false;
            submitBtn.innerHTML = '<i class="bi bi-check-circle"></i> Xác nhận xét duyệt';
        }
    }
}

// ========== CANCEL APPROVAL ==========
function cancelApprove() {
    if (confirm('Bạn có chắc muốn hủy xét duyệt?')) {
        window.location.href = '/receive-profile';
    }
}

// ========== HELPER FUNCTIONS ==========
function setFieldValue(fieldId, value) {
    const field = document.getElementById(fieldId);
    if (field) {
        field.value = value || '';
    }
}

function getFieldValue(fieldId, defaultValue = '') {
    const field = document.getElementById(fieldId);
    return field?.value?.trim() || defaultValue;
}

function showNotification(type, message) {
    const container = document.getElementById('notification-container');
    if (!container) {
        console.log(`${type.toUpperCase()}: ${message}`);
        return;
    }

    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;

    const iconMap = {
        'success': 'check-circle',
        'error': 'x-circle',
        'warning': 'exclamation-triangle',
        'info': 'info-circle'
    };

    const icon = iconMap[type] || 'info-circle';

    notification.innerHTML = `
        <i class="bi bi-${icon}"></i>
        <span>${message}</span>
    `;

    container.appendChild(notification);

    setTimeout(() => {
        notification.style.opacity = '0';
        setTimeout(() => notification.remove(), 300);
    }, 5000);
}

console.log('✅ Approve.js loaded');