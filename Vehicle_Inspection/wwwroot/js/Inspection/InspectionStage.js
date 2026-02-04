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
    return dbStages.map(stage => {
        // ✅ ĐẾM SỐ ITEM CÓ THRESHOLD (applicable)
        const applicableItemsCount = stage.items.filter(item => item.hasThreshold).length;
        const hasApplicableItems = applicableItemsCount > 0;

        return {
            stageId: stage.stageId,
            inspStageId: stage.inspStageId,
            stageName: stage.stageName,
            status: stage.status || 0,
            result: stage.stageResult,
            assignedUser: stage.assignedUserName,
            measurements: convertItemsToMeasurements(stage.items),
            items: stage.items,
            sortOrder: stage.sortOrder,
            // ✅ THÊM FLAG
            hasApplicableItems: hasApplicableItems,
            applicableItemsCount: applicableItemsCount,
            totalItemsCount: stage.items.length
        };
    });
}

/**
 * Convert stage items thành measurements object
 */
function convertItemsToMeasurements(items) {
    const measurements = {};
    items.forEach(item => {
        // ✅ ƯU TIÊN actualText (cho AllowedValues), sau đó mới đến actualValue
        if (item.actualText !== null && item.actualText !== undefined) {
            measurements[item.itemId] = item.actualText;
        } else if (item.actualValue !== null && item.actualValue !== undefined) {
            measurements[item.itemId] = item.actualValue;
        }
    });
    return measurements;
}

/**
 * Build stage items config cho form render
 */
function buildStageItemsConfig(stages) {
    console.log('Building stage items config...', stages);

    const stageItems = {};

    stages.forEach(stage => {
        if (!stage.items || stage.items.length === 0) {
            console.warn(`Stage ${stage.stageId} has no items`);
            stageItems[stage.stageId] = [];
            return;
        }

        stageItems[stage.stageId] = stage.items.map(item => {
            const config = {
                id: item.itemId,
                itemCode: item.itemCode,
                name: item.itemName,
                unit: item.unit,
                type: getInputType(item.dataType),
                standard: getStandardText(item),
                isRequired: item.isRequired,
                hasThreshold: item.hasThreshold // ✅ THÊM FLAG
            };

            // ✅ NẾU KHÔNG CÓ THRESHOLD → DISABLE
            if (!item.hasThreshold) {
                config.disabled = true;
                config.standard = 'Không áp dụng';
                console.warn(`⚠️ Item ${item.itemName} (ID: ${item.itemId}) has no threshold → disabled`);
            }

            // ✅ NẾU CÓ AllowedValues → Dùng SELECT
            if (item.allowedValues && item.hasThreshold) {
                config.type = 'select';
                config.options = item.allowedValues.split(';').map(v => v.trim()).filter(v => v);
                config.standard = config.options[0] || 'Đạt';
            }
            // ✅ NẾU KHÔNG CÓ AllowedValues → Dùng NUMBER INPUT
            else if (config.type === 'number' && item.hasThreshold) {
                config.min = item.minValue !== null && item.minValue !== undefined ? item.minValue : 0;
                config.max = item.maxValue !== null && item.maxValue !== undefined ? item.maxValue : 999999;
                config.standard = getStandardText(item);
            }
            // ✅ FALLBACK: Nếu là BOOL nhưng không có AllowedValues
            else if (item.hasThreshold) {
                config.type = 'select';
                config.options = ['Đạt', 'Không đạt'];
                config.standard = 'Đạt';
            }

            return config;
        });

        console.log(`Stage ${stage.stageId} (${stage.stageName}) has ${stageItems[stage.stageId].length} items`);
    });

    console.log('✅ Built stage items config:', stageItems);
    return stageItems;
}

function getInputType(dataType) {
    if (!dataType) return 'select';

    const type = dataType.toUpperCase();

    if (type === 'NUMBER') {
        return 'number';
    } else if (type === 'BOOL' || type === 'BOOLEAN') {
        return 'select';
    } else {
        return 'select';
    }
}

/**
 * Get standard text cho item
 */
function getStandardText(item) {
    if (item.passCondition && item.minValue != null && item.maxValue != null) {
        //return item.passCondition;
        return `${item.minValue} - ${item.maxValue}`;
    }

    if (item.minValue !== null && item.maxValue !== null) {
        return `${item.minValue} - ${item.maxValue}`;
    }

    if (item.minValue !== null && item.maxValue == null) {
        return `≥ ${item.minValue}`;
    }

    if (item.maxValue !== null && item.minValue == null) {
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