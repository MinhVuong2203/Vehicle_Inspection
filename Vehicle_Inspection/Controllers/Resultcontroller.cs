using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Xml.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vehicle_Inspection.Data;
using Vehicle_Inspection.Models;
using static System.Net.Mime.MediaTypeNames;
using Document = iTextSharp.text.Document;
using Font = iTextSharp.text.Font;

namespace Vehicle_Inspection.Controllers
{
    public class Resultcontroller : Controller
    {
        private readonly VehInsContext _context;

        public Resultcontroller(VehInsContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Hiển thị trang Kết luận - Danh sách hồ sơ có Status = 4 (Hoàn thành kiểm định)
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                // Lấy danh sách hồ sơ có Status = 4 (Hoàn thành kiểm định, chưa kết luận)
                var inspections = await _context.Inspections
                    .Where(i => i.Status == 4 && i.IsDeleted == false)
                    .Include(i => i.Vehicle)
                    .Include(i => i.Lane)
                    .Include(i => i.InspectionStages)
                        .ThenInclude(s => s.Stage)
                    .Include(i => i.InspectionStages)
                        .ThenInclude(s => s.InspectionDefects)
                    .OrderByDescending(i => i.CompletedAt)
                    .ToListAsync();

                // ========================================================================
                // ✅ TÍNH TOÁN AllPassed CHO MỖI INSPECTION
                // ========================================================================
                foreach (var inspection in inspections)
                {
                    var stageIds = inspection.InspectionStages.Select(s => s.StageId).ToList();

                    // Đếm số StageItem của từng stage
                    var stageItemCountDict = await _context.StageItems
                        .Where(si => stageIds.Contains(si.StageId))
                        .GroupBy(si => si.StageId)
                        .Select(g => new { StageId = g.Key, Count = g.Count() })
                        .ToDictionaryAsync(x => x.StageId, x => x.Count);

                    // Kiểm tra tất cả stages
                    bool allPassed = true;
                    foreach (var stage in inspection.InspectionStages)
                    {
                        int itemCount = stageItemCountDict.ContainsKey(stage.StageId)
                            ? stageItemCountDict[stage.StageId]
                            : 0;

                        // Xác định isPassed cho stage này
                        bool stagePassed;
                        if (stage.StageResult == null)
                        {
                            // Chưa kiểm tra → Coi là Đạt
                            stagePassed = true;
                        }
                        else if (itemCount == 0)
                        {
                            // Không có item → Coi là Đạt
                            stagePassed = true;
                        }
                        else
                        {
                            // Có item → Xét theo StageResult
                            stagePassed = (stage.StageResult == 1);
                        }

                        if (!stagePassed)
                        {
                            allPassed = false;
                            break;
                        }
                    }

                    // Lưu kết quả vào ViewBag để View sử dụng
                    ViewData[$"AllPassed_{inspection.InspectionId}"] = allPassed;
                }

                return View(inspections);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi khi tải danh sách: {ex.Message}";
                return View(new List<Inspection>());
            }
        }

        /// <summary>
        /// API: Lấy chi tiết kết quả kiểm định của một hồ sơ
        /// ✅ LOGIC MỚI: Override StageResult nếu công đoạn không có StageItem
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetInspectionDetails(int inspectionId)
        {
            try
            {
                var inspection = await _context.Inspections
                    .Include(i => i.Vehicle)
                    .Include(i => i.InspectionStages)
                        .ThenInclude(s => s.Stage)
                    .Include(i => i.InspectionStages)
                        .ThenInclude(s => s.InspectionDefects)
                    .FirstOrDefaultAsync(i => i.InspectionId == inspectionId);

                if (inspection == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy hồ sơ" });
                }

                // ========================================================================
                // ✅ BƯỚC 1: ĐẾM SỐ LƯỢNG STAGEITEM CỦA TỪNG STAGE
                // ========================================================================
                var stageIds = inspection.InspectionStages.Select(s => s.StageId).ToList();

                // Dictionary: StageId -> Số lượng StageItem
                var stageItemCountDict = await _context.StageItems
                    .Where(si => stageIds.Contains(si.StageId))
                    .GroupBy(si => si.StageId)
                    .Select(g => new { StageId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.StageId, x => x.Count);

                // ========================================================================
                // ✅ BƯỚC 2: XỬ LÝ KẾT QUẢ TỪNG STAGE
                // ========================================================================
                var stagesInfo = inspection.InspectionStages
                    .OrderBy(s => s.SortOrder)
                    .Select(stage =>
                    {
                        // Lấy số lượng StageItem của Stage này
                        int itemCount = stageItemCountDict.ContainsKey(stage.StageId)
                            ? stageItemCountDict[stage.StageId]
                            : 0;

                        // ✅ QUAN TRỌNG: XÁC ĐỊNH KẾT QUẢ THỰC TẾ
                        bool isPassed;
                        bool shouldDisplay = true; // Flag để ẩn/hiện stage

                        if (stage.StageResult == null)
                        {
                            // ✅ StageResult = null → Chưa được kiểm tra
                            // → ẨN công đoạn này (không hiển thị)
                            // → Nhưng vẫn coi là ĐẠT (cho phép xét tổng kết là Đạt)
                            isPassed = true;
                            shouldDisplay = false; // ẨN ĐI
                        }
                        else if (itemCount == 0)
                        {
                            // ✅ Stage KHÔNG có StageItem → BỎ QUA StageResult trong DB
                            // → Luôn coi là ĐẠT
                            isPassed = true;
                            shouldDisplay = true; // Vẫn hiển thị với badge "Đạt"
                        }
                        else
                        {
                            // ❌ Stage CÓ StageItem + CÓ kết quả → Xét theo StageResult trong DB
                            isPassed = (stage.StageResult == 1); // 1 = Đạt, 2 = Không đạt
                            shouldDisplay = true;
                        }

                        return new
                        {
                            stageId = stage.StageId,
                            stageName = stage.Stage?.StageName ?? "N/A",
                            isPassed = isPassed,
                            shouldDisplay = shouldDisplay,
                            itemCount = itemCount,
                            defects = stage.InspectionDefects?.Select(d => new
                            {
                                defectId = d.DefectId,
                                defectDescription = d.DefectDescription,
                                severity = d.Severity,
                                severityText = d.Severity switch
                                {
                                    1 => "Khuyết điểm",
                                    2 => "Hư hỏng",
                                    3 => "Nguy hiểm",
                                    _ => "Không xác định"
                                },
                                category = d.DefectCategory
                            }).ToList()
                        };
                    })
                    .Where(s => s.shouldDisplay) // ✅ CHỈ TRẢ VỀ CÁC STAGE CẦN HIỂN THỊ
                    .ToList();

                // ========================================================================
                // ✅ BƯỚC 3: KIỂM TRA TỔNG KẾT TOÀN BỘ
                // ========================================================================
                bool allPassed = inspection.InspectionStages.All(stage =>
                {
                    if (stage.StageResult == null) return true;

                    int itemCount = stageItemCountDict.ContainsKey(stage.StageId)
                        ? stageItemCountDict[stage.StageId]
                        : 0;

                    if (itemCount == 0) return true;

                    return stage.StageResult == 1;
                });

                Console.WriteLine($"📊 Tổng kết: {(allPassed ? "ĐẠT" : "KHÔNG ĐẠT")}");

                // ========================================================================
                // ✅ BƯỚC 4: TRẢ VỀ KẾT QUẢ
                // ========================================================================
                return Json(new
                {
                    success = true,
                    inspectionCode = inspection.InspectionCode,
                    plateNo = inspection.Vehicle?.PlateNo ?? "N/A",
                    vehicleInfo = $"{inspection.Vehicle?.Brand ?? ""} {inspection.Vehicle?.Model ?? ""}",
                    allPassed = allPassed, // ✅ true hoặc false
                    stages = stagesInfo,
                    countRe = inspection.Count_Re
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ LỖI: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi khi lấy thông tin hồ sơ"
                });
            }
        }

        /// <summary>
        /// API: Kết luận ĐẠT - Chuyển Status từ 4 → 5
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ConcludePassed([FromBody] ConcludeRequest request)
        {
            try
            {
                var inspection = await _context.Inspections
                    .FirstOrDefaultAsync(i => i.InspectionId == request.InspectionId);

                if (inspection == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy hồ sơ" });
                }

                if (inspection.Status != 4)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Hồ sơ không ở trạng thái chờ kết luận"
                    });
                }

                // Cập nhật Status = 5 (Đạt)
                inspection.Status = 5;
                inspection.FinalResult = 1; // 1 = Đạt
                inspection.ConclusionNote = request.Notes;
                inspection.ConcludedBy = request.UserId;
                inspection.ConcludedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Kết luận ĐẠT thành công",
                    data = new
                    {
                        inspectionId = inspection.InspectionId,
                        inspectionCode = inspection.InspectionCode,
                        status = inspection.Status
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi khi kết luận",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// API: Kết luận KHÔNG ĐẠT - Chuyển Status từ 4 → 6 và tạo PDF
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ConcludeFailed([FromBody] ConcludeRequest request)
        {
            try
            {
                Console.WriteLine($"🔴 Bắt đầu kết luận KHÔNG ĐẠT cho InspectionId: {request.InspectionId}");

                var inspection = await _context.Inspections
                    .Include(i => i.Vehicle)
                    .Include(i => i.InspectionStages)
                        .ThenInclude(s => s.Stage)
                    .Include(i => i.InspectionStages)
                        .ThenInclude(s => s.InspectionDefects)
                    .FirstOrDefaultAsync(i => i.InspectionId == request.InspectionId);

                if (inspection == null)
                {
                    Console.WriteLine($"❌ Không tìm thấy hồ sơ InspectionId: {request.InspectionId}");
                    return NotFound(new { success = false, message = "Không tìm thấy hồ sơ" });
                }

                if (inspection.Status != 4)
                {
                    Console.WriteLine($"⚠️ Hồ sơ không ở trạng thái chờ kết luận. Status hiện tại: {inspection.Status}");
                    return BadRequest(new
                    {
                        success = false,
                        message = "Hồ sơ không ở trạng thái chờ kết luận"
                    });
                }

                // Cập nhật Status = 6 (Không đạt)
                inspection.Status = 6;
                inspection.FinalResult = 2; // 2 = Không đạt
                inspection.ConclusionNote = request.Notes;
                inspection.ConcludedBy = request.UserId;
                inspection.ConcludedAt = DateTime.Now;

                Console.WriteLine($"✅ Đã cập nhật Status = 6 (Không đạt) cho hồ sơ {inspection.InspectionCode}");

                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ Đã lưu database");

                // Tạo file PDF biên bản không đạt
                Console.WriteLine($"📄 Bắt đầu tạo file PDF cho hồ sơ {inspection.InspectionCode}...");
                var pdfResult = await GeneratePdfReport(inspection);

                if (!pdfResult.Success)
                {
                    Console.WriteLine($"❌ Tạo PDF thất bại: {pdfResult.ErrorMessage}");
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Kết luận không đạt thành công nhưng không thể tạo file PDF",
                        error = pdfResult.ErrorMessage
                    });
                }

                Console.WriteLine($"✅ Tạo PDF thành công: {pdfResult.FileName}");

                // Trả về kết quả kèm link download
                return Ok(new
                {
                    success = true,
                    message = "Kết luận KHÔNG ĐẠT thành công",
                    data = new
                    {
                        inspectionId = inspection.InspectionId,
                        inspectionCode = inspection.InspectionCode,
                        status = inspection.Status,
                        pdfFileName = pdfResult.FileName,
                        downloadUrl = $"/Reports/{pdfResult.FileName}"
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ LỖI NGHIÊM TRỌNG khi kết luận không đạt: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi khi kết luận",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// API: Kết luận hồ sơ (Đạt hoặc Không đạt)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ConcludeInspection([FromBody] ConcludeRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                Console.WriteLine($"🎯 Bắt đầu kết luận hồ sơ ID: {request.InspectionId}");

                // Kiểm tra hồ sơ tồn tại
                var inspection = await _context.Inspections
                    .Include(i => i.InspectionStages)
                    .FirstOrDefaultAsync(i => i.InspectionId == request.InspectionId);

                if (inspection == null)
                {
                    Console.WriteLine($"❌ Không tìm thấy hồ sơ ID: {request.InspectionId}");
                    return NotFound(new { success = false, message = "Không tìm thấy hồ sơ" });
                }

                // Kiểm tra trạng thái hợp lệ
                if (inspection.Status != 4)
                {
                    Console.WriteLine($"❌ Hồ sơ ID {request.InspectionId} có Status không hợp lệ: {inspection.Status}");
                    return BadRequest(new { success = false, message = "Hồ sơ chưa hoàn thành kiểm định" });
                }

                // ========================================================================
                // ✅ TÍNH TOÁN allPassed
                // ========================================================================
                var stageIds = inspection.InspectionStages.Select(s => s.StageId).ToList();
                var stageItemCountDict = await _context.StageItems
                    .Where(si => stageIds.Contains(si.StageId))
                    .GroupBy(si => si.StageId)
                    .Select(g => new { StageId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.StageId, x => x.Count);

                bool allPassed = inspection.InspectionStages.All(stage =>
                {
                    if (stage.StageResult == null) return true;

                    int itemCount = stageItemCountDict.ContainsKey(stage.StageId)
                        ? stageItemCountDict[stage.StageId]
                        : 0;

                    if (itemCount == 0) return true;

                    return stage.StageResult == 1;
                });

                Console.WriteLine($"📊 Kết quả tính toán: {(allPassed ? "ĐẠT" : "KHÔNG ĐẠT")}");

                // ========================================================================
                // ✅ CẬP NHẬT HỒ SƠ
                // ========================================================================
                inspection.Status = (short)(allPassed ? 5 : 6); // 5 = Đạt, 6 = Không đạt
                inspection.ConcludedAt = DateTime.Now;
                inspection.ConcludedBy = request.UserId;
                inspection.ConclusionNote = request.Notes;

                Console.WriteLine($"✅ Đặt Status = {inspection.Status} ({(allPassed ? "Đạt" : "Không đạt")})");

                // ========================================================================
                // ✅ TẠO PDF
                // ========================================================================
                var pdfResult = await GeneratePdfReport(inspection);

                if (!pdfResult.Success)
                {
                    Console.WriteLine($"❌ Không thể tạo PDF: {pdfResult.ErrorMessage}");
                    await transaction.RollbackAsync();
                    return StatusCode(500, new { success = false, message = $"Lỗi tạo PDF: {pdfResult.ErrorMessage}" });
                }

                Console.WriteLine($"✅ Tạo PDF thành công: {pdfResult.FilePath}");

                // ========================================================================
                // ✅ LƯU VÀO DB
                // ========================================================================
                _context.Inspections.Update(inspection);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                Console.WriteLine($"✅ Kết luận thành công hồ sơ ID: {request.InspectionId}");

                return Json(new
                {
                    success = true,
                    message = allPassed ? "Kết luận: ĐẠT" : "Kết luận: KHÔNG ĐẠT",
                    status = inspection.Status,
                    pdfFileName = pdfResult.FileName
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ LỖI KẾT LUẬN: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"❌ Inner exception: {ex.InnerException.Message}");
                }

                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Có lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// ✅ HÀM TẠO PDF - SỬ DỤNG TIMES NEW ROMAN VỚI UNICODE HỖ TRỢ TIẾNG VIỆT
        /// </summary>
        private async Task<PdfGenerationResult> GeneratePdfReport(Inspection inspection)
        {
            var result = new PdfGenerationResult();
            Document pdfDoc = null;
            PdfWriter writer = null;
            FileStream stream = null;

            try
            {
                Console.WriteLine($"📝 Bắt đầu tạo PDF cho hồ sơ: {inspection.InspectionCode}");

                // Tạo thư mục Reports nếu chưa tồn tại
                string reportDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Reports");
                if (!Directory.Exists(reportDir))
                {
                    Directory.CreateDirectory(reportDir);
                    Console.WriteLine($"📁 Đã tạo thư mục: {reportDir}");
                }

                // Tạo tên file
                string fileName = $"KetLuan_{inspection.InspectionCode}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string filePath = Path.Combine(reportDir, fileName);

                Console.WriteLine($"📄 Đường dẫn file: {filePath}");

                // Tạo document
                pdfDoc = new Document(PageSize.A4, 50, 50, 50, 50);
                stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                writer = PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();

                Console.WriteLine($"✅ Đã mở PDF document");

                // ========================================================================
                // ✅ TẠO FONT TIMES NEW ROMAN VỚI UNICODE
                // ========================================================================

                // Tìm đường dẫn font Times New Roman trên hệ thống
                string fontPath;

                // Thử các đường dẫn phổ biến cho Times New Roman
                if (System.IO.File.Exists(@"C:\Windows\Fonts\times.ttf"))
                {
                    fontPath = @"C:\Windows\Fonts\times.ttf";
                }
                else if (System.IO.File.Exists("/usr/share/fonts/truetype/liberation/LiberationSerif-Regular.ttf"))
                {
                    // Linux - Liberation Serif (tương tự Times New Roman)
                    fontPath = "/usr/share/fonts/truetype/liberation/LiberationSerif-Regular.ttf";
                }
                else if (System.IO.File.Exists("/System/Library/Fonts/Supplemental/Times New Roman.ttf"))
                {
                    // macOS
                    fontPath = "/System/Library/Fonts/Supplemental/Times New Roman.ttf";
                }
                else
                {
                    // Fallback: sử dụng font hệ thống mặc định
                    Console.WriteLine("⚠️ Không tìm thấy Times New Roman, sử dụng font mặc định");
                    fontPath = null;
                }

                BaseFont baseFont;
                Font headerFont;
                Font normalFont;

                if (fontPath != null && System.IO.File.Exists(fontPath))
                {
                    // Tạo BaseFont với encoding IDENTITY_H (Unicode)
                    baseFont = BaseFont.CreateFont(
                        fontPath,
                        BaseFont.IDENTITY_H,  // Hỗ trợ Unicode đầy đủ
                        BaseFont.EMBEDDED     // Nhúng font vào PDF
                    );

                    headerFont = new Font(baseFont, 14, Font.BOLD);
                    normalFont = new Font(baseFont, 12, Font.NORMAL);

                    Console.WriteLine($"✅ Đã tạo font Times New Roman từ: {fontPath}");
                }
                else
                {
                    // Sử dụng font Helvetica mặc định (có hỗ trợ Unicode hạn chế)
                    baseFont = BaseFont.CreateFont(
                        BaseFont.HELVETICA,
                        BaseFont.IDENTITY_H,
                        BaseFont.EMBEDDED
                    );

                    headerFont = new Font(baseFont, 14, Font.BOLD);
                    normalFont = new Font(baseFont, 12, Font.NORMAL);

                    Console.WriteLine("⚠️ Sử dụng font Helvetica");
                }

                // ========================================================================
                // ✅ NỘI DUNG PDF
                // ========================================================================

                // Tiêu đề
                Paragraph title = new Paragraph("BIÊN BẢN KẾT LUẬN KIỂM ĐỊNH", headerFont);
                title.Alignment = Element.ALIGN_CENTER;
                pdfDoc.Add(title);
                pdfDoc.Add(new Paragraph(" ", normalFont));

                // Thông tin hồ sơ
                pdfDoc.Add(new Paragraph($"Mã hồ sơ: {inspection.InspectionCode ?? "N/A"}", normalFont));
                pdfDoc.Add(new Paragraph($"Biển số xe: {inspection.Vehicle?.PlateNo ?? "N/A"}", normalFont));
                pdfDoc.Add(new Paragraph($"Loại xe: {inspection.Vehicle?.Brand ?? ""} {inspection.Vehicle?.Model ?? ""}", normalFont));
                pdfDoc.Add(new Paragraph($"Ngày kết luận: {DateTime.Now:dd/MM/yyyy HH:mm}", normalFont));

                if (inspection.Count_Re != null && inspection.Count_Re > 0)
                {
                    pdfDoc.Add(new Paragraph($"Số lần tái kiểm: {inspection.Count_Re}", normalFont));
                }

                pdfDoc.Add(new Paragraph(" ", normalFont));

                // Danh sách lỗi
                pdfDoc.Add(new Paragraph("DANH SÁCH LỖI PHÁT HIỆN:", headerFont));
                pdfDoc.Add(new Paragraph(" ", normalFont));

                // ========================================================================
                // ✅ CHỈ LIỆT KÊ STAGE CÓ ITEM VÀ KHÔNG ĐẠT
                // ========================================================================
                var stageIds = inspection.InspectionStages.Select(s => s.StageId).ToList();
                var stageItemCountDict = await _context.StageItems
                    .Where(si => stageIds.Contains(si.StageId))
                    .GroupBy(si => si.StageId)
                    .Select(g => new { StageId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.StageId, x => x.Count);

                // Lọc: Stage phải:
                // 1. CÓ ITEM (itemCount > 0)
                // 2. StageResult = 2 (Không đạt)
                // 3. StageResult KHÔNG NULL (đã được kiểm tra)
                var failedStages = inspection.InspectionStages
                    .Where(s =>
                    {
                        // Bỏ qua stage chưa kiểm tra
                        if (s.StageResult == null) return false;

                        int itemCount = stageItemCountDict.ContainsKey(s.StageId)
                            ? stageItemCountDict[s.StageId]
                            : 0;

                        // ✅ Chỉ lấy stage: CÓ item VÀ không đạt
                        return itemCount > 0 && s.StageResult == 2;
                    })
                    .OrderBy(s => s.SortOrder)
                    .ToList();

                Console.WriteLine($"📊 Số công đoạn THỰC SỰ không đạt (có item): {failedStages.Count}");

                if (!failedStages.Any())
                {
                    pdfDoc.Add(new Paragraph("Không có công đoạn nào không đạt (có chỉ tiêu)", normalFont));
                }
                else
                {
                    int stageNumber = 1;
                    foreach (var stage in failedStages)
                    {
                        pdfDoc.Add(new Paragraph($"{stageNumber}. {stage.Stage?.StageName ?? "N/A"}", headerFont));

                        if (stage.InspectionDefects != null && stage.InspectionDefects.Any())
                        {
                            int defectNumber = 1;
                            foreach (var defect in stage.InspectionDefects)
                            {
                                string severity = "";
                                switch (defect.Severity)
                                {
                                    case 1: severity = "Khuyết điểm"; break;
                                    case 2: severity = "Hư hỏng"; break;
                                    case 3: severity = "Nguy hiểm"; break;
                                }

                                string defectDesc = defect.DefectDescription ?? "Không có mô tả";

                                pdfDoc.Add(new Paragraph(
                                    $"   {stageNumber}.{defectNumber}. {defectDesc} ({severity})",
                                    normalFont
                                ));

                                if (!string.IsNullOrEmpty(defect.DefectCategory))
                                {
                                    pdfDoc.Add(new Paragraph($"        Danh mục: {defect.DefectCategory}", normalFont));
                                }
                                defectNumber++;
                            }
                        }
                        else
                        {
                            pdfDoc.Add(new Paragraph("   Không đạt tiêu chuẩn", normalFont));
                        }

                        pdfDoc.Add(new Paragraph(" ", normalFont));
                        stageNumber++;
                    }
                }

                // Kết luận
                pdfDoc.Add(new Paragraph(" ", normalFont));
                pdfDoc.Add(new Paragraph("KẾT LUẬN:", headerFont));
                pdfDoc.Add(new Paragraph(
                    "Phương tiện KHÔNG ĐẠT tiêu chuẩn an toàn kỹ thuật và bảo vệ môi trường. " +
                    "Cần khắc phục các lỗi trên trước khi đăng kiểm lại.",
                    normalFont
                ));

                if (!string.IsNullOrEmpty(inspection.ConclusionNote))
                {
                    pdfDoc.Add(new Paragraph(" ", normalFont));
                    pdfDoc.Add(new Paragraph($"Ghi chú: {inspection.ConclusionNote}", normalFont));
                }

                Console.WriteLine($"✅ Đã thêm nội dung vào PDF");

                result.Success = true;
                result.FileName = fileName;
                result.FilePath = filePath;

                Console.WriteLine($"✅ Tạo PDF thành công: {fileName}");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ LỖI TẠO PDF: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"❌ Inner exception: {ex.InnerException.Message}");
                }

                result.Success = false;
                result.ErrorMessage = $"{ex.Message} | {ex.InnerException?.Message}";
                return result;
            }
            finally
            {
                // Đóng tất cả resources
                try
                {
                    if (pdfDoc != null && pdfDoc.IsOpen())
                    {
                        pdfDoc.Close();
                        Console.WriteLine($"🔒 Đã đóng PDF document");
                    }
                    if (writer != null)
                    {
                        writer.Close();
                        Console.WriteLine($"🔒 Đã đóng PDF writer");
                    }
                    if (stream != null)
                    {
                        stream.Close();
                        stream.Dispose();
                        Console.WriteLine($"🔒 Đã đóng file stream");
                    }
                }
                catch (Exception closeEx)
                {
                    Console.WriteLine($"⚠️ Lỗi khi đóng resources: {closeEx.Message}");
                }
            }
        }

        // Class để trả về kết quả tạo PDF
        private class PdfGenerationResult
        {
            public bool Success { get; set; }
            public string FileName { get; set; }
            public string FilePath { get; set; }
            public string ErrorMessage { get; set; }
        }
    }

    // DTO cho request kết luận
    public class ConcludeRequest
    {
        public int InspectionId { get; set; }
        public Guid UserId { get; set; }
        public string? Notes { get; set; }
    }
}