// ========== GLOBAL VARIABLES ==========
let currentOwner = null;
let currentVehicle = null;
let currentSpecification = null;
let currentPageMode = 'view'; // 'view', 'edit', 'create'
let currentSearchType = null; // ✅ 'cccd', 'taxCode', 'plateNo', 'combined_cccd',

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

    const ownerTypeSelect = getElement('owner-type', 'Owner.OwnerType');
    if (ownerTypeSelect) {
        ownerTypeSelect.addEventListener('change', toggleOwnerType);
    }

    setupPhoneSync();

    if (mode === 'edit') {
        await loadDataForEdit();
    }
}

// ========== HELPER FUNCTION: GET ELEMENT BY ID OR NAME ==========
function getElement(id, name = null) {
    // Thử tìm theo id trước
    let element = document.getElementById(id);

    // Nếu không có id, thử tìm theo name
    if (!element && name) {
        element = document.querySelector(`[name="${name}"]`);
    }

    // Nếu vẫn không có, thử tìm theo id là name
    if (!element) {
        element = document.querySelector(`[name="${id}"]`);
    }

    return element;
}

// ========== SETUP PHONE SYNC ==========
function setupPhoneSync() {
    const phone1 = getElement('owner-phone', 'Owner.Phone');
    const phone2 = getElement('owner-phone2', 'Owner.Phone');

    if (phone1 && phone2) {
        phone1.addEventListener('input', function () {
            phone2.value = this.value;
        });
        phone2.addEventListener('input', function () {
            phone1.value = this.value;
        });
    }
}

function createNewProfile() {
    console.log('➕ Create New Profile clicked');

    // Clear current data
    currentOwner = null;
    currentVehicle = null;
    currentSpecification = null;
    currentSearchType = null;

    // Redirect to create page
    window.location.href = '/receive-profile/create';
}

// ========== LOAD DATA CHO TRANG EDIT ==========
async function loadDataForEdit() {
    const urlParams = new URLSearchParams(window.location.search);
    const cccd = urlParams.get('cccd');
    const plateNo = urlParams.get('plateNo');
    const taxCode = urlParams.get('taxCode');

    if (!cccd && !plateNo && !taxCode) {
        showNotification('error', 'Thiếu thông tin để tải dữ liệu');
        return;
    }

    const loadingState = document.getElementById('loading-state');
    const dataDisplay = document.getElementById('data-display');
    const pageHeader = document.getElementById('page-header');
    const breadcrumb = document.getElementById('breadcrumb');

    if (loadingState) loadingState.style.display = 'flex';
    if (dataDisplay) dataDisplay.style.display = 'none';

    try {
        const url = `/api/receive-profile/search?cccd=${encodeURIComponent(cccd || '')}&plateNo=${encodeURIComponent(plateNo || '')}&taxCode=${encodeURIComponent(taxCode || '')}`;
        console.log('📡 Loading data from:', url);

        const response = await fetch(url);
        const data = await response.json();

        if (loadingState) loadingState.style.display = 'none';

        if (data.success) {
            currentOwner = data.data.owner;
            currentVehicle = data.data.vehicle;
            currentSpecification = data.data.specification;
            currentSearchType = data.searchType;

            if (pageHeader) pageHeader.style.display = 'block';
            if (breadcrumb) breadcrumb.style.display = 'block';
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
    const identifierInput = getFieldValue('search-cccd') || '';
    const plateNo = getFieldValue('search-plate') || '';

    console.log('🔍 Search with identifier:', identifierInput, 'plateNo:', plateNo);

    if (!identifierInput && !plateNo) {
        showNotification('warning', 'Vui lòng nhập CCCD/MST hoặc Biển số xe');
        return;
    }

    let cccd = '';
    let taxCode = '';

    if (identifierInput) {
        const cleanInput = identifierInput.replace(/[^0-9]/g, '');
        console.log('🔢 Clean input:', cleanInput, 'Length:', cleanInput.length);

        if (cleanInput.length === 9 || cleanInput.length === 12) {
            cccd = cleanInput;
            console.log('✅ Detected as CCCD:', cccd);
        }
        else if (cleanInput.length >= 10 && cleanInput.length <= 13) {
            taxCode = cleanInput;
            console.log('✅ Detected as Tax Code:', taxCode);
        }
        else {
            showNotification('warning', 'CCCD phải có 9 hoặc 12 chữ số. Mã số thuế phải có 10-13 chữ số');
            return;
        }
    }

    const noDataState = document.getElementById('no-data-state');
    const loadingState = document.getElementById('loading-state');
    const dataDisplay = document.getElementById('data-display');

    if (noDataState) noDataState.style.display = 'none';
    if (loadingState) loadingState.style.display = 'flex';
    if (dataDisplay) dataDisplay.style.display = 'none';

    try {
        const url = `/api/receive-profile/search?cccd=${encodeURIComponent(cccd)}&plateNo=${encodeURIComponent(plateNo)}&taxCode=${encodeURIComponent(taxCode)}`;
        console.log('📡 Request URL:', url);

        const response = await fetch(url);
        const data = await response.json();

        console.log('📊 Response:', data);

        if (loadingState) loadingState.style.display = 'none';

        if (data.success) {
            currentOwner = data.data.owner;
            currentVehicle = data.data.vehicle;
            currentSpecification = data.data.specification;
            currentSearchType = data.searchType;

            if (dataDisplay) dataDisplay.style.display = 'block';

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

// ========== HIỂN THỊ DATA THEO LOẠI TÌM KIẾM ==========
function displayDataBySearchType(searchType) {
    console.log('📋 Displaying data for search type:', searchType);

    const ownerCard = document.querySelector('.owner-info-card');
    const vehicleCards = document.querySelectorAll('.vehicle-info-card');
    const actionButtons = document.querySelector('.action-buttons');

    if (searchType === 'cccd' || searchType === 'taxCode') {
        console.log('👤 Showing OWNER info only (CCCD or TaxCode)');
        if (ownerCard) ownerCard.style.display = 'block';
        vehicleCards.forEach(card => card.style.display = 'none');

    } else if (searchType === 'plateNo') {
        console.log('🚗 Showing VEHICLE and SPECIFICATION only');
        if (ownerCard) ownerCard.style.display = 'none';
        vehicleCards.forEach(card => card.style.display = 'block');

    } else if (searchType === 'combined_cccd' || searchType === 'combined_taxcode') {
        console.log('📋 Showing ALL sections (Owner + Vehicle + Specification)');
        if (ownerCard) ownerCard.style.display = 'block';
        vehicleCards.forEach(card => card.style.display = 'block');
    }

    if (actionButtons) actionButtons.style.display = 'flex';
}

// ========== POPULATE FORM ==========
async function populateForm(data) {
    console.log('🔄 Populating form with data:', data);
    console.log('🔍 Current search type:', currentSearchType);

    if ((currentSearchType === 'cccd' ||
        currentSearchType === 'taxCode' ||
        currentSearchType === 'combined_cccd' ||
        currentSearchType === 'combined_taxcode') && data.owner) {

        console.log('👤 Populating OWNER data...');
        setFieldValue('owner-id', 'Owner.OwnerId', data.owner.ownerId);
        setFieldValue('owner-fullname', 'Owner.FullName', data.owner.fullName);

        const ownerTypeValue = data.owner.ownerType || 'PERSON';
        setFieldValue('owner-type', 'Owner.OwnerType', ownerTypeValue);

        setFieldValue('owner-cccd', 'Owner.CCCD', data.owner.cccd);
        setFieldValue('owner-taxcode', 'Owner.TaxCode', data.owner.taxCode);
        setFieldValue('owner-company', 'Owner.CompanyName', data.owner.companyName);

        const phoneValue = data.owner.phone || '';
        setFieldValue('owner-phone', 'Owner.Phone', phoneValue);
        setFieldValue('owner-phone2', 'Owner.Phone', phoneValue);

        setFieldValue('owner-email', 'Owner.Email', data.owner.email);
        setFieldValue('owner-address', 'Owner.Address', data.owner.address);
        setFieldValue('owner-province', 'Owner.Province', data.owner.province);
        setFieldValue('owner-ward', 'Owner.Ward', data.owner.ward);

        const createdAt = data.owner.createdAt ? new Date(data.owner.createdAt).toLocaleString('vi-VN') : '';
        setFieldValue('owner-created', null, createdAt);

        toggleOwnerType(ownerTypeValue);

        if (data.owner.imageUrl) {
            displayOwnerImage(data.owner.imageUrl);
        }
        console.log('✅ Owner data populated');
    }

    if ((currentSearchType === 'plateNo' ||
        currentSearchType === 'combined_cccd' ||
        currentSearchType === 'combined_taxcode') && data.vehicle) {

        console.log('🚗 Populating VEHICLE data...');
        setFieldValue('vehicle-id', 'Vehicle.VehicleId', data.vehicle.vehicleId);
        setFieldValue('vehicle-plate', 'Vehicle.PlateNo', data.vehicle.plateNo);
        setFieldValue('vehicle-inspection', 'Vehicle.InspectionNo', data.vehicle.inspectionNo);
        setFieldValue('vehicle-group', 'Vehicle.VehicleGroup', data.vehicle.vehicleGroup);

        if (data.vehicle.vehicleType) {
            setTimeout(() => {
                setFieldValue('vehicle-type', 'Vehicle.VehicleTypeId', data.vehicle.vehicleType);
                console.log(`✅ Vehicle type set to: ${data.vehicle.vehicleType}`);
            }, 300);
        }

        setFieldValue('vehicle-energy', 'Vehicle.EnergyType', data.vehicle.energyType);
        setCheckboxValue('vehicle-clean', 'Vehicle.IsCleanEnergy', data.vehicle.isCleanEnergy);
        setFieldValue('vehicle-usage', 'Vehicle.UsagePermission', data.vehicle.usagePermission);
        setFieldValue('vehicle-brand', 'Vehicle.Brand', data.vehicle.brand);
        setFieldValue('vehicle-model', 'Vehicle.Model', data.vehicle.model);
        setFieldValue('vehicle-engine', 'Vehicle.EngineNo', data.vehicle.engineNo);
        setFieldValue('vehicle-chassis', 'Vehicle.Chassis', data.vehicle.chassis);
        setFieldValue('vehicle-year', 'Vehicle.ManufactureYear', data.vehicle.manufactureYear);
        setFieldValue('vehicle-country', 'Vehicle.ManufactureCountry', data.vehicle.manufactureCountry);
        setFieldValue('vehicle-lifetime', 'Vehicle.LifetimeLimitYear', data.vehicle.lifetimeLimitYear);
        setCheckboxValue('vehicle-commercial', 'Vehicle.HasCommercialModification', data.vehicle.hasCommercialModification);
        setCheckboxValue('vehicle-modification', 'Vehicle.HasModification', data.vehicle.hasModification);

        const vehicleCreated = data.vehicle.createdAt ? new Date(data.vehicle.createdAt).toLocaleString('vi-VN') : '';
        const vehicleUpdated = data.vehicle.updatedAt ? new Date(data.vehicle.updatedAt).toLocaleString('vi-VN') : '';
        setFieldValue('vehicle-created', null, vehicleCreated);
        setFieldValue('vehicle-updated', null, vehicleUpdated);
        console.log('✅ Vehicle data populated');
    }

    if ((currentSearchType === 'plateNo' ||
        currentSearchType === 'combined_cccd' ||
        currentSearchType === 'combined_taxcode') && data.specification) {

        console.log('⚙️ Populating SPECIFICATION data...');
        setFieldValue('spec-id', 'Specification.SpecificationId', data.specification.specificationId);
        setFieldValue('spec-wheel-formula', 'Specification.WheelFormula', data.specification.wheelFormula);
        setFieldValue('spec-wheel-tread', 'Specification.WheelTread', data.specification.wheelTread);
        setFieldValue('spec-wheelbase', 'Specification.Wheelbase', data.specification.wheelbase);
        setFieldValue('spec-length', 'Specification.OverallLength', data.specification.overallLength);
        setFieldValue('spec-width', 'Specification.OverallWidth', data.specification.overallWidth);
        setFieldValue('spec-height', 'Specification.OverallHeight', data.specification.overallHeight);
        setFieldValue('spec-cargo-length', 'Specification.CargoInsideLength', data.specification.cargoInsideLength);
        setFieldValue('spec-cargo-width', 'Specification.CargoInsideWidth', data.specification.cargoInsideWidth);
        setFieldValue('spec-cargo-height', 'Specification.CargoInsideHeight', data.specification.cargoInsideHeight);
        setFieldValue('spec-kerb-weight', 'Specification.KerbWeight', data.specification.kerbWeight);
        setFieldValue('spec-cargo-weight', 'Specification.AuthorizedCargoWeight', data.specification.authorizedCargoWeight);
        setFieldValue('spec-towed-weight', 'Specification.AuthorizedTowedWeight', data.specification.authorizedTowedWeight);
        setFieldValue('spec-total-weight', 'Specification.AuthorizedTotalWeight', data.specification.authorizedTotalWeight);
        setFieldValue('spec-seating', 'Specification.SeatingCapacity', data.specification.seatingCapacity);
        setFieldValue('spec-standing', 'Specification.StandingCapacity', data.specification.standingCapacity);
        setFieldValue('spec-lying', 'Specification.LyingCapacity', data.specification.lyingCapacity);
        setFieldValue('spec-engine-type', 'Specification.EngineType', data.specification.engineType);
        setFieldValue('spec-engine-position', 'Specification.EnginePosition', data.specification.enginePosition);
        setFieldValue('spec-engine-model', 'Specification.EngineModel', data.specification.engineModel);
        setFieldValue('spec-displacement', 'Specification.EngineDisplacement', data.specification.engineDisplacement);
        setFieldValue('spec-max-power', 'Specification.MaxPower', data.specification.maxPower);
        setFieldValue('spec-max-rpm', 'Specification.MaxPowerRPM', data.specification.maxPowerRPM);
        setFieldValue('spec-fuel-type', 'Specification.FuelType', data.specification.fuelType);
        setFieldValue('spec-motor-type', 'Specification.MotorType', data.specification.motorType);
        setFieldValue('spec-motor-count', 'Specification.NumberOfMotors', data.specification.numberOfMotors);
        setFieldValue('spec-motor-model', 'Specification.MotorModel', data.specification.motorModel);
        setFieldValue('spec-motor-power', 'Specification.TotalMotorPower', data.specification.totalMotorPower);
        setFieldValue('spec-motor-voltage', 'Specification.MotorVoltage', data.specification.motorVoltage);
        setFieldValue('spec-battery-type', 'Specification.BatteryType', data.specification.batteryType);
        setFieldValue('spec-battery-voltage', 'Specification.BatteryVoltage', data.specification.batteryVoltage);
        setFieldValue('spec-battery-capacity', 'Specification.BatteryCapacity', data.specification.batteryCapacity);
        setFieldValue('spec-tire-count', 'Specification.TireCount', data.specification.tireCount);
        setFieldValue('spec-tire-size', 'Specification.TireSize', data.specification.tireSize);
        setFieldValue('spec-tire-axle', 'Specification.TireAxleInfo', data.specification.tireAxleInfo);
        setFieldValue('spec-image-position', 'Specification.ImagePosition', data.specification.imagePosition);
        setCheckboxValue('spec-tachograph', 'Specification.HasTachograph', data.specification.hasTachograph);
        setCheckboxValue('spec-camera', 'Specification.HasDriverCamera', data.specification.hasDriverCamera);
        setCheckboxValue('spec-no-stamp', 'Specification.NotIssuedStamp', data.specification.notIssuedStamp);
        setFieldValue('spec-notes', 'Specification.Notes', data.specification.notes);
        console.log('✅ Specification data populated');
    }

    console.log('✅ Form populated successfully');
}

// ========== HELPER FUNCTIONS (ĐÃ SỬA) ==========
function setFieldValue(fieldId, fieldName, value) {
    const field = getElement(fieldId, fieldName);
    if (field) {
        field.value = value || '';
    }
}

function setCheckboxValue(fieldId, fieldName, value) {
    const field = getElement(fieldId, fieldName);
    if (field) {
        field.checked = value || false;
    }
}

function getFieldValue(fieldId, fieldName = null, defaultValue = '') {
    const field = getElement(fieldId, fieldName);
    return field?.value?.trim() || defaultValue;
}

function getCheckboxValue(fieldId, fieldName = null, defaultValue = false) {
    const field = getElement(fieldId, fieldName);
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

// ========== COLLECT FORM DATA (ĐÃ SỬA) ==========
function collectFormData() {
    const ownerType = getFieldValue('owner-type', 'Owner.OwnerType');
    const phoneValue = getFieldValue('owner-phone2', 'Owner.Phone') || getFieldValue('owner-phone', 'Owner.Phone');

    const formData = {
        Owner: {
            OwnerId: getFieldValue('owner-id', 'Owner.OwnerId') || '00000000-0000-0000-0000-000000000000',
            OwnerType: ownerType || 'PERSON',
            FullName: getFieldValue('owner-fullname', 'Owner.FullName'),
            CompanyName: getFieldValue('owner-company', 'Owner.CompanyName'),
            TaxCode: getFieldValue('owner-taxcode', 'Owner.TaxCode'),
            CCCD: getFieldValue('owner-cccd', 'Owner.CCCD'),
            Phone: phoneValue,
            Email: getFieldValue('owner-email', 'Owner.Email'),
            Address: getFieldValue('owner-address', 'Owner.Address'),
            Ward: getFieldValue('owner-ward', 'Owner.Ward'),
            Province: getFieldValue('owner-province', 'Owner.Province'),
            ImageUrl: currentOwner?.imageUrl || '',
            CreatedAt: currentOwner?.createdAt
        },
        Vehicle: {
            VehicleId: (() => {
                const val = getFieldValue('vehicle-id', 'Vehicle.VehicleId');
                return val ? parseInt(val) : null;
            })(),
            PlateNo: getFieldValue('vehicle-plate', 'Vehicle.PlateNo'),
            InspectionNo: getFieldValue('vehicle-inspection', 'Vehicle.InspectionNo'),
            VehicleGroup: getFieldValue('vehicle-group', 'Vehicle.VehicleGroup'),
            VehicleType: getFieldValue('vehicle-type', 'Vehicle.VehicleTypeId'),
            EnergyType: getFieldValue('vehicle-energy', 'Vehicle.EnergyType'),
            IsCleanEnergy: getCheckboxValue('vehicle-clean', 'Vehicle.IsCleanEnergy'),
            UsagePermission: getFieldValue('vehicle-usage', 'Vehicle.UsagePermission'),
            Brand: getFieldValue('vehicle-brand', 'Vehicle.Brand'),
            Model: getFieldValue('vehicle-model', 'Vehicle.Model'),
            EngineNo: getFieldValue('vehicle-engine', 'Vehicle.EngineNo'),
            Chassis: getFieldValue('vehicle-chassis', 'Vehicle.Chassis'),
            ManufactureYear: parseInt(getFieldValue('vehicle-year', 'Vehicle.ManufactureYear')) || null,
            ManufactureCountry: getFieldValue('vehicle-country', 'Vehicle.ManufactureCountry'),
            LifetimeLimitYear: parseInt(getFieldValue('vehicle-lifetime', 'Vehicle.LifetimeLimitYear')) || null,
            HasCommercialModification: getCheckboxValue('vehicle-commercial', 'Vehicle.HasCommercialModification'),
            HasModification: getCheckboxValue('vehicle-modification', 'Vehicle.HasModification'),
            CreatedAt: currentVehicle?.createdAt,
            UpdatedAt: currentVehicle?.updatedAt
        },
        Specification: {
            SpecificationId: (() => {
                const val = getFieldValue('spec-id', 'Specification.SpecificationId');
                return val ? parseInt(val) : null;
            })(),
            PlateNo: getFieldValue('vehicle-plate', 'Vehicle.PlateNo'),
            WheelFormula: getFieldValue('spec-wheel-formula', 'Specification.WheelFormula'),
            WheelTread: parseInt(getFieldValue('spec-wheel-tread', 'Specification.WheelTread')) || null,
            OverallLength: parseInt(getFieldValue('spec-length', 'Specification.OverallLength')) || null,
            OverallWidth: parseInt(getFieldValue('spec-width', 'Specification.OverallWidth')) || null,
            OverallHeight: parseInt(getFieldValue('spec-height', 'Specification.OverallHeight')) || null,
            CargoInsideLength: parseInt(getFieldValue('spec-cargo-length', 'Specification.CargoInsideLength')) || null,
            CargoInsideWidth: parseInt(getFieldValue('spec-cargo-width', 'Specification.CargoInsideWidth')) || null,
            CargoInsideHeight: parseInt(getFieldValue('spec-cargo-height', 'Specification.CargoInsideHeight')) || null,
            Wheelbase: parseInt(getFieldValue('spec-wheelbase', 'Specification.Wheelbase')) || null,
            KerbWeight: parseFloat(getFieldValue('spec-kerb-weight', 'Specification.KerbWeight')) || null,
            AuthorizedCargoWeight: parseFloat(getFieldValue('spec-cargo-weight', 'Specification.AuthorizedCargoWeight')) || null,
            AuthorizedTowedWeight: parseFloat(getFieldValue('spec-towed-weight', 'Specification.AuthorizedTowedWeight')) || null,
            AuthorizedTotalWeight: parseFloat(getFieldValue('spec-total-weight', 'Specification.AuthorizedTotalWeight')) || null,
            SeatingCapacity: parseInt(getFieldValue('spec-seating', 'Specification.SeatingCapacity')) || null,
            StandingCapacity: parseInt(getFieldValue('spec-standing', 'Specification.StandingCapacity')) || null,
            LyingCapacity: parseInt(getFieldValue('spec-lying', 'Specification.LyingCapacity')) || null,
            EngineType: getFieldValue('spec-engine-type', 'Specification.EngineType'),
            EnginePosition: getFieldValue('spec-engine-position', 'Specification.EnginePosition'),
            EngineModel: getFieldValue('spec-engine-model', 'Specification.EngineModel'),
            EngineDisplacement: parseInt(getFieldValue('spec-displacement', 'Specification.EngineDisplacement')) || null,
            MaxPower: parseFloat(getFieldValue('spec-max-power', 'Specification.MaxPower')) || null,
            MaxPowerRPM: parseInt(getFieldValue('spec-max-rpm', 'Specification.MaxPowerRPM')) || null,
            FuelType: getFieldValue('spec-fuel-type', 'Specification.FuelType'),
            MotorType: getFieldValue('spec-motor-type', 'Specification.MotorType'),
            NumberOfMotors: parseInt(getFieldValue('spec-motor-count', 'Specification.NumberOfMotors')) || null,
            MotorModel: getFieldValue('spec-motor-model', 'Specification.MotorModel'),
            TotalMotorPower: parseFloat(getFieldValue('spec-motor-power', 'Specification.TotalMotorPower')) || null,
            MotorVoltage: parseFloat(getFieldValue('spec-motor-voltage', 'Specification.MotorVoltage')) || null,
            BatteryType: getFieldValue('spec-battery-type', 'Specification.BatteryType'),
            BatteryVoltage: parseFloat(getFieldValue('spec-battery-voltage', 'Specification.BatteryVoltage')) || null,
            BatteryCapacity: parseFloat(getFieldValue('spec-battery-capacity', 'Specification.BatteryCapacity')) || null,
            TireCount: parseInt(getFieldValue('spec-tire-count', 'Specification.TireCount')) || null,
            TireSize: getFieldValue('spec-tire-size', 'Specification.TireSize'),
            TireAxleInfo: getFieldValue('spec-tire-axle', 'Specification.TireAxleInfo'),
            ImagePosition: getFieldValue('spec-image-position', 'Specification.ImagePosition'),
            HasTachograph: getCheckboxValue('spec-tachograph', 'Specification.HasTachograph'),
            HasDriverCamera: getCheckboxValue('spec-camera', 'Specification.HasDriverCamera'),
            NotIssuedStamp: getCheckboxValue('spec-no-stamp', 'Specification.NotIssuedStamp'),
            Notes: getFieldValue('spec-notes', 'Specification.Notes'),
            CreatedAt: currentSpecification?.createdAt,
            UpdatedAt: currentSpecification?.updatedAt
        }
    };

    return formData;
}

// ========== VALIDATE, SAVE, CREATE FUNCTIONS ==========
function validateForm(mode) {
    const errors = [];
    const ownerType = getFieldValue('owner-type', 'Owner.OwnerType');
    const fullName = getFieldValue('owner-fullname', 'Owner.FullName');

    if (!fullName) {
        errors.push('Vui lòng nhập họ và tên');
    }

    if (ownerType === 'PERSON') {
        const cccd = getFieldValue('owner-cccd', 'Owner.CCCD');
        if (mode === 'create' && !cccd) {
            errors.push('Vui lòng nhập CCCD/CMND');
        }
    } else if (ownerType === 'COMPANY') {
        const companyName = getFieldValue('owner-company', 'Owner.CompanyName');
        const taxCode = getFieldValue('owner-taxcode', 'Owner.TaxCode');
        if (mode === 'create' && !companyName) {
            errors.push('Vui lòng nhập tên công ty');
        }
        if (mode === 'create' && !taxCode) {
            errors.push('Vui lòng nhập mã số thuế');
        }
    }

    const plateNo = getFieldValue('vehicle-plate', 'Vehicle.PlateNo');
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

        const fileInput = getElement('owner-image-upload');
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

        // ✅ THÊM ĐOẠN DEBUG NÀY
        const plateNoValue = getFieldValue('vehicle-plate', 'Vehicle.PlateNo');
        console.log('🚗 PlateNo value:', plateNoValue);
        console.log('🚗 PlateNo element:', getElement('vehicle-plate', 'Vehicle.PlateNo'));

        const errors = validateForm('create');
        if (errors.length > 0) {
            console.log('❌ Validation errors:', errors);
            showNotification('error', errors.join('<br>'));
            return;
        }

        const formData = new FormData();
        const requestData = collectFormData();

        // ✅ THÊM LOG ĐỂ XEM REQUEST DATA
        console.log('📤 Full Request data:', JSON.stringify(requestData, null, 2));

        delete requestData.Owner.OwnerId;
        delete requestData.Vehicle.VehicleId;
        delete requestData.Specification.SpecificationId;

        formData.append('jsonData', JSON.stringify(requestData));

        const fileInput = getElement('owner-image-upload');
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
    const searchTaxCode = currentOwner?.taxCode || '';
    const searchPlate = currentVehicle?.plateNo || '';

    if (!searchCCCD && !searchTaxCode && !searchPlate) return;

    try {
        const url = `/api/receive-profile/search?cccd=${encodeURIComponent(searchCCCD)}&plateNo=${encodeURIComponent(searchPlate)}&taxCode=${encodeURIComponent(searchTaxCode)}`;
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
    const taxCode = currentOwner.taxCode || '';
    const plateNo = currentVehicle.plateNo || '';

    window.location.href = `/receive-profile/edit?cccd=${encodeURIComponent(cccd)}&plateNo=${encodeURIComponent(plateNo)}&taxCode=${encodeURIComponent(taxCode)}`;
}

function approveProfile() {
    console.log('✅ Approve Profile clicked');

    if (!currentOwner || !currentVehicle) {
        showNotification('error', 'Vui lòng tìm kiếm thông tin trước');
        return;
    }

    const cccd = currentOwner.cccd || '';
    const taxCode = currentOwner.taxCode || '';
    const plateNo = currentVehicle.plateNo || '';

    window.location.href = `/receive-profile/approve?cccd=${encodeURIComponent(cccd)}&plateNo=${encodeURIComponent(plateNo)}&taxCode=${encodeURIComponent(taxCode)}`;
}

function cancelChanges() {
    console.log('❌ Cancel Changes clicked');
    window.location.href = '/receive-profile';
}

function cancelCreate() {
    console.log('❌ Cancel Create clicked');
    window.location.href = '/receive-profile';
}

function clearSearch() {
    setFieldValue('search-cccd', null, '');
    setFieldValue('search-plate', null, '');

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
            const provinceSelect = getElement('owner-province', 'Owner.Province');
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

        const wardSelect = getElement('owner-ward', 'Owner.Ward');
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
            const vehicleTypeSelect = getElement('vehicle-type', 'Vehicle.VehicleTypeId');

            if (!vehicleTypeSelect) {
                console.error('❌ Cannot find element with id="vehicle-type" or name="Vehicle.VehicleTypeId"');
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
    const provinceSelect = getElement('owner-province', 'Owner.Province');
    if (provinceSelect) {
        provinceSelect.addEventListener('change', function () {
            const wardSelect = getElement('owner-ward', 'Owner.Ward');
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
        ownerType = getFieldValue('owner-type', 'Owner.OwnerType');
    }

    const personInfo = document.getElementById('person-info');
    const companyInfo = document.getElementById('company-info');

    if (ownerType === 'PERSON' || ownerType === 'Cá nhân') {
        if (personInfo) personInfo.style.display = 'grid';
        if (companyInfo) companyInfo.style.display = 'none';
    } else {
        if (personInfo) personInfo.style.display = 'none';
        if (companyInfo) companyInfo.style.display = 'grid';
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