// ==================== GLOBAL STATE ====================
let currentData = null;
let originalData = null;
let provincesData = [];
let recentSearches = JSON.parse(localStorage.getItem('recentSearches') || '[]');

// ==================== INITIALIZE ====================
document.addEventListener('DOMContentLoaded', () => {
    loadProvincesData();
    loadRecentSearches();
    setupEventListeners();
});

// ==================== LOAD PROVINCES ====================
async function loadProvincesData() {
    try {
        const response = await fetch('/api/receive-profile/provinces');
        const result = await response.json();

        if (result.success) {
            provincesData = result.data;

            // 🔍 DEBUG: Xem cấu trúc JSON thực tế
            console.log('📦 RAW DATA:', result.data);
            console.log('📦 Type:', typeof result.data);
            console.log('📦 Is Array:', Array.isArray(result.data));

            if (Array.isArray(result.data) && result.data.length > 0) {
                console.log('📦 First province:', result.data[0]);
                console.log('📦 Keys of first province:', Object.keys(result.data[0]));
            }

            console.log('✅ Đã tải:', provincesData.length, 'tỉnh/thành phố');
        } else {
            console.error('❌ API không success:', result);
        }
    } catch (error) {
        console.error('❌ Lỗi tải provinces:', error);
        showNotification('Không thể tải dữ liệu tỉnh/thành phố', 'error');
    }
}

// ==================== POPULATE COMBOBOX ====================
function populateProvinceCombobox(selectedProvince = '') {
    const select = document.getElementById('owner-province');

    console.log('🔍 populateProvinceCombobox được gọi');
    console.log('🔍 provincesData:', provincesData);
    console.log('🔍 provincesData.length:', provincesData?.length);
    console.log('🔍 selectedProvince:', selectedProvince);

    if (!select) {
        console.error('❌ Không tìm thấy element #owner-province');
        return;
    }

    if (!provincesData || provincesData.length === 0) {
        console.error('❌ provincesData trống hoặc undefined');
        return;
    }

    select.innerHTML = '<option value="">-- Chọn Tỉnh/Thành phố --</option>';

    let count = 0;
    provincesData.forEach((province, index) => {
        console.log(`  Province[${index}]:`, province);

        const option = document.createElement('option');

        // Thử nhiều cách lấy tên tỉnh
        const provinceName = province.tentinhtp || province.Name || province.name || province.TenTinhTP;

        if (!provinceName) {
            console.warn(`  ⚠️ Province[${index}] không có tên:`, province);
            return;
        }

        option.value = provinceName;
        option.textContent = provinceName;
        option.selected = (provinceName === selectedProvince);
        select.appendChild(option);
        count++;
    });

    console.log(`✅ Đã thêm ${count} options vào combobox`);
    console.log(`✅ Tổng options trong select:`, select.options.length);
}

function populateWardCombobox(provinceName, selectedWard = '') {
    const select = document.getElementById('owner-ward');
    select.innerHTML = '<option value="">-- Chọn Phường/Xã --</option>';

    if (!provinceName) {
        select.disabled = true;
        return;
    }

    select.disabled = false;

    // Tìm tỉnh/thành phố theo tên TIẾNG VIỆT
    const province = provincesData.find(p =>
        p.tentinhtp === provinceName || p.Name === provinceName
    );

    if (!province) {
        console.warn('Không tìm thấy tỉnh:', provinceName);
        return;
    }

    // Duyệt qua các quận/huyện (cấu trúc: quanhuyen -> phuongxa)
    if (province.quanhuyen && Array.isArray(province.quanhuyen)) {
        province.quanhuyen.forEach(district => {
            if (district.phuongxa && Array.isArray(district.phuongxa)) {
                district.phuongxa.forEach(ward => {
                    const option = document.createElement('option');
                    option.value = ward.tenphuongxa;
                    option.textContent = `${ward.tenphuongxa} (${district.tenquanhuyen})`;
                    option.selected = (ward.tenphuongxa === selectedWard);
                    select.appendChild(option);
                });
            }
        });
    }

    console.log('✅ Đã populate ward cho:', provinceName);
}

// ==================== EVENT LISTENERS ====================
function setupEventListeners() {
    ['search-cccd', 'search-plate'].forEach(id => {
        document.getElementById(id).addEventListener('keypress', (e) => {
            if (e.key === 'Enter') searchProfile();
        });
    });

    document.getElementById('owner-province').addEventListener('change', function () {
        populateWardCombobox(this.value);
    });
}

// ==================== SEARCH ====================
async function searchProfile() {
    const cccd = document.getElementById('search-cccd').value.trim();
    const plateNo = document.getElementById('search-plate').value.trim();

    if (!cccd && !plateNo) {
        showNotification('Vui lòng nhập CCCD hoặc Biển số xe', 'warning');
        return;
    }

    showLoading();

    try {
        const params = new URLSearchParams();
        if (cccd) params.append('cccd', cccd);
        if (plateNo) params.append('plateNo', plateNo);

        const response = await fetch(`/api/receive-profile/search?${params}`);
        const result = await response.json();

        if (result.success) {
            currentData = result.data;
            originalData = JSON.parse(JSON.stringify(result.data));
            displayData(result.data);
            saveToRecentSearches(cccd, plateNo, result.searchType);
            showNotification(result.message, 'success');
        } else {
            showNoData();
            showNotification(result.message, 'error');
        }
    } catch (error) {
        console.error('❌ Search error:', error);
        showNoData();
        showNotification('Có lỗi xảy ra khi tìm kiếm', 'error');
    }
}

function clearSearch() {
    document.getElementById('search-cccd').value = '';
    document.getElementById('search-plate').value = '';
    currentData = null;
    originalData = null;
    showNoData();
}

// ==================== DISPLAY ====================
function showLoading() {
    document.getElementById('no-data-state').style.display = 'none';
    document.getElementById('data-display').style.display = 'none';
    document.getElementById('loading-state').style.display = 'flex';
}

function showNoData() {
    document.getElementById('loading-state').style.display = 'none';
    document.getElementById('data-display').style.display = 'none';
    document.getElementById('no-data-state').style.display = 'flex';
}

function displayData(data) {
    document.getElementById('loading-state').style.display = 'none';
    document.getElementById('no-data-state').style.display = 'none';
    document.getElementById('data-display').style.display = 'block';

    displayOwnerInfo(data.owner);
    displayVehicleInfo(data.vehicle);
    displaySpecificationInfo(data.specification);

    document.getElementById('data-display').scrollIntoView({ behavior: 'smooth' });
}

function displayOwnerInfo(owner) {
    document.getElementById('owner-id').value = owner.ownerId;
    document.getElementById('owner-type').value = owner.ownerType;
    toggleOwnerType();

    const fields = {
        'owner-fullname': owner.fullName,
        'owner-cccd': owner.cccd,
        'owner-company': owner.companyName,
        'owner-taxcode': owner.taxCode,
        'owner-phone': owner.phone,
        'owner-phone2': owner.phone,
        'owner-email': owner.email,
        'owner-address': owner.address,
        'owner-created': formatDate(owner.createdAt)
    };

    Object.entries(fields).forEach(([id, value]) => {
        const el = document.getElementById(id);
        if (el) el.value = value || '';
    });

    populateProvinceCombobox(owner.province);
    populateWardCombobox(owner.province, owner.ward);

    const imgContainer = document.getElementById('owner-image-container');
    imgContainer.innerHTML = owner.imageUrl
        ? `<img src="${owner.imageUrl}" alt="Owner" class="owner-image">`
        : `<div class="image-placeholder"><i class="bi bi-person"></i><span>HÌNH 3X4</span></div>`;
}

function displayVehicleInfo(vehicle) {
    const fields = {
        'vehicle-id': vehicle.vehicleId,
        'vehicle-plate': vehicle.plateNo,
        'vehicle-inspection': vehicle.inspectionNo,
        'vehicle-group': vehicle.vehicleGroup,
        'vehicle-type': vehicle.vehicleType,
        'vehicle-brand': vehicle.brand,
        'vehicle-model': vehicle.model,
        'vehicle-engine': vehicle.engineNo,
        'vehicle-chassis': vehicle.chassis,
        'vehicle-year': vehicle.manufactureYear,
        'vehicle-country': vehicle.manufactureCountry,
        'vehicle-energy': vehicle.energyType,
        'vehicle-usage': vehicle.usagePermission,
        'vehicle-lifetime': vehicle.lifetimeLimitYear,
        'vehicle-created': formatDate(vehicle.createdAt),
        'vehicle-updated': formatDate(vehicle.updatedAt)
    };

    Object.entries(fields).forEach(([id, value]) => {
        const el = document.getElementById(id);
        if (el) el.value = value || '';
    });

    document.getElementById('vehicle-clean').checked = vehicle.isCleanEnergy || false;
    document.getElementById('vehicle-modification').checked = vehicle.hasModification || false;
    document.getElementById('vehicle-commercial').checked = vehicle.hasCommercialModification || false;
}

function displaySpecificationInfo(spec) {
    const card = document.querySelector('.vehicle-info-card:last-child');

    if (!spec) {
        if (card) card.style.display = 'none';
        return;
    }

    if (card) card.style.display = 'block';

    // Map all spec fields (simplified for brevity - include all fields as needed)
    const fields = {
        'spec-id': spec.specificationId,
        'spec-wheel-formula': spec.wheelFormula,
        'spec-wheel-tread': spec.wheelTread,
        'spec-wheelbase': spec.wheelbase,
        'spec-length': spec.overallLength,
        'spec-width': spec.overallWidth,
        'spec-height': spec.overallHeight,
        // ... (add remaining fields)
    };

    Object.entries(fields).forEach(([id, value]) => {
        const el = document.getElementById(id);
        if (el) el.value = value || '';
    });

    // Checkboxes
    ['tachograph', 'camera', 'no-stamp'].forEach(name => {
        const checkbox = document.getElementById(`spec-${name}`);
        if (checkbox) checkbox.checked = spec[`has${name.charAt(0).toUpperCase() + name.slice(1)}`] || false;
    });
}

// ==================== TOGGLE OWNER TYPE ====================
function toggleOwnerType() {
    const type = document.getElementById('owner-type').value;
    document.getElementById('person-info').style.display = type === 'PERSON' ? 'flex' : 'none';
    document.getElementById('company-info').style.display = type === 'COMPANY' ? 'flex' : 'none';
}

// ==================== SAVE & CANCEL ====================
async function saveChanges() {
    try {
        const updatedData = collectFormData();
        const saveBtn = document.querySelector('.btn-save');
        const originalText = saveBtn.innerHTML;

        saveBtn.innerHTML = '<i class="bi bi-hourglass-split"></i> Đang lưu...';
        saveBtn.disabled = true;

        const response = await fetch('/api/receive-profile/update', {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(updatedData)
        });

        const result = await response.json();

        if (result.success) {
            currentData = updatedData;
            originalData = JSON.parse(JSON.stringify(updatedData));
            showNotification('Lưu thành công!', 'success');
        } else {
            showNotification(result.message || 'Có lỗi xảy ra khi lưu', 'error');
        }

        saveBtn.innerHTML = originalText;
        saveBtn.disabled = false;
    } catch (error) {
        console.error('❌ Save error:', error);
        showNotification('Có lỗi xảy ra khi lưu', 'error');
    }
}

function cancelChanges() {
    if (!originalData) {
        showNotification('Không có dữ liệu để khôi phục', 'warning');
        return;
    }

    if (confirm('Bạn có chắc muốn hủy tất cả thay đổi?')) {
        displayData(originalData);
        currentData = JSON.parse(JSON.stringify(originalData));
        showNotification('Đã khôi phục dữ liệu gốc', 'info');
    }
}

// ==================== COLLECT FORM DATA ====================
function collectFormData() {
    const ownerType = document.getElementById('owner-type').value;

    return {
        owner: {
            ownerId: document.getElementById('owner-id').value,
            ownerType: ownerType,
            fullName: document.getElementById('owner-fullname').value.trim(),
            companyName: ownerType === 'COMPANY' ? document.getElementById('owner-company').value.trim() : null,
            taxCode: ownerType === 'COMPANY' ? document.getElementById('owner-taxcode').value.trim() : null,
            cccd: ownerType === 'PERSON' ? document.getElementById('owner-cccd').value.trim() : null,
            phone: document.getElementById('owner-phone').value.trim(),
            email: document.getElementById('owner-email').value.trim(),
            address: document.getElementById('owner-address').value.trim(),
            ward: document.getElementById('owner-ward').value,
            province: document.getElementById('owner-province').value
        },
        vehicle: {
            vehicleId: parseInt(document.getElementById('vehicle-id').value),
            plateNo: document.getElementById('vehicle-plate').value.trim().toUpperCase(),
            // ... (include all vehicle fields)
        },
        specification: collectSpecificationData()
    };
}

function collectSpecificationData() {
    const specId = document.getElementById('spec-id').value;
    return specId ? { /* all spec fields */ } : null;
}

// ==================== RECENT SEARCHES ====================
function saveToRecentSearches(cccd, plateNo, searchType) {
    recentSearches = recentSearches.filter(item =>
        !(item.cccd === cccd && item.plateNo === plateNo)
    );
    recentSearches.unshift({ cccd, plateNo, searchType, timestamp: new Date().toISOString() });
    recentSearches = recentSearches.slice(0, 5);
    localStorage.setItem('recentSearches', JSON.stringify(recentSearches));
    loadRecentSearches();
}

function loadRecentSearches() {
    const container = document.getElementById('recent-list');
    container.innerHTML = recentSearches.length === 0
        ? '<p style="color:#999;font-size:13px;text-align:center;padding:10px;">Chưa có lịch sử tìm kiếm</p>'
        : recentSearches.map(item => `
            <div class="recent-item" onclick='applyRecentSearch(${JSON.stringify(item).replace(/'/g, "&#39;")})'>
                <i class="bi bi-clock"></i>
                <div style="flex:1;">
                    <div style="font-weight:500;color:#333;">
                        ${item.searchType === 'cccd' ? `CCCD: ${item.cccd}` : `Biển số: ${item.plateNo}`}
                    </div>
                    <div style="font-size:11px;color:#999;margin-top:2px;">${formatRelativeTime(item.timestamp)}</div>
                </div>
            </div>
        `).join('');
}

function applyRecentSearch(item) {
    document.getElementById('search-cccd').value = item.cccd || '';
    document.getElementById('search-plate').value = item.plateNo || '';
    searchProfile();
}

// ==================== UTILITY FUNCTIONS ====================
function formatDate(dateString) {
    if (!dateString) return '';
    const date = new Date(dateString);
    return `${String(date.getHours()).padStart(2, '0')}:${String(date.getMinutes()).padStart(2, '0')} ${String(date.getDate()).padStart(2, '0')}/${String(date.getMonth() + 1).padStart(2, '0')}/${date.getFullYear()}`;
}

function formatRelativeTime(timestamp) {
    const diffMs = new Date() - new Date(timestamp);
    const diffMins = Math.floor(diffMs / 60000);

    if (diffMins < 1) return 'Vừa xong';
    if (diffMins < 60) return `${diffMins} phút trước`;

    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours} giờ trước`;

    const diffDays = Math.floor(diffHours / 24);
    if (diffDays < 7) return `${diffDays} ngày trước`;

    return new Date(timestamp).toLocaleDateString('vi-VN');
}

function showNotification(message, type = 'info') {
    const container = document.getElementById('notification-container');
    if (!container) return;

    const icons = {
        success: 'bi-check-circle-fill',
        error: 'bi-x-circle-fill',
        warning: 'bi-exclamation-triangle-fill',
        info: 'bi-info-circle-fill'
    };

    const notification = document.createElement('div');
    notification.className = `notification ${type}`;
    notification.innerHTML = `<i class="bi ${icons[type]}"></i><span>${message}</span>`;
    container.appendChild(notification);

    setTimeout(() => {
        notification.style.opacity = '0';
        notification.style.transform = 'translateX(100%)';
        setTimeout(() => notification.remove(), 300);
    }, 3000);
}

function previewOwnerImage(event) {
    const file = event.target.files[0];
    if (!file) return;

    if (!file.type.startsWith('image/')) {
        showNotification('Vui lòng chọn file ảnh hợp lệ', 'error');
        return;
    }

    if (file.size > 5 * 1024 * 1024) {
        showNotification('Kích thước ảnh không được vượt quá 5MB', 'error');
        return;
    }

    const reader = new FileReader();
    reader.onload = (e) => {
        document.getElementById('owner-image-container').innerHTML =
            `<img src="${e.target.result}" alt="Owner Image" class="owner-image">`;
    };
    reader.readAsDataURL(file);
}