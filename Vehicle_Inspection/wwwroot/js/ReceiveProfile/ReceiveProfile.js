// ========== GLOBAL VARIABLES ==========
let currentOwner = null;
let currentVehicle = null;
let currentSpecification = null;

// ========== SEARCH FUNCTION ==========
// ========== SEARCH FUNCTION ==========
async function searchProfile() {
    const cccd = document.getElementById('search-cccd')?.value?.trim() || '';
    const plateNo = document.getElementById('search-plate')?.value?.trim() || '';

    console.log('🔍 Search with:', { cccd, plateNo });

    if (!cccd && !plateNo) {
        alert('Vui lòng nhập CCCD hoặc Biển số xe');
        return;
    }

    // Show loading state
    document.getElementById('no-data-state').style.display = 'none';
    document.getElementById('loading-state').style.display = 'flex';
    document.getElementById('data-display').style.display = 'none';

    try {
        const url = `/api/receive-profile/search?cccd=${encodeURIComponent(cccd)}&plateNo=${encodeURIComponent(plateNo)}`;
        console.log('📡 Request URL:', url);

        const response = await fetch(url, {
            method: 'GET'
        });

        console.log('📊 Response status:', response.status);

        const data = await response.json();
        console.log('📊 Response data:', data);

        // Hide loading
        document.getElementById('loading-state').style.display = 'none';

        if (data.success) {
            // Lưu dữ liệu vào biến global
            currentOwner = data.data.owner;
            currentVehicle = data.data.vehicle;
            currentSpecification = data.data.specification;

            console.log('✅ Search success:', data);

            // Show data display
            document.getElementById('data-display').style.display = 'block';

            // ✅ Populate form với dữ liệu tìm được (async)
            await populateForm(data.data);

            showNotification('success', data.message);
        } else {
            // Show no data state again
            document.getElementById('no-data-state').style.display = 'flex';
            showNotification('error', data.message);
        }
    } catch (error) {
        console.error('❌ Search error:', error);
        document.getElementById('loading-state').style.display = 'none';
        document.getElementById('no-data-state').style.display = 'flex';
        showNotification('error', 'Có lỗi xảy ra khi tìm kiếm: ' + error.message);
    }
}

// ========== POPULATE FORM ==========
// ========== POPULATE FORM ==========
async function populateForm(data) {
    console.log('🔄 Populating form with data:', data);

    // Owner fields
    if (data.owner) {
        document.getElementById('owner-id').value = data.owner.ownerId || '';
        document.getElementById('owner-fullname').value = data.owner.fullName || '';
        document.getElementById('owner-type').value = data.owner.ownerType || 'PERSON';
        document.getElementById('owner-cccd').value = data.owner.cccd || '';

        // Sync cả 2 trường phone
        const phoneValue = data.owner.phone || '';
        document.getElementById('owner-phone').value = phoneValue;
        document.getElementById('owner-phone2').value = phoneValue;

        document.getElementById('owner-email').value = data.owner.email || '';
        document.getElementById('owner-address').value = data.owner.address || '';
        document.getElementById('owner-company').value = data.owner.companyName || '';
        document.getElementById('owner-taxcode').value = data.owner.taxCode || '';
        document.getElementById('owner-created').value = data.owner.createdAt ? new Date(data.owner.createdAt).toLocaleString('vi-VN') : '';

        console.log('📍 Data Province:', data.owner.province);
        console.log('📍 Data Ward:', data.owner.ward);

        // Toggle owner type display
        toggleOwnerType();

        // Display image if exists
        if (data.owner.imageUrl) {
            const imgContainer = document.getElementById('owner-image-container');
            if (imgContainer) {
                imgContainer.innerHTML = `<img src="${data.owner.imageUrl}" alt="Owner Image" class="owner-image">`;
            }
        }

        // ✅ QUAN TRỌNG: Load Province trước, sau đó load Ward và set giá trị
        const provinceSelect = document.getElementById('owner-province');
        const wardSelect = document.getElementById('owner-ward');

        if (data.owner.province) {
            // Set province value
            provinceSelect.value = data.owner.province;
            console.log('✅ Province set to:', provinceSelect.value);

            // Load wards cho province này
            console.log('🔄 Loading wards for:', data.owner.province);

            try {
                await loadWards(data.owner.province);

                // Đợi một chút để đảm bảo wards đã được load
                setTimeout(() => {
                    if (data.owner.ward) {
                        wardSelect.value = data.owner.ward;
                        console.log('✅ Ward set to:', wardSelect.value);

                        // Verify
                        if (wardSelect.value !== data.owner.ward) {
                            console.error('❌ Ward value mismatch!');
                            console.error('   Expected:', data.owner.ward);
                            console.error('   Actual:', wardSelect.value);
                            console.error('   Available options:', Array.from(wardSelect.options).map(o => o.value));
                        }
                    }
                }, 200);
            } catch (error) {
                console.error('❌ Error loading wards:', error);
            }
        } else {
            // Reset province và ward nếu không có dữ liệu
            provinceSelect.value = '';
            wardSelect.innerHTML = '<option value="">-- Chọn Phường/Xã --</option>';
            wardSelect.disabled = true;
        }
    }

    // Vehicle fields
    if (data.vehicle) {
        document.getElementById('vehicle-id').value = data.vehicle.vehicleId || '';
        document.getElementById('vehicle-plate').value = data.vehicle.plateNo || '';
        document.getElementById('vehicle-inspection').value = data.vehicle.inspectionNo || '';
        document.getElementById('vehicle-group').value = data.vehicle.vehicleGroup || '';
        document.getElementById('vehicle-type').value = data.vehicle.vehicleType || '';
        document.getElementById('vehicle-energy').value = data.vehicle.energyType || '';
        document.getElementById('vehicle-clean').checked = data.vehicle.isCleanEnergy || false;
        document.getElementById('vehicle-usage').value = data.vehicle.usagePermission || '';
        document.getElementById('vehicle-brand').value = data.vehicle.brand || '';
        document.getElementById('vehicle-model').value = data.vehicle.model || '';
        document.getElementById('vehicle-engine').value = data.vehicle.engineNo || '';
        document.getElementById('vehicle-chassis').value = data.vehicle.chassis || '';
        document.getElementById('vehicle-year').value = data.vehicle.manufactureYear || '';
        document.getElementById('vehicle-country').value = data.vehicle.manufactureCountry || '';
        document.getElementById('vehicle-lifetime').value = data.vehicle.lifetimeLimitYear || '';
        document.getElementById('vehicle-commercial').checked = data.vehicle.hasCommercialModification || false;
        document.getElementById('vehicle-modification').checked = data.vehicle.hasModification || false;
        document.getElementById('vehicle-created').value = data.vehicle.createdAt ? new Date(data.vehicle.createdAt).toLocaleString('vi-VN') : '';
        document.getElementById('vehicle-updated').value = data.vehicle.updatedAt ? new Date(data.vehicle.updatedAt).toLocaleString('vi-VN') : '';
    }

    // Specification fields
    if (data.specification) {
        document.getElementById('spec-id').value = data.specification.specificationId || '';
        document.getElementById('spec-wheel-formula').value = data.specification.wheelFormula || '';
        document.getElementById('spec-wheel-tread').value = data.specification.wheelTread || '';
        document.getElementById('spec-wheelbase').value = data.specification.wheelbase || '';
        document.getElementById('spec-length').value = data.specification.overallLength || '';
        document.getElementById('spec-width').value = data.specification.overallWidth || '';
        document.getElementById('spec-height').value = data.specification.overallHeight || '';
        document.getElementById('spec-cargo-length').value = data.specification.cargoInsideLength || '';
        document.getElementById('spec-cargo-width').value = data.specification.cargoInsideWidth || '';
        document.getElementById('spec-cargo-height').value = data.specification.cargoInsideHeight || '';
        document.getElementById('spec-kerb-weight').value = data.specification.kerbWeight || '';
        document.getElementById('spec-cargo-weight').value = data.specification.authorizedCargoWeight || '';
        document.getElementById('spec-towed-weight').value = data.specification.authorizedTowedWeight || '';
        document.getElementById('spec-total-weight').value = data.specification.authorizedTotalWeight || '';
        document.getElementById('spec-seating').value = data.specification.seatingCapacity || '';
        document.getElementById('spec-standing').value = data.specification.standingCapacity || '';
        document.getElementById('spec-lying').value = data.specification.lyingCapacity || '';
        document.getElementById('spec-engine-type').value = data.specification.engineType || '';
        document.getElementById('spec-engine-position').value = data.specification.enginePosition || '';
        document.getElementById('spec-engine-model').value = data.specification.engineModel || '';
        document.getElementById('spec-displacement').value = data.specification.engineDisplacement || '';
        document.getElementById('spec-max-power').value = data.specification.maxPower || '';
        document.getElementById('spec-max-rpm').value = data.specification.maxPowerRPM || '';
        document.getElementById('spec-fuel-type').value = data.specification.fuelType || '';
        document.getElementById('spec-motor-type').value = data.specification.motorType || '';
        document.getElementById('spec-motor-count').value = data.specification.numberOfMotors || '';
        document.getElementById('spec-motor-model').value = data.specification.motorModel || '';
        document.getElementById('spec-motor-power').value = data.specification.totalMotorPower || '';
        document.getElementById('spec-motor-voltage').value = data.specification.motorVoltage || '';
        document.getElementById('spec-battery-type').value = data.specification.batteryType || '';
        document.getElementById('spec-battery-voltage').value = data.specification.batteryVoltage || '';
        document.getElementById('spec-battery-capacity').value = data.specification.batteryCapacity || '';
        document.getElementById('spec-tire-count').value = data.specification.tireCount || '';
        document.getElementById('spec-tire-size').value = data.specification.tireSize || '';
        document.getElementById('spec-tire-axle').value = data.specification.tireAxleInfo || '';
        document.getElementById('spec-image-position').value = data.specification.imagePosition || '';
        document.getElementById('spec-tachograph').checked = data.specification.hasTachograph || false;
        document.getElementById('spec-camera').checked = data.specification.hasDriverCamera || false;
        document.getElementById('spec-no-stamp').checked = data.specification.notIssuedStamp || false;
        document.getElementById('spec-notes').value = data.specification.notes || '';
    }

    console.log('✅ Form populated successfully');
}

// ========== SAVE CHANGES FUNCTION ==========
async function saveChanges() {
    try {
        console.log('💾 ========== BẮT ĐẦU LƯU ==========');

        if (!currentOwner || !currentVehicle) {
            showNotification('error', 'Vui lòng tìm kiếm thông tin trước khi cập nhật');
            return;
        }

        const formData = new FormData();

        // Lấy Province/Ward
        const provinceSelect = document.getElementById('owner-province');
        const wardSelect = document.getElementById('owner-ward');

        const selectedProvince = provinceSelect?.value?.trim() || '';
        const selectedWard = wardSelect?.value?.trim() || '';

        console.log('📍 Province VALUE:', selectedProvince);
        console.log('📍 Ward VALUE:', selectedWard);

        // Lấy phone
        const phoneValue = document.getElementById('owner-phone2')?.value?.trim() ||
            document.getElementById('owner-phone')?.value?.trim() ||
            currentOwner.phone;

        // Chuẩn bị request data
        const requestData = {
            Owner: {
                OwnerId: currentOwner.ownerId,
                OwnerType: document.getElementById('owner-type')?.value || currentOwner.ownerType,
                FullName: document.getElementById('owner-fullname')?.value?.trim() || currentOwner.fullName,
                CompanyName: document.getElementById('owner-company')?.value?.trim() || currentOwner.companyName,
                TaxCode: document.getElementById('owner-taxcode')?.value?.trim() || currentOwner.taxCode,
                CCCD: document.getElementById('owner-cccd')?.value?.trim() || currentOwner.cccd,
                Phone: phoneValue,
                Email: document.getElementById('owner-email')?.value?.trim() || currentOwner.email,
                Address: document.getElementById('owner-address')?.value?.trim() || currentOwner.address,
                Ward: selectedWard,
                Province: selectedProvince,
                ImageUrl: currentOwner.imageUrl,
                CreatedAt: currentOwner.createdAt
            },
            Vehicle: {
                VehicleId: currentVehicle.vehicleId,
                PlateNo: document.getElementById('vehicle-plate')?.value?.trim() || currentVehicle.plateNo,
                InspectionNo: document.getElementById('vehicle-inspection')?.value?.trim() || currentVehicle.inspectionNo,
                VehicleGroup: document.getElementById('vehicle-group')?.value?.trim() || currentVehicle.vehicleGroup,
                VehicleType: document.getElementById('vehicle-type')?.value?.trim() || currentVehicle.vehicleType,
                EnergyType: document.getElementById('vehicle-energy')?.value?.trim() || currentVehicle.energyType,
                IsCleanEnergy: document.getElementById('vehicle-clean')?.checked ?? currentVehicle.isCleanEnergy,
                UsagePermission: document.getElementById('vehicle-usage')?.value?.trim() || currentVehicle.usagePermission,
                Brand: document.getElementById('vehicle-brand')?.value?.trim() || currentVehicle.brand,
                Model: document.getElementById('vehicle-model')?.value?.trim() || currentVehicle.model,
                EngineNo: document.getElementById('vehicle-engine')?.value?.trim() || currentVehicle.engineNo,
                Chassis: document.getElementById('vehicle-chassis')?.value?.trim() || currentVehicle.chassis,
                ManufactureYear: parseInt(document.getElementById('vehicle-year')?.value) || currentVehicle.manufactureYear,
                ManufactureCountry: document.getElementById('vehicle-country')?.value?.trim() || currentVehicle.manufactureCountry,
                LifetimeLimitYear: parseInt(document.getElementById('vehicle-lifetime')?.value) || currentVehicle.lifetimeLimitYear,
                HasCommercialModification: document.getElementById('vehicle-commercial')?.checked ?? currentVehicle.hasCommercialModification,
                HasModification: document.getElementById('vehicle-modification')?.checked ?? currentVehicle.hasModification,
                CreatedAt: currentVehicle.createdAt,
                UpdatedAt: currentVehicle.updatedAt
            },
            Specification: currentSpecification ? {
                SpecificationId: currentSpecification.specificationId,
                PlateNo: document.getElementById('vehicle-plate')?.value?.trim() || currentSpecification.plateNo,
                WheelFormula: document.getElementById('spec-wheel-formula')?.value?.trim() || currentSpecification.wheelFormula,
                WheelTread: parseInt(document.getElementById('spec-wheel-tread')?.value) || currentSpecification.wheelTread,
                OverallLength: parseInt(document.getElementById('spec-length')?.value) || currentSpecification.overallLength,
                OverallWidth: parseInt(document.getElementById('spec-width')?.value) || currentSpecification.overallWidth,
                OverallHeight: parseInt(document.getElementById('spec-height')?.value) || currentSpecification.overallHeight,
                CargoInsideLength: parseInt(document.getElementById('spec-cargo-length')?.value) || currentSpecification.cargoInsideLength,
                CargoInsideWidth: parseInt(document.getElementById('spec-cargo-width')?.value) || currentSpecification.cargoInsideWidth,
                CargoInsideHeight: parseInt(document.getElementById('spec-cargo-height')?.value) || currentSpecification.cargoInsideHeight,
                Wheelbase: parseInt(document.getElementById('spec-wheelbase')?.value) || currentSpecification.wheelbase,
                KerbWeight: parseFloat(document.getElementById('spec-kerb-weight')?.value) || currentSpecification.kerbWeight,
                AuthorizedCargoWeight: parseFloat(document.getElementById('spec-cargo-weight')?.value) || currentSpecification.authorizedCargoWeight,
                AuthorizedTowedWeight: parseFloat(document.getElementById('spec-towed-weight')?.value) || currentSpecification.authorizedTowedWeight,
                AuthorizedTotalWeight: parseFloat(document.getElementById('spec-total-weight')?.value) || currentSpecification.authorizedTotalWeight,
                SeatingCapacity: parseInt(document.getElementById('spec-seating')?.value) || currentSpecification.seatingCapacity,
                StandingCapacity: parseInt(document.getElementById('spec-standing')?.value) || currentSpecification.standingCapacity,
                LyingCapacity: parseInt(document.getElementById('spec-lying')?.value) || currentSpecification.lyingCapacity,
                EngineType: document.getElementById('spec-engine-type')?.value?.trim() || currentSpecification.engineType,
                EnginePosition: document.getElementById('spec-engine-position')?.value?.trim() || currentSpecification.enginePosition,
                EngineModel: document.getElementById('spec-engine-model')?.value?.trim() || currentSpecification.engineModel,
                EngineDisplacement: parseInt(document.getElementById('spec-displacement')?.value) || currentSpecification.engineDisplacement,
                MaxPower: parseFloat(document.getElementById('spec-max-power')?.value) || currentSpecification.maxPower,
                MaxPowerRPM: parseInt(document.getElementById('spec-max-rpm')?.value) || currentSpecification.maxPowerRPM,
                FuelType: document.getElementById('spec-fuel-type')?.value?.trim() || currentSpecification.fuelType,
                MotorType: document.getElementById('spec-motor-type')?.value?.trim() || currentSpecification.motorType,
                NumberOfMotors: parseInt(document.getElementById('spec-motor-count')?.value) || currentSpecification.numberOfMotors,
                MotorModel: document.getElementById('spec-motor-model')?.value?.trim() || currentSpecification.motorModel,
                TotalMotorPower: parseFloat(document.getElementById('spec-motor-power')?.value) || currentSpecification.totalMotorPower,
                MotorVoltage: parseFloat(document.getElementById('spec-motor-voltage')?.value) || currentSpecification.motorVoltage,
                BatteryType: document.getElementById('spec-battery-type')?.value?.trim() || currentSpecification.batteryType,
                BatteryVoltage: parseFloat(document.getElementById('spec-battery-voltage')?.value) || currentSpecification.batteryVoltage,
                BatteryCapacity: parseFloat(document.getElementById('spec-battery-capacity')?.value) || currentSpecification.batteryCapacity,
                TireCount: parseInt(document.getElementById('spec-tire-count')?.value) || currentSpecification.tireCount,
                TireSize: document.getElementById('spec-tire-size')?.value?.trim() || currentSpecification.tireSize,
                TireAxleInfo: document.getElementById('spec-tire-axle')?.value?.trim() || currentSpecification.tireAxleInfo,
                ImagePosition: document.getElementById('spec-image-position')?.value?.trim() || currentSpecification.imagePosition,
                HasTachograph: document.getElementById('spec-tachograph')?.checked ?? currentSpecification.hasTachograph,
                HasDriverCamera: document.getElementById('spec-camera')?.checked ?? currentSpecification.hasDriverCamera,
                NotIssuedStamp: document.getElementById('spec-no-stamp')?.checked ?? currentSpecification.notIssuedStamp,
                Notes: document.getElementById('spec-notes')?.value?.trim() || currentSpecification.notes,
                CreatedAt: currentSpecification.createdAt,
                UpdatedAt: currentSpecification.updatedAt
            } : null
        };

        console.log('📤 Request data:', requestData);

        // Append JSON
        formData.append('jsonData', JSON.stringify(requestData));

        // Append image
        const fileInput = document.getElementById('owner-image-upload');
        if (fileInput && fileInput.files.length > 0) {
            formData.append('ProfilePicture', fileInput.files[0]);
            console.log('📷 Image attached:', fileInput.files[0].name);
        }

        // Gửi request
        const response = await fetch('/api/receive-profile/update', {
            method: 'POST',
            body: formData
        });

        console.log('📊 Response status:', response.status);

        const contentType = response.headers.get('content-type');
        if (!contentType || !contentType.includes('application/json')) {
            const text = await response.text();
            console.error('❌ Server response (not JSON):', text);
            showNotification('error', 'Lỗi: Server không trả về JSON');
            return;
        }

        const data = await response.json();
        console.log('📊 Response data:', data);

        if (data.success) {
            showNotification('success', data.message);

            // Cập nhật ảnh nếu có
            if (data.imageUrl) {
                const imgContainer = document.getElementById('owner-image-container');
                if (imgContainer) {
                    imgContainer.innerHTML = `<img src="${data.imageUrl}?t=${new Date().getTime()}" alt="Owner Image" class="owner-image">`;
                }
            }

            // ✅ TỰ ĐỘNG SEARCH LẠI ĐỂ LẤY DỮ LIỆU MỚI TỪ DATABASE
            console.log('🔄 Refreshing data from database...');

            // Lưu lại CCCD hoặc PlateNo để search
            const searchCCCD = currentOwner.cccd || '';
            const searchPlate = currentVehicle.plateNo || '';

            // Gọi API search lại
            try {
                const searchUrl = `/api/receive-profile/search?cccd=${encodeURIComponent(searchCCCD)}&plateNo=${encodeURIComponent(searchPlate)}`;
                const searchResponse = await fetch(searchUrl, { method: 'GET' });
                const searchData = await searchResponse.json();

                if (searchData.success) {
                    // Cập nhật lại global variables
                    currentOwner = searchData.data.owner;
                    currentVehicle = searchData.data.vehicle;
                    currentSpecification = searchData.data.specification;

                    // Populate lại form với dữ liệu mới
                    populateForm(searchData.data);

                    console.log('✅ Data refreshed successfully');
                    console.log('✅ New Province:', currentOwner.province);
                    console.log('✅ New Ward:', currentOwner.ward);
                }
            } catch (refreshError) {
                console.error('⚠️ Failed to refresh data:', refreshError);
                // Không hiện lỗi cho user vì save đã thành công
            }

        } else {
            console.error('❌ Save failed:', data);
            showNotification('error', data.message);
        }

        console.log('💾 ========== KẾT THÚC LƯU ==========');

    } catch (error) {
        console.error('❌ Save error:', error);
        showNotification('error', 'Có lỗi xảy ra: ' + error.message);
    }
}

// ========== LOAD PROVINCES ==========
async function loadProvinces() {
    try {
        console.log('🔍 Loading provinces...');

        const response = await fetch('/api/receive-profile/provinces');

        console.log('📊 Response status:', response.status);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        console.log('📊 Provinces data:', data);

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
        } else {
            console.error('❌ Invalid provinces data:', data);
            showNotification('error', 'Không thể tải danh sách tỉnh/thành phố');
        }
    } catch (error) {
        console.error('❌ Load provinces error:', error);
        showNotification('error', 'Không thể kết nối đến server');
    }
}
// ========== LOAD WARDS ==========
// ========== LOAD WARDS ==========
async function loadWards(provinceName) {
    try {
        if (!provinceName) {
            console.log('⚠️ Province name is empty');
            return;
        }

        console.log('🔍 Loading wards for:', provinceName);

        const response = await fetch(`/api/receive-profile/wards?province=${encodeURIComponent(provinceName)}`);

        console.log('📊 Response status:', response.status);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        console.log('📊 Wards data:', data);

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
            } else {
                console.log('⚠️ No wards found');
            }
        }
    } catch (error) {
        console.error('❌ Load wards error:', error);
        showNotification('error', 'Không thể tải danh sách phường/xã');
    }
}

// ========== TOGGLE OWNER TYPE ==========
function toggleOwnerType() {
    const ownerType = document.getElementById('owner-type')?.value;
    const personInfo = document.getElementById('person-info');
    const companyInfo = document.getElementById('company-info');

    if (ownerType === 'PERSON') {
        if (personInfo) personInfo.style.display = 'flex';
        if (companyInfo) companyInfo.style.display = 'none';
    } else {
        if (personInfo) personInfo.style.display = 'none';
        if (companyInfo) companyInfo.style.display = 'flex';
    }
}

// ========== PREVIEW OWNER IMAGE ==========
function previewOwnerImage(event) {
    if (event.target.files && event.target.files[0]) {
        const reader = new FileReader();
        reader.onload = function (e) {
            const imgContainer = document.getElementById('owner-image-container');
            if (imgContainer) {
                imgContainer.innerHTML = `<img src="${e.target.result}" alt="Owner Image" class="owner-image">`;
            }
        };
        reader.readAsDataURL(event.target.files[0]);
    }
}

// ========== CLEAR SEARCH ==========
function clearSearch() {
    document.getElementById('search-cccd').value = '';
    document.getElementById('search-plate').value = '';

    // Reset display
    document.getElementById('no-data-state').style.display = 'flex';
    document.getElementById('data-display').style.display = 'none';

    // Clear global variables
    currentOwner = null;
    currentVehicle = null;
    currentSpecification = null;
}

// ========== CANCEL CHANGES ==========
function cancelChanges() {
    if (confirm('Bạn có chắc muốn hủy các thay đổi?')) {
        if (currentOwner && currentVehicle) {
            // Reload lại dữ liệu ban đầu
            populateForm({
                owner: currentOwner,
                vehicle: currentVehicle,
                specification: currentSpecification
            });
            showNotification('info', 'Đã hủy thay đổi');
        }
    }
}

// ========== SHOW NOTIFICATION ==========
function showNotification(type, message) {
    const container = document.getElementById('notification-container');
    if (!container) return;

    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;

    const icon = type === 'success' ? 'check-circle' :
        type === 'error' ? 'x-circle' : 'info-circle';

    notification.innerHTML = `
        <i class="bi bi-${icon}"></i>
        <span>${message}</span>
    `;

    container.appendChild(notification);

    // Auto remove after 5 seconds
    setTimeout(() => {
        notification.style.opacity = '0';
        setTimeout(() => notification.remove(), 300);
    }, 5000);
}

// ========== EVENT LISTENERS ==========
document.addEventListener('DOMContentLoaded', function () {
    // Load provinces khi trang load
    loadProvinces();

    // Load wards khi chọn province
    const provinceSelect = document.getElementById('owner-province');
    if (provinceSelect) {
        provinceSelect.addEventListener('change', function () {
            if (this.value) {
                loadWards(this.value);
            } else {
                const wardSelect = document.getElementById('owner-ward');
                if (wardSelect) {
                    wardSelect.innerHTML = '<option value="">-- Chọn Phường/Xã --</option>';
                    wardSelect.disabled = true;
                }
            }
        });
    }
});