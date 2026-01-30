// ========================================
// FILE: Approve.js - Auto Detection with RE_INSPECTION Logic
// MỤC ĐÍCH: Tự động xác định loại kiểm định và xử lý tái kiểm
// ========================================

// ========== GLOBAL VARIABLES ==========
let ownerId = null;
let vehicleId = null;
let detectedInspectionType = null;
let needCreateNew = false;
let updatedInspectionId = null;

// ========== KHỞI TẠO TRANG ==========
document.addEventListener('DOMContentLoaded', async function () {
    console.log('🚀 Initializing Approve page with RE_INSPECTION Logic');

    // Load data từ URL params
    await loadApprovalData();

    // Auto-generate inspection code
    generateInspectionCode();

    // Phân tích lịch sử và xác định loại kiểm định
    await detectInspectionType();
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

// ========== PHÁT HIỆN LOẠI KIỂM ĐỊNH VÀ XỬ LÝ TÁI KIỂM ==========
async function detectInspectionType() {
    try {
        console.log('🔍 ========== BẮT ĐẦU PHÂN TÍCH LỊCH SỬ ==========');
        console.log('📋 VehicleId:', vehicleId);

        const historyInfo = document.getElementById('history-info');
        if (historyInfo) {
            historyInfo.innerHTML = '<div class="loading-indicator"><i class="bi bi-hourglass-split"></i> Đang phân tích lịch sử kiểm định...</div>';
        }

        // Gọi API để lấy lịch sử kiểm định và xử lý tái kiểm
        const response = await fetch(`/api/approve/detect-type?vehicleId=${vehicleId}`);
        const data = await response.json();

        console.log('📊 Detection result:', data);

        if (data.success) {
            detectedInspectionType = data.data.inspectionType;
            needCreateNew = data.data.needCreateNew;
            updatedInspectionId = data.data.updatedInspectionId;
            const history = data.data.history;

            console.log('✅ Inspection type detected:', detectedInspectionType);
            console.log('🆕 Need create new:', needCreateNew);
            console.log('🔄 Updated inspection ID:', updatedInspectionId);

            // Hiển thị kết quả phân tích
            displayInspectionTypeResult(data.data);

            // Cập nhật form
            setFieldValue('inspection-type-value', detectedInspectionType);
            setFieldValue('inspection-type-display', getInspectionTypeLabel(detectedInspectionType));

            const reasonElement = document.getElementById('inspection-type-reason');
            if (reasonElement) {
                reasonElement.textContent = data.data.reason;
                reasonElement.style.color = '#28a745';
                reasonElement.style.fontWeight = '600';
            }

            // Cập nhật button text
            const submitBtn = document.getElementById('submit-btn');
            if (submitBtn) {
                submitBtn.disabled = false;

                if (needCreateNew) {
                    submitBtn.innerHTML = '<i class="bi bi-check-circle"></i> Xác nhận tạo hồ sơ mới';
                } else {
                    submitBtn.innerHTML = '<i class="bi bi-arrow-repeat"></i> Xác nhận tái kiểm';
                }
            }

            // Hiển thị thông báo nếu đã cập nhật hồ sơ cũ
            if (updatedInspectionId) {
                showNotification('info', `Đã cập nhật hồ sơ #${updatedInspectionId} để tái kiểm`);
            }
        } else {
            showNotification('error', data.message || 'Không thể xác định loại kiểm định');

            if (historyInfo) {
                historyInfo.innerHTML = `
                    <div class="alert alert-danger">
                        <i class="bi bi-exclamation-triangle"></i>
                        <strong>Lỗi:</strong> ${data.message || 'Không thể phân tích lịch sử'}
                    </div>
                `;
            }
        }
    } catch (error) {
        console.error('❌ Detect type error:', error);
        showNotification('error', 'Có lỗi xảy ra khi phân tích lịch sử');

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

// ========== HIỂN THỊ KẾT QUẢ PHÂN TÍCH ==========
function displayInspectionTypeResult(data) {
    const historyInfo = document.getElementById('history-info');
    if (!historyInfo) return;

    let html = '<div class="inspection-analysis">';

    // Hiển thị kết quả chính
    html += `
        <div class="analysis-result ${getResultClass(data.inspectionType)}">
            <div class="result-icon">
                <i class="bi ${getResultIcon(data.inspectionType)}"></i>
            </div>
            <div class="result-content">
                <h4>Kết quả phân tích</h4>
                <p class="result-type">${getInspectionTypeLabel(data.inspectionType)}</p>
                <p class="result-reason">${data.reason}</p>
                ${data.needCreateNew ?
            '<span class="badge badge-info"><i class="bi bi-plus-circle"></i> Cần tạo hồ sơ mới</span>' :
            '<span class="badge badge-warning"><i class="bi bi-arrow-repeat"></i> Cập nhật hồ sơ cũ</span>'}
            </div>
        </div>
    `;

    // Hiển thị lịch sử (nếu có)
    if (data.history && data.history.length > 0) {
        html += '<div class="history-section">';
        html += '<h5><i class="bi bi-clock-history"></i> Lịch sử kiểm định gần đây</h5>';
        html += '<div class="history-list">';

        data.history.forEach((item, index) => {
            const statusBadge = getStatusBadge(item.status);
            const date = new Date(item.createdAt).toLocaleDateString('vi-VN');
            const isUpdated = item.inspectionId === data.updatedInspectionId;

            html += `
                <div class="history-item ${isUpdated ? 'item-updated' : ''}">
                    <div class="history-icon">
                        <i class="bi ${isUpdated ? 'bi-arrow-repeat text-warning' : 'bi-check-circle'}"></i>
                    </div>
                    <div class="history-content">
                        <div class="history-header">
                            <span class="history-code">${item.inspectionCode}</span>
                            ${statusBadge}
                            ${isUpdated ? '<span class="badge badge-warning ml-2"><i class="bi bi-arrow-repeat"></i> Đã cập nhật</span>' : ''}
                        </div>
                        <div class="history-details">
                            <span><i class="bi bi-calendar"></i> ${date}</span>
                            <span><i class="bi bi-tag"></i> ${getInspectionTypeLabel(item.inspectionType)}</span>
                            ${item.Count_Re > 0 ? `<span><i class="bi bi-arrow-repeat"></i> Tái kiểm: ${item.Count_Re} lần</span>` : ''}
                        </div>
                    </div>
                </div>
            `;
        });

        html += '</div></div>';
    } else {
        html += `
            <div class="no-history">
                <i class="bi bi-info-circle"></i>
                <p>Chưa có lịch sử kiểm định trước đó</p>
            </div>
        `;
    }

    html += '</div>';

    historyInfo.innerHTML = html;
}

// ========== HELPER: GET STATUS BADGE ==========
function getStatusBadge(status) {
    const badges = {
        0: '<span class="badge badge-secondary">Nháp</span>',
        1: '<span class="badge badge-info">Đã tiếp nhận</span>',
        2: '<span class="badge badge-primary">Đã thu phí</span>',
        3: '<span class="badge badge-warning">Đang kiểm định</span>',
        4: '<span class="badge badge-info">Hoàn thành KĐ</span>',
        5: '<span class="badge badge-success">Đạt</span>',
        6: '<span class="badge badge-danger">Không đạt</span>',
        7: '<span class="badge badge-success">Đã cấp GCN</span>',
        8: '<span class="badge badge-secondary">Đã hủy</span>'
    };
    return badges[status] || '<span class="badge badge-secondary">N/A</span>';
}

// ========== HELPER: GET RESULT CLASS ==========
function getResultClass(type) {
    const classes = {
        'FIRST': 'result-first',
        'PERIODIC': 'result-periodic',
        'RE_INSPECTION': 'result-reinspection'
    };
    return classes[type] || '';
}

// ========== HELPER: GET RESULT ICON ==========
function getResultIcon(type) {
    const icons = {
        'FIRST': 'bi-star',
        'PERIODIC': 'bi-arrow-repeat',
        'RE_INSPECTION': 'bi-tools'
    };
    return icons[type] || 'bi-question-circle';
}

// ========== HELPER: GET INSPECTION TYPE LABEL ==========
function getInspectionTypeLabel(type) {
    const labels = {
        'FIRST': 'Đăng kiểm lần đầu',
        'PERIODIC': 'Kiểm định định kỳ',
        'RE_INSPECTION': 'Tái kiểm'
    };
    return labels[type] || 'Không xác định';
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
    const inspectionType = getFieldValue('inspection-type-value');

    // Nếu là cập nhật hồ sơ cũ, không cần inspection code mới
    if (needCreateNew) {
        if (!inspectionCode) {
            errors.push('Vui lòng tạo mã lượt kiểm định');
        }
    }

    if (!inspectionType) {
        errors.push('Chưa xác định được loại kiểm định');
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
            InspectionType: getFieldValue('inspection-type-value'),
            Notes: getFieldValue('inspection-notes'),
            UpdatedInspectionId: updatedInspectionId // Truyền ID hồ sơ đã update (nếu có)
        };

        console.log('📤 Request data:', requestData);
        console.log('🆕 Need create new:', needCreateNew);

        // Show loading
        const submitBtn = document.getElementById('submit-btn');
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<i class="bi bi-hourglass-split"></i> Đang xử lý...';
        }

        // Send request
        const response = await fetch('/api/approve/create', {
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
            if (needCreateNew) {
                submitBtn.innerHTML = '<i class="bi bi-check-circle"></i> Xác nhận tạo hồ sơ mới';
            } else {
                submitBtn.innerHTML = '<i class="bi bi-arrow-repeat"></i> Xác nhận tái kiểm';
            }
        }

        if (data.success) {
            const message = data.data.isUpdated ?
                'Đã cập nhật hồ sơ để tái kiểm thành công' :
                'Tạo hồ sơ kiểm định mới thành công';

            showNotification('success', message);

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
            if (needCreateNew) {
                submitBtn.innerHTML = '<i class="bi bi-check-circle"></i> Xác nhận tạo hồ sơ mới';
            } else {
                submitBtn.innerHTML = '<i class="bi bi-arrow-repeat"></i> Xác nhận tái kiểm';
            }
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

console.log('Approve.js with RE_INSPECTION Logic loaded');