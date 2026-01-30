// ========== GLOBAL VARIABLES ==========
let currentOwner = null;
let currentVehicle = null;
let currentSpecification = null;
let currentPageMode = 'view'; // 'view', 'edit', 'create'
let currentSearchType = null; // ✅ 'cccd' hoặc 'plateNo'

// ========== KHỞI TẠO TRANG ==========
async function initializePage(mode) {
    currentPageMode = mode;
    console.log(`🚀 Initializing page in ${mode} mode`);

    loadProvinces();

    try {
        console.log('🔥 CALLING loadVehicleTypes...');
        await loadVehicleTypes();
    } catch (err) {
        console.error('💥 Error calling loadVehicleTypes:', err);
    }

    setupProvinceListener();

    const ownerTypeSelect = document.getElementById('owner-type');
    if (ownerTypeSelect) {
        ownerTypeSelect.addEventListener('change', toggleOwnerType);
    }

    setupPhoneSync();

    if (mode === 'edit') {
        await loadDataForEdit();
    }
}

// ========== SETUP PHONE SYNC ==========
function setupPhoneSync() {
    const phone1 = document.getElementById('owner-phone');
    const phone2 = document.getElementById('owner-phone2');

    if (phone1 && phone2) {
        phone1.addEventListener('input', function () {
            phone2.value = this.value;
        });
        phone2.addEventListener('input', function () {
            phone1.value = this.value;
        });
    }
}

// ========== LOAD DATA CHO TRANG EDIT ==========
async function loadDataForEdit() {
    const urlParams = new URLSearchParams(window.location.search);
    const cccd = urlParams.get('cccd');
    const plateNo = urlParams.get('plateNo');

    if (!cccd && !plateNo) {
        showNotification('error', 'Thiếu thông tin để tải dữ liệu');
        return;
    }

    const loadingState = document.getElementById('loading-state');
    const dataDisplay = document.getElementById('data-display');

    if (loadingState) loadingState.style.display = 'flex';
    if (dataDisplay) dataDisplay.style.display = 'none';

    try {
        const url = `/api/receive-profile/search?cccd=${encodeURIComponent(cccd || '')}&plateNo=${encodeURIComponent(plateNo || '')}`;
        console.log('📡 Loading data from:', url);

        const response = await fetch(url);
        const data = await response.json();

        if (loadingState) loadingState.style.display = 'none';

        if (data.success) {
            currentOwner = data.data.owner;
            currentVehicle = data.data.vehicle;
            currentSpecification = data.data.specification;
            currentSearchType = data.searchType;

            if (dataDisplay) dataDisplay.style.display = 'block';
            await populateForm(data.data);
            console.log('✅ Data loaded successfully');
        } else {
            showNotification('error', data.message);
        }
    } catch (error) {
        console.error('❌ Load data error:', error);
        if (loadingState) loadingState.style.display = 'none';
        showNotification('error', 'Không thể tải dữ liệu');
    }
}

// ========== SEARCH FUNCTION (CHO TRANG INDEX) ==========
async function searchProfile() {
    const cccd = document.getElementById('search-cccd')?.value?.trim() || '';
    const plateNo = document.getElementById('search-plate')?.value?.trim() || '';

    console.log('🔍 Search with:', { cccd, plateNo });

    if (!cccd && !plateNo) {
        showNotification('warning', 'Vui lòng nhập CCCD hoặc Biển số xe');
        return;
    }

    // ✅ Cho phép nhập cả 2:
    // - Chỉ CCCD: Hiển thị thông tin người đi đăng kiểm
    // - Chỉ Biển số: Hiển thị thông tin xe + thông số kỹ thuật
    // - Cả hai: Hiển thị cả 3 (người đi đăng kiểm có thể khác chủ xe)

    const noDataState = document.getElementById('no-data-state');
    const loadingState = document.getElementById('loading-state');
    const dataDisplay = document.getElementById('data-display');

    if (noDataState) noDataState.style.display = 'none';
    if (loadingState) loadingState.style.display = 'flex';
    if (dataDisplay) dataDisplay.style.display = 'none';

    try {
        const url = `/api/receive-profile/search?cccd=${encodeURIComponent(cccd)}&plateNo=${encodeURIComponent(plateNo)}`;
        console.log('📡 Request URL:', url);

        const response = await fetch(url);
        const data = await response.json();

        console.log('📊 Response:', data);

        if (loadingState) loadingState.style.display = 'none';

        if (data.success) {
            currentOwner = data.data.owner;
            currentVehicle = data.data.vehicle;
            currentSpecification = data.data.specification;
            currentSearchType = data.searchType; // ✅ 'cccd' hoặc 'plateNo'

            if (dataDisplay) dataDisplay.style.display = 'block';

            // ✅ Hiển thị theo loại tìm kiếm
            displayDataBySearchType(data.searchType);

            await populateForm(data.data);
            showNotification('success', data.message);
        } else {
            if (noDataState) noDataState.style.display = 'flex';
            showNotification('error', data.message);
        }
    } catch (error) {
        console.error('❌ Search error:', error);
        if (loadingState) loadingState.style.display = 'none';
        if (noDataState) noDataState.style.display = 'flex';
        showNotification('error', 'Có lỗi xảy ra khi tìm kiếm');
    }
}

// ✅ ========== HIỂN THỊ DATA THEO LOẠI TÌM KIẾM ==========
function displayDataBySearchType(searchType) {
    console.log('📋 Displaying data for search type:', searchType);

    const ownerCard = document.querySelector('.owner-info-card');
    const vehicleCards = document.querySelectorAll('.vehicle-info-card');
    const actionButtons = document.querySelector('.action-buttons');

    if (searchType === 'cccd') {
        // ✅ CHỈ CCCD: Chỉ hiển thị thông tin người đi đăng kiểm
        console.log('👤 Showing OWNER info only');
        if (ownerCard) ownerCard.style.display = 'block';
        vehicleCards.forEach(card => card.style.display = 'none');

    } else if (searchType === 'plateNo') {
        // ✅ CHỈ BIỂN SỐ: Hiển thị thông tin xe + thông số kỹ thuật
        console.log('🚗 Showing VEHICLE and SPECIFICATION only');
        if (ownerCard) ownerCard.style.display = 'none';
        vehicleCards.forEach(card => card.style.display = 'block');

    } else if (searchType === 'combined') {
        // ✅ CẢ HAI: Hiển thị đầy đủ 3 sections
        // Người đi đăng kiểm có thể khác chủ xe
        console.log('📋 Showing ALL sections (Owner + Vehicle + Specification)');
        if (ownerCard) ownerCard.style.display = 'block';
        vehicleCards.forEach(card => card.style.display = 'block');
    }

    // Luôn hiển thị action buttons
    if (actionButtons) actionButtons.style.display = 'flex';
}

// ========== POPULATE FORM ==========
async function populateForm(data) {
    console.log('🔄 Populating form with data:', data);
    console.log('🔍 Current search type:', currentSearchType);

    // ✅ POPULATE DỮ LIỆU THEO LOẠI TÌM KIẾM

    // Populate Owner data (nếu tìm theo CCCD hoặc combined)
    if ((currentSearchType === 'cccd' || currentSearchType === 'combined') && data.owner) {
        console.log('👤 Populating OWNER data...');
        setFieldValue('owner-id', data.owner.ownerId);
        setFieldValue('owner-fullname', data.owner.fullName);
        setFieldValue('owner-type', data.owner.ownerType === 'PERSON' ? 'Cá nhân' : 'Công ty');
        setFieldValue('owner-cccd', data.owner.cccd);

        const phoneValue = data.owner.phone || '';
        setFieldValue('owner-phone', phoneValue);
        setFieldValue('owner-phone2', phoneValue);

        setFieldValue('owner-email', data.owner.email);
        setFieldValue('owner-address', data.owner.address);
        setFieldValue('owner-company', data.owner.companyName);
        setFieldValue('owner-taxcode', data.owner.taxCode);
        setFieldValue('owner-province', data.owner.province);
        setFieldValue('owner-ward', data.owner.ward);

        const createdAt = data.owner.createdAt ? new Date(data.owner.createdAt).toLocaleString('vi-VN') : '';
        setFieldValue('owner-created', createdAt);

        toggleOwnerType(data.owner.ownerType);

        if (data.owner.imageUrl) {
            displayOwnerImage(data.owner.imageUrl);
        }
        console.log('✅ Owner data populated');
    }

    // Populate Vehicle data (nếu tìm theo plateNo hoặc combined)
    if ((currentSearchType === 'plateNo' || currentSearchType === 'combined') && data.vehicle) {
        console.log('🚗 Populating VEHICLE data...');
        setFieldValue('vehicle-id', data.vehicle.vehicleId);
        setFieldValue('vehicle-plate', data.vehicle.plateNo);
        setFieldValue('vehicle-inspection', data.vehicle.inspectionNo);
        setFieldValue('vehicle-group', data.vehicle.vehicleGroup);
        if (data.vehicle.vehicleType) {
            setTimeout(() => {
                setFieldValue('vehicle-type', data.vehicle.vehicleType);
                console.log(`✅ Vehicle type set to: ${data.vehicle.vehicleType}`);
            }, 300);
        }
        setFieldValue('vehicle-energy', data.vehicle.energyType);
        setCheckboxValue('vehicle-clean', data.vehicle.isCleanEnergy);
        setFieldValue('vehicle-usage', data.vehicle.usagePermission);
        setFieldValue('vehicle-brand', data.vehicle.brand);
        setFieldValue('vehicle-model', data.vehicle.model);
        setFieldValue('vehicle-engine', data.vehicle.engineNo);
        setFieldValue('vehicle-chassis', data.vehicle.chassis);
        setFieldValue('vehicle-year', data.vehicle.manufactureYear);
        setFieldValue('vehicle-country', data.vehicle.manufactureCountry);
        setFieldValue('vehicle-lifetime', data.vehicle.lifetimeLimitYear);
        setCheckboxValue('vehicle-commercial', data.vehicle.hasCommercialModification);
        setCheckboxValue('vehicle-modification', data.vehicle.hasModification);

        const vehicleCreated = data.vehicle.createdAt ? new Date(data.vehicle.createdAt).toLocaleString('vi-VN') : '';
        const vehicleUpdated = data.vehicle.updatedAt ? new Date(data.vehicle.updatedAt).toLocaleString('vi-VN') : '';
        setFieldValue('vehicle-created', vehicleCreated);
        setFieldValue('vehicle-updated', vehicleUpdated);
        console.log('✅ Vehicle data populated');
    }

    // Populate Specification data (nếu tìm theo plateNo hoặc combined)
    if ((currentSearchType === 'plateNo' || currentSearchType === 'combined') && data.specification) {
        console.log('⚙️ Populating SPECIFICATION data...');
        setFieldValue('spec-id', data.specification.specificationId);
        setFieldValue('spec-wheel-formula', data.specification.wheelFormula);
        setFieldValue('spec-wheel-tread', data.specification.wheelTread);
        setFieldValue('spec-wheelbase', data.specification.wheelbase);
        setFieldValue('spec-length', data.specification.overallLength);
        setFieldValue('spec-width', data.specification.overallWidth);
        setFieldValue('spec-height', data.specification.overallHeight);
        setFieldValue('spec-cargo-length', data.specification.cargoInsideLength);
        setFieldValue('spec-cargo-width', data.specification.cargoInsideWidth);
        setFieldValue('spec-cargo-height', data.specification.cargoInsideHeight);
        setFieldValue('spec-kerb-weight', data.specification.kerbWeight);
        setFieldValue('spec-cargo-weight', data.specification.authorizedCargoWeight);
        setFieldValue('spec-towed-weight', data.specification.authorizedTowedWeight);
        setFieldValue('spec-total-weight', data.specification.authorizedTotalWeight);
        setFieldValue('spec-seating', data.specification.seatingCapacity);
        setFieldValue('spec-standing', data.specification.standingCapacity);
        setFieldValue('spec-lying', data.specification.lyingCapacity);
        setFieldValue('spec-engine-type', data.specification.engineType);
        setFieldValue('spec-engine-position', data.specification.enginePosition);
        setFieldValue('spec-engine-model', data.specification.engineModel);
        setFieldValue('spec-displacement', data.specification.engineDisplacement);
        setFieldValue('spec-max-power', data.specification.maxPower);
        setFieldValue('spec-max-rpm', data.specification.maxPowerRPM);
        setFieldValue('spec-fuel-type', data.specification.fuelType);
        setFieldValue('spec-motor-type', data.specification.motorType);
        setFieldValue('spec-motor-count', data.specification.numberOfMotors);
        setFieldValue('spec-motor-model', data.specification.motorModel);
        setFieldValue('spec-motor-power', data.specification.totalMotorPower);
        setFieldValue('spec-motor-voltage', data.specification.motorVoltage);
        setFieldValue('spec-battery-type', data.specification.batteryType);
        setFieldValue('spec-battery-voltage', data.specification.batteryVoltage);
        setFieldValue('spec-battery-capacity', data.specification.batteryCapacity);
        setFieldValue('spec-tire-count', data.specification.tireCount);
        setFieldValue('spec-tire-size', data.specification.tireSize);
        setFieldValue('spec-tire-axle', data.specification.tireAxleInfo);
        setFieldValue('spec-image-position', data.specification.imagePosition);
        setCheckboxValue('spec-tachograph', data.specification.hasTachograph);
        setCheckboxValue('spec-camera', data.specification.hasDriverCamera);
        setCheckboxValue('spec-no-stamp', data.specification.notIssuedStamp);
        setFieldValue('spec-notes', data.specification.notes);
        console.log('✅ Specification data populated');
    }

    console.log('✅ Form populated successfully');
}

// ========== HELPER FUNCTIONS ==========
function setFieldValue(fieldId, value) {
    const field = document.getElementById(fieldId);
    if (field) {
        field.value = value || '';
    }
}

function setCheckboxValue(fieldId, value) {
    const field = document.getElementById(fieldId);
    if (field) {
        field.checked = value || false;
    }
}

function getFieldValue(fieldId, defaultValue = '') {
    const field = document.getElementById(fieldId);
    return field?.value?.trim() || defaultValue;
}

function getCheckboxValue(fieldId, defaultValue = false) {
    const field = document.getElementById(fieldId);
    return field?.checked ?? defaultValue;
}

function displayOwnerImage(imageUrl) {
    const imgContainer = document.getElementById('owner-image-container');
    if (imgContainer && imageUrl) {
        imgContainer.innerHTML = `<img src="${imageUrl}" alt="Owner Image" class="owner-image">`;
    }
}

function previewOwnerImage(event) {
    if (event.target.files && event.target.files[0]) {
        const reader = new FileReader();
        reader.onload = function (e) {
            displayOwnerImage(e.target.result);
        };
        reader.readAsDataURL(event.target.files[0]);
    }
}

// ========== COLLECT FORM DATA ==========
function collectFormData() {
    const ownerType = getFieldValue('owner-type');
    const phoneValue = getFieldValue('owner-phone2') || getFieldValue('owner-phone');

    const formData = {
        Owner: {
            OwnerId: getFieldValue('owner-id') || '00000000-0000-0000-0000-000000000000',
            OwnerType: ownerType || 'PERSON',
            FullName: getFieldValue('owner-fullname'),
            CompanyName: getFieldValue('owner-company'),
            TaxCode: getFieldValue('owner-taxcode'),
            CCCD: getFieldValue('owner-cccd'),
            Phone: phoneValue,
            Email: getFieldValue('owner-email'),
            Address: getFieldValue('owner-address'),
            Ward: getFieldValue('owner-ward'),
            Province: getFieldValue('owner-province'),
            ImageUrl: currentOwner?.imageUrl || '',
            CreatedAt: currentOwner?.createdAt
        },
        Vehicle: {
            VehicleId: (() => {
                const val = getFieldValue('vehicle-id');
                return val ? parseInt(val) : null;
            })(),
            PlateNo: getFieldValue('vehicle-plate'),
            InspectionNo: getFieldValue('vehicle-inspection'),
            VehicleGroup: getFieldValue('vehicle-group'),
            VehicleType: getFieldValue('vehicle-type'),
            EnergyType: getFieldValue('vehicle-energy'),
            IsCleanEnergy: getCheckboxValue('vehicle-clean'),
            UsagePermission: getFieldValue('vehicle-usage'),
            Brand: getFieldValue('vehicle-brand'),
            Model: getFieldValue('vehicle-model'),
            EngineNo: getFieldValue('vehicle-engine'),
            Chassis: getFieldValue('vehicle-chassis'),
            ManufactureYear: parseInt(getFieldValue('vehicle-year')) || null,
            ManufactureCountry: getFieldValue('vehicle-country'),
            LifetimeLimitYear: parseInt(getFieldValue('vehicle-lifetime')) || null,
            HasCommercialModification: getCheckboxValue('vehicle-commercial'),
            HasModification: getCheckboxValue('vehicle-modification'),
            CreatedAt: currentVehicle?.createdAt,
            UpdatedAt: currentVehicle?.updatedAt
        },
        Specification: {
            SpecificationId: (() => {
                const val = getFieldValue('spec-id');
                return val ? parseInt(val) : null;
            })(),
            PlateNo: getFieldValue('vehicle-plate'),
            WheelFormula: getFieldValue('spec-wheel-formula'),
            WheelTread: parseInt(getFieldValue('spec-wheel-tread')) || null,
            OverallLength: parseInt(getFieldValue('spec-length')) || null,
            OverallWidth: parseInt(getFieldValue('spec-width')) || null,
            OverallHeight: parseInt(getFieldValue('spec-height')) || null,
            CargoInsideLength: parseInt(getFieldValue('spec-cargo-length')) || null,
            CargoInsideWidth: parseInt(getFieldValue('spec-cargo-width')) || null,
            CargoInsideHeight: parseInt(getFieldValue('spec-cargo-height')) || null,
            Wheelbase: parseInt(getFieldValue('spec-wheelbase')) || null,
            KerbWeight: parseFloat(getFieldValue('spec-kerb-weight')) || null,
            AuthorizedCargoWeight: parseFloat(getFieldValue('spec-cargo-weight')) || null,
            AuthorizedTowedWeight: parseFloat(getFieldValue('spec-towed-weight')) || null,
            AuthorizedTotalWeight: parseFloat(getFieldValue('spec-total-weight')) || null,
            SeatingCapacity: parseInt(getFieldValue('spec-seating')) || null,
            StandingCapacity: parseInt(getFieldValue('spec-standing')) || null,
            LyingCapacity: parseInt(getFieldValue('spec-lying')) || null,
            EngineType: getFieldValue('spec-engine-type'),
            EnginePosition: getFieldValue('spec-engine-position'),
            EngineModel: getFieldValue('spec-engine-model'),
            EngineDisplacement: parseInt(getFieldValue('spec-displacement')) || null,
            MaxPower: parseFloat(getFieldValue('spec-max-power')) || null,
            MaxPowerRPM: parseInt(getFieldValue('spec-max-rpm')) || null,
            FuelType: getFieldValue('spec-fuel-type'),
            MotorType: getFieldValue('spec-motor-type'),
            NumberOfMotors: parseInt(getFieldValue('spec-motor-count')) || null,
            MotorModel: getFieldValue('spec-motor-model'),
            TotalMotorPower: parseFloat(getFieldValue('spec-motor-power')) || null,
            MotorVoltage: parseFloat(getFieldValue('spec-motor-voltage')) || null,
            BatteryType: getFieldValue('spec-battery-type'),
            BatteryVoltage: parseFloat(getFieldValue('spec-battery-voltage')) || null,
            BatteryCapacity: parseFloat(getFieldValue('spec-battery-capacity')) || null,
            TireCount: parseInt(getFieldValue('spec-tire-count')) || null,
            TireSize: getFieldValue('spec-tire-size'),
            TireAxleInfo: getFieldValue('spec-tire-axle'),
            ImagePosition: getFieldValue('spec-image-position'),
            HasTachograph: getCheckboxValue('spec-tachograph'),
            HasDriverCamera: getCheckboxValue('spec-camera'),
            NotIssuedStamp: getCheckboxValue('spec-no-stamp'),
            Notes: getFieldValue('spec-notes'),
            CreatedAt: currentSpecification?.createdAt,
            UpdatedAt: currentSpecification?.updatedAt
        }
    };

    return formData;
}

// ========== VALIDATE, SAVE, CREATE FUNCTIONS ==========
function validateForm(mode) {
    const errors = [];
    const ownerType = getFieldValue('owner-type');
    const fullName = getFieldValue('owner-fullname');

    if (!fullName) {
        errors.push('Vui lòng nhập họ và tên');
    }

    if (ownerType === 'PERSON') {
        const cccd = getFieldValue('owner-cccd');
        if (mode === 'create' && !cccd) {
            errors.push('Vui lòng nhập CCCD/CMND');
        }
    } else if (ownerType === 'COMPANY') {
        const companyName = getFieldValue('owner-company');
        const taxCode = getFieldValue('owner-taxcode');
        if (mode === 'create' && !companyName) {
            errors.push('Vui lòng nhập tên công ty');
        }
        if (mode === 'create' && !taxCode) {
            errors.push('Vui lòng nhập mã số thuế');
        }
    }

    const plateNo = getFieldValue('vehicle-plate');
    if (!plateNo) {
        errors.push('Vui lòng nhập biển số xe');
    }

    return errors;
}

async function saveChanges() {
    try {
        console.log('💾 ========== BẮT ĐẦU LƯU (EDIT) ==========');

        if (!currentOwner || !currentVehicle) {
            showNotification('error', 'Vui lòng tìm kiếm thông tin trước khi cập nhật');
            return;
        }

        const errors = validateForm('edit');
        if (errors.length > 0) {
            showNotification('error', errors.join('<br>'));
            return;
        }

        const formData = new FormData();
        const requestData = collectFormData();

        console.log('📤 Request data:', requestData);

        formData.append('jsonData', JSON.stringify(requestData));

        const fileInput = document.getElementById('owner-image-upload');
        if (fileInput && fileInput.files.length > 0) {
            formData.append('ProfilePicture', fileInput.files[0]);
        }

        const response = await fetch('/api/receive-profile/update', {
            method: 'POST',
            body: formData
        });

        const data = await response.json();
        console.log('📊 Response:', data);

        if (data.success) {
            showNotification('success', data.message);

            if (data.imageUrl) {
                displayOwnerImage(data.imageUrl + '?t=' + new Date().getTime());
            }

            await refreshCurrentData();
        } else {
            showNotification('error', data.message);
        }

    } catch (error) {
        console.error('❌ Save error:', error);
        showNotification('error', 'Có lỗi xảy ra khi lưu');
    }
}

async function createProfile() {
    try {
        console.log('💾 ========== BẮT ĐẦU TẠO MỚI ==========');

        const errors = validateForm('create');
        if (errors.length > 0) {
            showNotification('error', errors.join('<br>'));
            return;
        }

        const formData = new FormData();
        const requestData = collectFormData();

        delete requestData.Owner.OwnerId;
        delete requestData.Vehicle.VehicleId;
        delete requestData.Specification.SpecificationId;

        console.log('📤 Request data:', requestData);

        formData.append('jsonData', JSON.stringify(requestData));

        const fileInput = document.getElementById('owner-image-upload');
        if (fileInput && fileInput.files.length > 0) {
            formData.append('ProfilePicture', fileInput.files[0]);
        }

        const response = await fetch('/api/receive-profile/create', {
            method: 'POST',
            body: formData
        });

        const data = await response.json();
        console.log('📊 Response:', data);

        if (data.success) {
            showNotification('success', data.message);

            setTimeout(() => {
                window.location.href = '/receive-profile';
            }, 2000);
        } else {
            showNotification('error', data.message);
        }

    } catch (error) {
        console.error('❌ Create error:', error);
        showNotification('error', 'Có lỗi xảy ra khi tạo mới');
    }
}

async function refreshCurrentData() {
    const searchCCCD = currentOwner?.cccd || '';
    const searchPlate = currentVehicle?.plateNo || '';

    if (!searchCCCD && !searchPlate) return;

    try {
        const url = `/api/receive-profile/search?cccd=${encodeURIComponent(searchCCCD)}&plateNo=${encodeURIComponent(searchPlate)}`;
        const response = await fetch(url);
        const data = await response.json();

        if (data.success) {
            currentOwner = data.data.owner;
            currentVehicle = data.data.vehicle;
            currentSpecification = data.data.specification;
            await populateForm(data.data);
            console.log('✅ Data refreshed');
        }
    } catch (error) {
        console.error('⚠️ Failed to refresh data:', error);
    }
}

function editProfile() {
    console.log('🔧 Edit Profile clicked');

    if (!currentOwner || !currentVehicle) {
        showNotification('error', 'Vui lòng tìm kiếm thông tin trước');
        return;
    }

    const cccd = currentOwner.cccd || '';
    const plateNo = currentVehicle.plateNo || '';

    window.location.href = `/receive-profile/edit?cccd=${encodeURIComponent(cccd)}&plateNo=${encodeURIComponent(plateNo)}`;
}

function approveProfile() {
    console.log('✅ Approve Profile clicked');

    if (!currentOwner || !currentVehicle) {
        showNotification('error', 'Vui lòng tìm kiếm thông tin trước');
        return;
    }

    const cccd = currentOwner.cccd || currentOwner.taxCode || '';
    const plateNo = currentVehicle.plateNo || '';

    window.location.href = `/receive-profile/approve?cccd=${encodeURIComponent(cccd)}&plateNo=${encodeURIComponent(plateNo)}`;
}

function createNewProfile() {
    window.location.href = '/receive-profile/create';
}

function cancelChanges() {
    if (confirm('Bạn có chắc muốn hủy các thay đổi?')) {
        if (currentOwner && currentVehicle) {
            populateForm({
                owner: currentOwner,
                vehicle: currentVehicle,
                specification: currentSpecification
            });
            showNotification('info', 'Đã hủy thay đổi');
        }
    }
}

function cancelCreate() {
    if (confirm('Bạn có chắc muốn hủy tạo mới?')) {
        window.location.href = '/receive-profile';
    }
}

function clearSearch() {
    setFieldValue('search-cccd', '');
    setFieldValue('search-plate', '');

    const noDataState = document.getElementById('no-data-state');
    const dataDisplay = document.getElementById('data-display');

    if (noDataState) noDataState.style.display = 'flex';
    if (dataDisplay) dataDisplay.style.display = 'none';

    currentOwner = null;
    currentVehicle = null;
    currentSpecification = null;
    currentSearchType = null;
}

// ========== LOAD DATA FUNCTIONS ==========
async function loadProvinces() {
    try {
        const response = await fetch('/api/receive-profile/provinces');
        const data = await response.json();

        if (data.success && data.data) {
            const provinceSelect = document.getElementById('owner-province');
            if (provinceSelect) {
                provinceSelect.innerHTML = '<option value="">-- Chọn Tỉnh/Thành phố --</option>';
                data.data.forEach(province => {
                    const option = document.createElement('option');
                    option.value = province;
                    option.textContent = province;
                    provinceSelect.appendChild(option);
                });
                console.log(`✅ Loaded ${data.data.length} provinces`);
            }
        }
    } catch (error) {
        console.error('❌ Load provinces error:', error);
        showNotification('error', 'Không thể tải danh sách tỉnh/thành phố');
    }
}

async function loadWards(provinceName) {
    try {
        if (!provinceName) return;

        const response = await fetch(`/api/receive-profile/wards?province=${encodeURIComponent(provinceName)}`);
        const data = await response.json();

        const wardSelect = document.getElementById('owner-ward');
        if (data.success && wardSelect) {
            wardSelect.innerHTML = '<option value="">-- Chọn Phường/Xã --</option>';
            wardSelect.disabled = false;

            if (data.data && data.data.length > 0) {
                data.data.forEach(ward => {
                    const option = document.createElement('option');
                    option.value = ward.tenphuongxa;
                    option.textContent = ward.tenphuongxa;
                    wardSelect.appendChild(option);
                });
                console.log(`✅ Loaded ${data.data.length} wards`);
            }
        }
    } catch (error) {
        console.error('❌ Load wards error:', error);
        showNotification('error', 'Không thể tải danh sách phường/xã');
    }
}

async function loadVehicleTypes() {
    console.log('🚗 ========== BẮT ĐẦU LOAD VEHICLE TYPES ==========');

    try {
        const url = '/api/receive-profile/vehicle-types';
        const response = await fetch(url);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();

        if (data.success && data.data) {
            const vehicleTypeSelect = document.getElementById('vehicle-type');

            if (!vehicleTypeSelect) {
                console.error('❌ Cannot find element with id="vehicle-type"');
                return;
            }

            vehicleTypeSelect.innerHTML = '<option value="">-- Chọn loại phương tiện --</option>';

            data.data.forEach(type => {
                const option = document.createElement('option');
                option.value = type.typeName;
                option.textContent = type.typeName;
                option.setAttribute('data-id', type.vehicleTypeId);
                vehicleTypeSelect.appendChild(option);
            });

            console.log(`✅ Added ${data.data.length} vehicle types`);
        }
    } catch (error) {
        console.error('❌ Load vehicle types error:', error);
        showNotification('error', 'Không thể tải danh sách loại phương tiện');
    }
}

function setupProvinceListener() {
    const provinceSelect = document.getElementById('owner-province');
    if (provinceSelect) {
        provinceSelect.addEventListener('change', function () {
            const wardSelect = document.getElementById('owner-ward');
            if (this.value) {
                loadWards(this.value);
            } else if (wardSelect) {
                wardSelect.innerHTML = '<option value="">-- Chọn Phường/Xã --</option>';
                wardSelect.disabled = true;
            }
        });
    }
}

function toggleOwnerType(ownerType) {
    if (!ownerType) {
        ownerType = getFieldValue('owner-type');
    }

    const personInfo = document.getElementById('person-info');
    const companyInfo = document.getElementById('company-info');

    if (ownerType === 'PERSON' || ownerType === 'Cá nhân') {
        if (personInfo) personInfo.style.display = 'flex';
        if (companyInfo) companyInfo.style.display = 'none';
    } else {
        if (personInfo) personInfo.style.display = 'none';
        if (companyInfo) companyInfo.style.display = 'flex';
    }
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

// ========== AUTO INITIALIZE ==========
document.addEventListener('DOMContentLoaded', function () {
    const path = window.location.pathname.toLowerCase();

    if (path.includes('/receive-profile/edit')) {
        initializePage('edit');
    } else if (path.includes('/receive-profile/create')) {
        initializePage('create');
    } else if (path.includes('/receive-profile')) {
        initializePage('view');
    }
});

// ========== EXPOSE TO WINDOW ==========
window.searchProfile = searchProfile;
window.clearSearch = clearSearch;
window.editProfile = editProfile;
window.createNewProfile = createNewProfile;
window.approveProfile = approveProfile;
window.saveChanges = saveChanges;
window.cancelChanges = cancelChanges;
window.createProfile = createProfile;
window.cancelCreate = cancelCreate;
window.previewOwnerImage = previewOwnerImage;
window.toggleOwnerType = toggleOwnerType;
window.loadVehicleTypes = loadVehicleTypes;

console.log('✅ All functions exposed to window scope');