// ==================== GLOBAL STATE ====================
let currentData = null;
let originalData = null;
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
        console.log('🔍 [loadProvincesData] Bắt đầu fetch...');

        const response = await fetch('/api/receive-profile/provinces');
        console.log('🔍 [loadProvincesData] Response status:', response.status);

        const result = await response.json();
        console.log('🔍 [loadProvincesData] Result:', result);

        if (result.success && result.data) {
            console.log('✅ Đã tải:', result.data.length, 'tỉnh/thành phố');
        } else {
            console.error('❌ API không success:', result);
        }
    } catch (error) {
        console.error('❌ Lỗi tải provinces:', error);
        showNotification('Không thể tải dữ liệu tỉnh/thành phố', 'error');
    }
}

// ==================== POPULATE COMBOBOX ====================
async function populateProvinceCombobox(selectedProvince = '') {
    const select = document.getElementById('owner-province');

    if (!select) {
        console.error('❌ Không tìm thấy element #owner-province');
        return;
    }

    try {
        const response = await fetch('/api/receive-profile/provinces');
        const result = await response.json();

        if (!result.success || !result.data) {
            console.error('❌ Không có dữ liệu tỉnh/thành phố');
            return;
        }

        select.innerHTML = '<option value="">-- Chọn Tỉnh/Thành phố --</option>';

        result.data.forEach(provinceName => {
            const option = document.createElement('option');
            option.value = provinceName;
            option.textContent = provinceName;
            option.selected = (provinceName === selectedProvince);
            select.appendChild(option);
        });

        console.log(`✅ Đã thêm ${result.data.length} options vào combobox`);
    } catch (error) {
        console.error('❌ Lỗi populate provinces:', error);
    }
}

async function populateWardCombobox(provinceName, selectedWard = '') {
    const select = document.getElementById('owner-ward');
    select.innerHTML = '<option value="">-- Chọn Phường/Xã --</option>';

    if (!provinceName) {
        select.disabled = true;
        return;
    }

    select.disabled = false;

    try {
        const response = await fetch(`/api/receive-profile/wards?province=${encodeURIComponent(provinceName)}`);
        const result = await response.json();

        if (!result.success || !result.data) {
            console.warn('❌ Không có dữ liệu phường/xã cho:', provinceName);
            return;
        }

        result.data.forEach(ward => {
            const option = document.createElement('option');
            option.value = ward.tenphuongxa;
            // ✅ Không hiển thị quận/huyện vì không có trong data
            option.textContent = ward.tenphuongxa;
            option.selected = (ward.tenphuongxa === selectedWard);
            select.appendChild(option);
        });

        console.log(`✅ Đã populate ${result.data.length} ward cho:`, provinceName);
    } catch (error) {
        console.error('❌ Lỗi populate wards:', error);
    }
}

// ==================== EVENT LISTENERS ====================
function setupEventListeners() {
    ['search-cccd', 'search-plate'].forEach(id => {
        const element = document.getElementById(id);
        if (element) {
            element.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') searchProfile();
            });
        }
    });

    const provinceSelect = document.getElementById('owner-province');
    if (provinceSelect) {
        provinceSelect.addEventListener('change', function () {
            populateWardCombobox(this.value);
        });
    }
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
    if (imgContainer) {
        imgContainer.innerHTML = owner.imageUrl
            ? `<img src="${owner.imageUrl}" alt="Owner" class="owner-image">`
            : `<div class="image-placeholder"><i class="bi bi-person"></i><span>HÌNH 3X4</span></div>`;
    }
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

    const cleanCheckbox = document.getElementById('vehicle-clean');
    if (cleanCheckbox) cleanCheckbox.checked = vehicle.isCleanEnergy || false;

    const modCheckbox = document.getElementById('vehicle-modification');
    if (modCheckbox) modCheckbox.checked = vehicle.hasModification || false;

    const commercialCheckbox = document.getElementById('vehicle-commercial');
    if (commercialCheckbox) commercialCheckbox.checked = vehicle.hasCommercialModification || false;
}

function displaySpecificationInfo(spec) {
    const card = document.querySelector('.vehicle-info-card:last-child');

    if (!spec) {
        if (card) card.style.display = 'none';
        return;
    }

    if (card) card.style.display = 'block';

    const fields = {
        'spec-id': spec.specificationId,
        'spec-wheel-formula': spec.wheelFormula,
        'spec-wheel-tread': spec.wheelTread,
        'spec-wheelbase': spec.wheelbase,
        'spec-length': spec.overallLength,
        'spec-width': spec.overallWidth,
        'spec-height': spec.overallHeight,
        'spec-cargo-length': spec.cargoInsideLength,
        'spec-cargo-width': spec.cargoInsideWidth,
        'spec-cargo-height': spec.cargoInsideHeight,
        'spec-kerb-weight': spec.kerbWeight,
        'spec-cargo-weight': spec.authorizedCargoWeight,
        'spec-towed-weight': spec.authorizedTowedWeight,
        'spec-total-weight': spec.authorizedTotalWeight,
        'spec-seating': spec.seatingCapacity,
        'spec-standing': spec.standingCapacity,
        'spec-lying': spec.lyingCapacity,
        'spec-engine-type': spec.engineType,
        'spec-engine-position': spec.enginePosition,
        'spec-engine-model': spec.engineModel,
        'spec-displacement': spec.engineDisplacement,
        'spec-max-power': spec.maxPower,
        'spec-max-rpm': spec.maxPowerRPM,
        'spec-fuel-type': spec.fuelType,
        'spec-motor-type': spec.motorType,
        'spec-motor-count': spec.numberOfMotors,
        'spec-motor-model': spec.motorModel,
        'spec-motor-power': spec.totalMotorPower,
        'spec-motor-voltage': spec.motorVoltage,
        'spec-battery-type': spec.batteryType,
        'spec-battery-voltage': spec.batteryVoltage,
        'spec-battery-capacity': spec.batteryCapacity,
        'spec-tire-count': spec.tireCount,
        'spec-tire-size': spec.tireSize,
        'spec-tire-axle': spec.tireAxleInfo,
        'spec-image-position': spec.imagePosition,
        'spec-notes': spec.notes
    };

    Object.entries(fields).forEach(([id, value]) => {
        const el = document.getElementById(id);
        if (el) el.value = value || '';
    });

    const tachographCheckbox = document.getElementById('spec-tachograph');
    if (tachographCheckbox) tachographCheckbox.checked = spec.hasTachograph || false;

    const cameraCheckbox = document.getElementById('spec-camera');
    if (cameraCheckbox) cameraCheckbox.checked = spec.hasDriverCamera || false;

    const noStampCheckbox = document.getElementById('spec-no-stamp');
    if (noStampCheckbox) noStampCheckbox.checked = spec.notIssuedStamp || false;
}

// ==================== TOGGLE OWNER TYPE ====================
function toggleOwnerType() {
    const type = document.getElementById('owner-type').value;
    const personInfo = document.getElementById('person-info');
    const companyInfo = document.getElementById('company-info');

    if (personInfo) personInfo.style.display = type === 'PERSON' ? 'flex' : 'none';
    if (companyInfo) companyInfo.style.display = type === 'COMPANY' ? 'flex' : 'none';
}

// ==================== SAVE & CANCEL ====================
async function saveChanges() {
    try {
        const updatedData = collectFormData();
        const saveBtn = document.querySelector('.btn-save');
        if (!saveBtn) return;

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
            inspectionNo: document.getElementById('vehicle-inspection').value.trim(),
            vehicleGroup: document.getElementById('vehicle-group').value.trim(),
            vehicleType: document.getElementById('vehicle-type').value.trim(),
            energyType: document.getElementById('vehicle-energy').value.trim(),
            isCleanEnergy: document.getElementById('vehicle-clean').checked,
            usagePermission: document.getElementById('vehicle-usage').value.trim(),
            brand: document.getElementById('vehicle-brand').value.trim(),
            model: document.getElementById('vehicle-model').value.trim(),
            engineNo: document.getElementById('vehicle-engine').value.trim(),
            chassis: document.getElementById('vehicle-chassis').value.trim(),
            manufactureYear: parseInt(document.getElementById('vehicle-year').value) || null,
            manufactureCountry: document.getElementById('vehicle-country').value.trim(),
            lifetimeLimitYear: parseInt(document.getElementById('vehicle-lifetime').value) || null,
            hasCommercialModification: document.getElementById('vehicle-commercial').checked,
            hasModification: document.getElementById('vehicle-modification').checked
        },
        specification: collectSpecificationData()
    };
}

function collectSpecificationData() {
    const specId = document.getElementById('spec-id').value;
    if (!specId) return null;

    return {
        specificationId: parseInt(specId),
        plateNo: document.getElementById('vehicle-plate').value.trim().toUpperCase(),
        wheelFormula: document.getElementById('spec-wheel-formula').value.trim(),
        wheelTread: parseInt(document.getElementById('spec-wheel-tread').value) || null,
        overallLength: parseInt(document.getElementById('spec-length').value) || null,
        overallWidth: parseInt(document.getElementById('spec-width').value) || null,
        overallHeight: parseInt(document.getElementById('spec-height').value) || null,
        cargoInsideLength: parseInt(document.getElementById('spec-cargo-length').value) || null,
        cargoInsideWidth: parseInt(document.getElementById('spec-cargo-width').value) || null,
        cargoInsideHeight: parseInt(document.getElementById('spec-cargo-height').value) || null,
        wheelbase: parseInt(document.getElementById('spec-wheelbase').value) || null,
        kerbWeight: parseFloat(document.getElementById('spec-kerb-weight').value) || null,
        authorizedCargoWeight: parseFloat(document.getElementById('spec-cargo-weight').value) || null,
        authorizedTowedWeight: parseFloat(document.getElementById('spec-towed-weight').value) || null,
        authorizedTotalWeight: parseFloat(document.getElementById('spec-total-weight').value) || null,
        seatingCapacity: parseInt(document.getElementById('spec-seating').value) || null,
        standingCapacity: parseInt(document.getElementById('spec-standing').value) || null,
        lyingCapacity: parseInt(document.getElementById('spec-lying').value) || null,
        engineType: document.getElementById('spec-engine-type').value.trim(),
        enginePosition: document.getElementById('spec-engine-position').value.trim(),
        engineModel: document.getElementById('spec-engine-model').value.trim(),
        engineDisplacement: parseInt(document.getElementById('spec-displacement').value) || null,
        maxPower: parseFloat(document.getElementById('spec-max-power').value) || null,
        maxPowerRPM: parseInt(document.getElementById('spec-max-rpm').value) || null,
        fuelType: document.getElementById('spec-fuel-type').value.trim(),
        motorType: document.getElementById('spec-motor-type').value.trim(),
        numberOfMotors: parseInt(document.getElementById('spec-motor-count').value) || null,
        motorModel: document.getElementById('spec-motor-model').value.trim(),
        totalMotorPower: parseFloat(document.getElementById('spec-motor-power').value) || null,
        motorVoltage: parseFloat(document.getElementById('spec-motor-voltage').value) || null,
        batteryType: document.getElementById('spec-battery-type').value.trim(),
        batteryVoltage: parseFloat(document.getElementById('spec-battery-voltage').value) || null,
        batteryCapacity: parseFloat(document.getElementById('spec-battery-capacity').value) || null,
        tireCount: parseInt(document.getElementById('spec-tire-count').value) || null,
        tireSize: document.getElementById('spec-tire-size').value.trim(),
        tireAxleInfo: document.getElementById('spec-tire-axle').value.trim(),
        imagePosition: document.getElementById('spec-image-position').value.trim(),
        hasTachograph: document.getElementById('spec-tachograph').checked,
        hasDriverCamera: document.getElementById('spec-camera').checked,
        notIssuedStamp: document.getElementById('spec-no-stamp').checked,
        notes: document.getElementById('spec-notes').value.trim()
    };
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
    if (!container) return;

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
        const container = document.getElementById('owner-image-container');
        if (container) {
            container.innerHTML = `<img src="${e.target.result}" alt="Owner Image" class="owner-image">`;
        }
    };
    reader.readAsDataURL(file);
}