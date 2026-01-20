// ============================================
// MODULE: INSPECTION STAGES
// Load và quản lý các bước kiểm định từ database
// ============================================

// Biến global cho stages
let loadedStagesData = [];
let currentInspectionId = null;

/**
 * Load các bước kiểm định từ database theo InspectionId
 */
async function loadInspectionStagesFromDB(inspectionId) {
    try {
        console.log('=== Loading Inspection Stages from DB ===');
        console.log('InspectionId:', inspectionId);

        const response = await fetch(`/Inspection/GetInspectionStages?inspectionId=${inspectionId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            }
        });

        console.log('Response status:', response.status);

        if (!response.ok) {
            throw new Error('Không thể tải dữ liệu bước kiểm định');
        }

        const result = await response.json();
        console.log('API Response:', result);

        if (result.success && result.data) {
            loadedStagesData = result.data;
            console.log(`Loaded ${loadedStagesData.length} stages from database`);
            return loadedStagesData;
        } else {
            throw new Error(result.message || 'Không tìm thấy dữ liệu bước kiểm định');
        }
    } catch (error) {
        console.error('Error loading inspection stages:', error);
        alert('Không thể tải dữ liệu bước kiểm định. Vui lòng thử lại.');
        return [];
    }
}

/**
 * Convert loaded stages từ DB sang format của UI
 */
function convertStagesToUIFormat(dbStages) {
    return dbStages.map(stage => ({
        stageId: stage.stageId,
        stageName: stage.stageName,
        status: stage.status || 0,
        result: stage.stageResult,
        assignedUser: stage.assignedUserName,
        measurements: convertItemsToMeasurements(stage.items),
        items: stage.items, // Giữ nguyên để dùng sau
        sortOrder: stage.sortOrder
    }));
}

/**
 * Convert stage items thành measurements object
 */
function convertItemsToMeasurements(items) {
    const measurements = {};
    items.forEach(item => {
        if (item.actualValue !== null || item.actualText !== null) {
            measurements[item.itemId] = item.actualText || item.actualValue;
        }
    });
    return measurements;
}

/**
 * Build stage items config cho form render
 */
function buildStageItemsConfig(stages) {
    const stageItems = {};

    stages.forEach(stage => {
        stageItems[stage.stageId] = stage.items.map(item => {
            const config = {
                id: item.itemId,
                name: item.itemName,
                type: item.dataType.toLowerCase() === 'number' ? 'number' : 'select',
                standard: getStandardText(item)
            };

            // Nếu là select, build options
            if (config.type === 'select') {
                if (item.allowedValues) {
                    config.options = item.allowedValues.split(';').map(v => v.trim());
                    config.standard = config.options[0]; // Default first option
                } else {
                    config.options = ['Đạt', 'Không đạt'];
                    config.standard = 'Đạt';
                }
            }

            // Nếu là number, set min/max
            if (config.type === 'number') {
                config.min = item.minValue || 0;
                config.max = item.maxValue || 999999;
            }

            return config;
        });
    });

    return stageItems;
}

/**
 * Get standard text cho item
 */
function getStandardText(item) {
    if (item.passCondition) {
        return item.passCondition;
    }

    if (item.minValue !== null && item.maxValue !== null) {
        return `${item.minValue} - ${item.maxValue}`;
    }

    if (item.minValue !== null) {
        return `≥ ${item.minValue}`;
    }

    if (item.maxValue !== null) {
        return `≤ ${item.maxValue}`;
    }

    return 'Theo quy định';
}

/**
 * Export function để sử dụng trong Inspection.js
 */
window.InspectionStageLoader = {
    loadStages: loadInspectionStagesFromDB,
    convertToUIFormat: convertStagesToUIFormat,
    buildItemsConfig: buildStageItemsConfig
};