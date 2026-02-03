// ========================================
// FILE: Approve.js - Simplified Logic
// MỤC ĐÍCH: Đơn giản hóa - chỉ hiển thị thông tin, backend tự xử lý
// ========================================

// ========== GLOBAL VARIABLES ==========
let ownerId = null;
let vehicleId = null;

// ========== KHỞI TẠO TRANG ==========
document.addEventListener('DOMContentLoaded', async function () {
    console.log('🚀 Initializing Approve page - Simplified Logic');

    // Load data từ URL params
    await loadApprovalData();

    // Auto-generate inspection code
    generateInspectionCode();

    // Lấy thông tin hồ sơ mới nhất (để hiển thị UI)
    await loadLatestInspection();
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

// ========== LẤY THÔNG TIN HỒ SƠ MỚI NHẤT ==========
async function loadLatestInspection() {
    try {
        console.log('🔍 Loading latest inspection info...');

        const historyInfo = document.getElementById('history-info');
        if (historyInfo) {
            historyInfo.innerHTML = '<div class="loading-indicator"><i class="bi bi-hourglass-split"></i> Đang tải thông tin...</div>';
        }

        const response = await fetch(`/api/approve/latest?vehicleId=${vehicleId}`);
        const data = await response.json();

        console.log('📊 Latest inspection:', data);

        if (data.success) {
            displayInspectionInfo(data.data);

            // Enable submit button
            const submitBtn = document.getElementById('submit-btn');
            if (submitBtn) {
                submitBtn.disabled = false;
            }
        } else {
            showNotification('error', data.message || 'Không thể tải thông tin');
            if (historyInfo) {
                historyInfo.innerHTML = `
                    <div class="alert alert-danger">
                        <i class="bi bi-exclamation-triangle"></i> ${data.message || 'Lỗi tải dữ liệu'}
                    </div>
                `;
            }
        }
    } catch (error) {
        console.error('❌ Load latest error:', error);
        const historyInfo = document.getElementById('history-info');
        if (historyInfo) {
            historyInfo.innerHTML = `
                <div class="alert alert-danger">
                    <i class="bi bi-x-circle"></i> Không thể kết nối server
                </div>
            `;
        }
    }
}

// ========== HIỂN THỊ THÔNG TIN ==========
function displayInspectionInfo(data) {
    const historyInfo = document.getElementById('history-info');
    if (!historyInfo) return;

    let html = '<div class="inspection-info">';

    // ========== TRƯỜNG HỢP 1: CHƯA CÓ HỒ SƠ ==========
    if (!data.hasInspection) {
        html += `
            <div class="info-box info-first">
                <div class="info-icon">
                    <i class="bi bi-star"></i>
                </div>
                <div class="info-content">
                    <h4>Đăng kiểm lần đầu</h4>
                    <p>Xe chưa có hồ sơ kiểm định trước đó</p>
                    <span class="badge badge-success">
                        <i class="bi bi-plus-circle"></i> Tạo hồ sơ mới với FIRST
                    </span>
                </div>
            </div>
        `;
    }
    // ========== TRƯỜNG HỢP 2: CÓ HỒ SƠ TRƯỚC ==========
    else {
        const latest = data.latestInspection;

        if (data.action === 'UPDATE') {
            // Hồ sơ không đạt → Cập nhật
            html += `
                <div class="info-box info-update">
                    <div class="info-icon">
                        <i class="bi bi-arrow-repeat"></i>
                    </div>
                    <div class="info-content">
                        <h4>Cập nhật hồ sơ tái kiểm</h4>
                        <p>${data.message}</p>
                        <div class="inspection-details">
                            <span class="detail-item"><strong>Mã:</strong> ${latest.inspectionCode}</span>
                            <span class="detail-item"><strong>Loại:</strong> ${getInspectionTypeLabel(latest.inspectionType)}</span>
                            <span class="detail-item"><strong>Số lần kiểm lại:</strong> ${latest.count_Re || 0}</span>
                        </div>
                        <span class="badge badge-warning">
                            <i class="bi bi-arrow-repeat"></i> Cập nhật hồ sơ hiện tại
                        </span>
                    </div>
                </div>
            `;
        } else if (data.action === 'CREATE') {
            // Đã cấp GCN → Tạo mới
            html += `
                <div class="info-box info-periodic">
                    <div class="info-icon">
                        <i class="bi bi-arrow-repeat"></i>
                    </div>
                    <div class="info-content">
                        <h4>Kiểm định định kỳ</h4>
                        <p>${data.message}</p>
                        <div class="inspection-details">
                            <span class="detail-item"><strong>Mã hồ sơ trước:</strong> ${latest.inspectionCode}</span>
                            <span class="detail-item"><strong>Trạng thái:</strong> ${getStatusText(latest.status)}</span>
                        </div>
                        <span class="badge badge-info">
                            <i class="bi bi-plus-circle"></i> Tạo hồ sơ mới với PERIODIC
                        </span>
                    </div>
                </div>
            `;
        } else {
            // Trạng thái không hợp lệ
            html += `
                <div class="info-box info-error">
                    <div class="info-icon">
                        <i class="bi bi-exclamation-triangle"></i>
                    </div>
                    <div class="info-content">
                        <h4>Không thể xét duyệt</h4>
                        <p>${data.message}</p>
                        <div class="inspection-details">
                            <span class="detail-item"><strong>Mã:</strong> ${latest.inspectionCode}</span>
                            <span class="detail-item"><strong>Trạng thái:</strong> ${getStatusText(latest.status)}</span>
                        </div>
                        <span class="badge badge-danger">
                            <i class="bi bi-x-circle"></i> Cần hoàn thành quy trình hiện tại
                        </span>
                    </div>
                </div>
            `;

            // Disable submit button
            const submitBtn = document.getElementById('submit-btn');
            if (submitBtn) {
                submitBtn.disabled = true;
            }
        }
    }

    html += '</div>';
    historyInfo.innerHTML = html;
}

// ========== HELPER: GET STATUS TEXT ==========
function getStatusText(status) {
    const statusMap = {
        0: 'Pending',
        1: 'Received',
        2: 'Approved',
        3: 'In Progress',
        4: 'Completed',
        5: 'Passed',
        6: 'Failed',
        7: 'Certified'
    };
    return statusMap[status] || 'Unknown';
}

// ========== HELPER: GET INSPECTION TYPE LABEL ==========
function getInspectionTypeLabel(type) {
    const labels = {
        'FIRST': 'Đăng kiểm lần đầu',
        'PERIODIC': 'Kiểm định định kỳ'
    };
    return labels[type] || type;
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
            Notes: getFieldValue('inspection-notes')
        };

        console.log('📤 Request data:', requestData);

        // Show loading
        const submitBtn = document.getElementById('submit-btn');
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<i class="bi bi-hourglass-split"></i> Đang xử lý...';
        }

        // Send request
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
            submitBtn.innerHTML = '<i class="bi bi-check-circle"></i> Xác nhận duyệt';
        }

        if (data.success) {
            showNotification('success', data.message);

            // Redirect về trang index sau 2 giây
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
        const submitBtn = document.getElementById('submit-btn');
        if (submitBtn) {
            submitBtn.disabled = false;
            submitBtn.innerHTML = '<i class="bi bi-check-circle"></i> Xác nhận duyệt';
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

console.log('Approve.js - Simplified Logic loaded');