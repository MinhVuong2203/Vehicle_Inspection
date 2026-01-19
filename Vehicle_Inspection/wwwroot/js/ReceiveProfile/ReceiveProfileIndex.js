// ========================================
// FILE: ReceiveProfileIndex.js
// ========================================

// ========== GLOBAL VARIABLES ==========
var currentOwner = null;
var currentVehicle = null;
var currentSpecification = null;

// ========== SEARCH FUNCTION ==========
async function searchProfile() {
    const cccd = document.getElementById('search-cccd')?.value?.trim() || '';
    const plateNo = document.getElementById('search-plate')?.value?.trim() || '';

    console.log('🔍 Search with:', { cccd, plateNo });

    if (!cccd && !plateNo) {
        alert('Vui lòng nhập CCCD hoặc Biển số xe');
        return;
    }

    const noDataState = document.getElementById('no-data-state');
    const loadingState = document.getElementById('loading-state');
    const dataDisplay = document.getElementById('data-display');

    if (noDataState) noDataState.style.display = 'none';
    if (loadingState) loadingState.style.display = 'flex';
    if (dataDisplay) dataDisplay.style.display = 'none';

    try {
        const url = `/ReceiveProfile/Search?cccd=${encodeURIComponent(cccd)}&plateNo=${encodeURIComponent(plateNo)}`;
        console.log('📡 Request:', url);

        const response = await fetch(url, {
            method: 'GET',
            headers: { 'Accept': 'application/json' }
        });

        console.log('📊 Status:', response.status);

        if (!response.ok) {
            throw new Error('HTTP ' + response.status);
        }

        const data = await response.json();
        console.log('📊 Data:', data);

        if (loadingState) loadingState.style.display = 'none';

        if (data.success) {
            currentOwner = data.data.owner;
            currentVehicle = data.data.vehicle;
            currentSpecification = data.data.specification;

            if (dataDisplay) dataDisplay.style.display = 'block';
            populateForm(data.data);
            showNotification('success', data.message || 'Tìm kiếm thành công');
        } else {
            if (noDataState) noDataState.style.display = 'flex';
            showNotification('error', data.message || 'Không tìm thấy');
        }
    } catch (error) {
        console.error('❌ Error:', error);
        if (loadingState) loadingState.style.display = 'none';
        if (noDataState) noDataState.style.display = 'flex';
        showNotification('error', 'Lỗi: ' + error.message);
    }
}

// ========== POPULATE FORM ==========
function populateForm(data) {
    console.log('🔄 Populate:', data);

    if (data.owner) {
        setValue('owner-id', data.owner.ownerId);
        setValue('owner-fullname', data.owner.fullName);
        setValue('owner-type', data.owner.ownerType === 'PERSON' ? 'Cá nhân' : 'Công ty');
        setValue('owner-cccd', data.owner.cccd);
        setValue('owner-phone', data.owner.phone);
        setValue('owner-phone2', data.owner.phone);
        setValue('owner-email', data.owner.email);
        setValue('owner-address', data.owner.address);
        setValue('owner-company', data.owner.companyName);
        setValue('owner-taxcode', data.owner.taxCode);
        setValue('owner-province', data.owner.province);
        setValue('owner-ward', data.owner.ward);
        setValue('owner-created', data.owner.createdAt ? new Date(data.owner.createdAt).toLocaleString('vi-VN') : '');

        toggleOwnerType(data.owner.ownerType);

        if (data.owner.imageUrl) {
            const img = document.getElementById('owner-image-container');
            if (img) img.innerHTML = '<img src="' + data.owner.imageUrl + '" class="owner-image">';
        }
    }

    if (data.vehicle) {
        setValue('vehicle-id', data.vehicle.vehicleId);
        setValue('vehicle-plate', data.vehicle.plateNo);
        setValue('vehicle-inspection', data.vehicle.inspectionNo);
        setValue('vehicle-group', data.vehicle.vehicleGroup);
        setValue('vehicle-type', data.vehicle.vehicleType);
        setValue('vehicle-energy', data.vehicle.energyType);
        setChecked('vehicle-clean', data.vehicle.isCleanEnergy);
        setValue('vehicle-usage', data.vehicle.usagePermission);
        setValue('vehicle-brand', data.vehicle.brand);
        setValue('vehicle-model', data.vehicle.model);
        setValue('vehicle-engine', data.vehicle.engineNo);
        setValue('vehicle-chassis', data.vehicle.chassis);
        setValue('vehicle-year', data.vehicle.manufactureYear);
        setValue('vehicle-country', data.vehicle.manufactureCountry);
        setValue('vehicle-lifetime', data.vehicle.lifetimeLimitYear);
        setChecked('vehicle-commercial', data.vehicle.hasCommercialModification);
        setChecked('vehicle-modification', data.vehicle.hasModification);
        setValue('vehicle-created', data.vehicle.createdAt ? new Date(data.vehicle.createdAt).toLocaleString('vi-VN') : '');
        setValue('vehicle-updated', data.vehicle.updatedAt ? new Date(data.vehicle.updatedAt).toLocaleString('vi-VN') : '');
    }

    if (data.specification) {
        setValue('spec-id', data.specification.specificationId);
        setValue('spec-wheel-formula', data.specification.wheelFormula);
        setValue('spec-wheel-tread', data.specification.wheelTread);
        setValue('spec-wheelbase', data.specification.wheelbase);
        setValue('spec-length', data.specification.overallLength);
        setValue('spec-width', data.specification.overallWidth);
        setValue('spec-height', data.specification.overallHeight);
        setValue('spec-cargo-length', data.specification.cargoInsideLength);
        setValue('spec-cargo-width', data.specification.cargoInsideWidth);
        setValue('spec-cargo-height', data.specification.cargoInsideHeight);
        setValue('spec-kerb-weight', data.specification.kerbWeight);
        setValue('spec-cargo-weight', data.specification.authorizedCargoWeight);
        setValue('spec-towed-weight', data.specification.authorizedTowedWeight);
        setValue('spec-total-weight', data.specification.authorizedTotalWeight);
        setValue('spec-seating', data.specification.seatingCapacity);
        setValue('spec-standing', data.specification.standingCapacity);
        setValue('spec-lying', data.specification.lyingCapacity);
        setValue('spec-engine-type', data.specification.engineType);
        setValue('spec-engine-position', data.specification.enginePosition);
        setValue('spec-engine-model', data.specification.engineModel);
        setValue('spec-displacement', data.specification.engineDisplacement);
        setValue('spec-max-power', data.specification.maxPower);
        setValue('spec-max-rpm', data.specification.maxPowerRPM);
        setValue('spec-fuel-type', data.specification.fuelType);
        setValue('spec-motor-type', data.specification.motorType);
        setValue('spec-motor-count', data.specification.numberOfMotors);
        setValue('spec-motor-model', data.specification.motorModel);
        setValue('spec-motor-power', data.specification.totalMotorPower);
        setValue('spec-motor-voltage', data.specification.motorVoltage);
        setValue('spec-battery-type', data.specification.batteryType);
        setValue('spec-battery-voltage', data.specification.batteryVoltage);
        setValue('spec-battery-capacity', data.specification.batteryCapacity);
        setValue('spec-tire-count', data.specification.tireCount);
        setValue('spec-tire-size', data.specification.tireSize);
        setValue('spec-tire-axle', data.specification.tireAxleInfo);
        setValue('spec-image-position', data.specification.imagePosition);
        setChecked('spec-tachograph', data.specification.hasTachograph);
        setChecked('spec-camera', data.specification.hasDriverCamera);
        setChecked('spec-no-stamp', data.specification.notIssuedStamp);
        setValue('spec-notes', data.specification.notes);
    }

    console.log('✅ Done');
}

// ========== HELPERS ==========
function setValue(id, val) {
    const el = document.getElementById(id);
    if (el) el.value = val || '';
}

function setChecked(id, val) {
    const el = document.getElementById(id);
    if (el) el.checked = val || false;
}

function toggleOwnerType(type) {
    const person = document.getElementById('person-info');
    const company = document.getElementById('company-info');
    if (type === 'PERSON') {
        if (person) person.style.display = 'flex';
        if (company) company.style.display = 'none';
    } else {
        if (person) person.style.display = 'none';
        if (company) company.style.display = 'flex';
    }
}

// ========== EDIT ==========
function editProfile() {
    if (!currentOwner || !currentVehicle) {
        alert('Vui lòng tìm kiếm trước');
        return;
    }
    const cccd = currentOwner.cccd || '';
    const plate = currentVehicle.plateNo || '';
    window.location.href = '/ReceiveProfile/Edit?cccd=' + encodeURIComponent(cccd) + '&plateNo=' + encodeURIComponent(plate);
}

// ========== CREATE ==========
function createNewProfile() {
    window.location.href = '/ReceiveProfile/Create';
}

// ========== CLEAR ==========
function clearSearch() {
    setValue('search-cccd', '');
    setValue('search-plate', '');

    const noData = document.getElementById('no-data-state');
    const display = document.getElementById('data-display');
    if (noData) noData.style.display = 'flex';
    if (display) display.style.display = 'none';

    currentOwner = null;
    currentVehicle = null;
    currentSpecification = null;
}

// ========== NOTIFICATION ==========
function showNotification(type, msg) {
    const container = document.getElementById('notification-container');
    if (!container) {
        console.log(type + ': ' + msg);
        return;
    }

    const icons = {
        'success': 'check-circle',
        'error': 'x-circle',
        'warning': 'exclamation-triangle',
        'info': 'info-circle'
    };

    const div = document.createElement('div');
    div.className = 'notification notification-' + type;
    div.innerHTML = '<i class="bi bi-' + icons[type] + '"></i><span>' + msg + '</span>';

    container.appendChild(div);

    setTimeout(function () {
        div.style.opacity = '0';
        setTimeout(function () { div.remove(); }, 300);
    }, 5000);
}