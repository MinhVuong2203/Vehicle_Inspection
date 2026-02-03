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

                // Tổng hợp kết quả các công đoạn
                var stagesInfo = inspection.InspectionStages
                    .OrderBy(s => s.SortOrder)
                    .Select(stage => new
                    {
                        stageId = stage.StageId,
                        stageName = stage.Stage?.StageName,
                        status = stage.Status,
                        stageResult = stage.StageResult,
                        isPassed = stage.StageResult == 1, // 1 = Đạt, 2 = Không đạt
                        defects = stage.InspectionDefects.Select(d => new
                        {
                            defectDescription = d.DefectDescription,
                            defectCategory = d.DefectCategory,
                            severity = d.Severity,
                            defectCode = d.DefectCode
                        }).ToList()
                    }).ToList();

                // Kiểm tra tất cả công đoạn đã đạt chưa
                bool allPassed = stagesInfo.All(s => s.isPassed);
                var failedStages = stagesInfo.Where(s => !s.isPassed).ToList();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        inspectionId = inspection.InspectionId,
                        inspectionCode = inspection.InspectionCode,
                        vehicleInfo = new
                        {
                            plateNo = inspection.Vehicle?.PlateNo,
                            brand = inspection.Vehicle?.Brand,
                            model = inspection.Vehicle?.Model
                        },
                        allPassed = allPassed,
                        stages = stagesInfo,
                        failedStages = failedStages,
                        totalDefects = stagesInfo.Sum(s => s.defects.Count)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi khi lấy chi tiết kiểm định",
                    error = ex.Message
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
        /// API: Kết luận KHÔNG ĐẠT - Chuyển Status từ 4 → 6 và trả về download link
        /// ✅ ĐÃ BỎ LOGIC TĂNG Count_Re (Count_Re chỉ được cập nhật ở ApproveController khi xét duyệt lại)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ConcludeFailed([FromBody] ConcludeRequest request)
        {
            try
            {
                var inspection = await _context.Inspections
                    .Include(i => i.Vehicle)
                    .Include(i => i.InspectionStages)
                        .ThenInclude(s => s.Stage)
                    .Include(i => i.InspectionStages)
                        .ThenInclude(s => s.InspectionDefects)
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

                // ✅ BỎ LOGIC TĂNG Count_Re
                // Count_Re chỉ được cập nhật ở ApproveController khi xét duyệt lại hồ sơ không đạt

                // Cập nhật Status = 6 (Không đạt)
                inspection.Status = 6;
                inspection.FinalResult = null; // ✅ Đặt về NULL khi không đạt (để phân biệt với đạt = 1)
                inspection.ConclusionNote = request.Notes;
                inspection.ConcludedBy = request.UserId;
                inspection.ConcludedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Kết luận KHÔNG ĐẠT cho hồ sơ {inspection.InspectionCode}, Count_Re hiện tại = {inspection.Count_Re}");

                // Tạo PDF báo cáo lỗi
                var pdfResult = await GenerateFailedInspectionPDF(inspection);

                if (!pdfResult.Success)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Kết luận thành công nhưng có lỗi khi tạo PDF",
                        error = pdfResult.ErrorMessage
                    });
                }

                // Tạo URL download
                string downloadUrl = $"/downloads/{pdfResult.FileName}";

                return Ok(new
                {
                    success = true,
                    message = "Kết luận KHÔNG ĐẠT thành công",
                    data = new
                    {
                        inspectionId = inspection.InspectionId,
                        inspectionCode = inspection.InspectionCode,
                        status = inspection.Status,
                        countRe = inspection.Count_Re, // Trả về Count_Re hiện tại (không thay đổi)
                        pdfFileName = pdfResult.FileName,
                        downloadUrl = downloadUrl
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ LỖI ConcludeFailed: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi khi kết luận",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Tạo file PDF báo cáo lỗi cho hồ sơ không đạt
        /// ✅ SỬA LỖI FONT TIẾNG VIỆT
        /// ✅ BỎ MÃ CHỈ TIÊU (DefectCode)
        /// </summary>
        private async Task<PdfGenerationResult> GenerateFailedInspectionPDF(Inspection inspection)
        {
            var result = new PdfGenerationResult();
            Document pdfDoc = null;
            PdfWriter writer = null;
            FileStream stream = null;

            try
            {
                Console.WriteLine($"🔧 Bắt đầu tạo PDF cho hồ sơ: {inspection.InspectionCode}");

                // Tạo thư mục downloads nếu chưa có
                string downloadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "downloads");
                if (!Directory.Exists(downloadsPath))
                {
                    Directory.CreateDirectory(downloadsPath);
                    Console.WriteLine($"✅ Đã tạo thư mục: {downloadsPath}");
                }

                // Tên file PDF
                string fileName = $"KHONG_DAT_{inspection.InspectionCode}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string filePath = Path.Combine(downloadsPath, fileName);
                Console.WriteLine($"📄 Đường dẫn file: {filePath}");

                // ✅ SỬA LỖI FONT: Kiểm tra và load font tiếng Việt
                string fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "Arial.ttf");
                bool useCustomFont = System.IO.File.Exists(fontPath);

                if (!useCustomFont)
                {
                    Console.WriteLine($"⚠️ Không tìm thấy font tại: {fontPath}");
                    Console.WriteLine($"⚠️ Sẽ sử dụng font mặc định (có thể không hiển thị tiếng Việt)");
                }
                else
                {
                    Console.WriteLine($"✅ Tìm thấy font tiếng Việt: {fontPath}");
                }

                // Tạo PDF
                stream = new FileStream(filePath, FileMode.Create);
                pdfDoc = new Document(PageSize.A4, 25, 25, 30, 30);
                writer = PdfWriter.GetInstance(pdfDoc, stream);

                pdfDoc.Open();
                Console.WriteLine($"✅ Đã mở PDF document");

                // ✅ SỬA LỖI FONT: Load font tiếng Việt
                Font titleFont, headerFont, normalFont;

                if (useCustomFont)
                {
                    try
                    {
                        // Sử dụng font tiếng Việt với encoding Unicode
                        BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                        titleFont = new Font(bf, 16, Font.BOLD);
                        headerFont = new Font(bf, 12, Font.BOLD);
                        normalFont = new Font(bf, 10, Font.NORMAL);
                        Console.WriteLine($"✅ Đã load font tiếng Việt thành công");
                    }
                    catch (Exception fontEx)
                    {
                        Console.WriteLine($"⚠️ Lỗi load font tiếng Việt: {fontEx.Message}, sử dụng font mặc định");
                        titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                        headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                        normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                    }
                }
                else
                {
                    // Sử dụng font mặc định
                    titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                    headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                    normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                    Console.WriteLine($"ℹ️ Sử dụng font mặc định");
                }

                // Tiêu đề
                Paragraph title = new Paragraph("BIÊN BẢN KIỂM ĐỊNH - KHÔNG ĐẠT", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20;
                pdfDoc.Add(title);

                // Thông tin hồ sơ
                pdfDoc.Add(new Paragraph($"Mã hồ sơ: {inspection.InspectionCode ?? "N/A"}", normalFont));
                pdfDoc.Add(new Paragraph($"Biển số xe: {inspection.Vehicle?.PlateNo ?? "N/A"}", normalFont));
                pdfDoc.Add(new Paragraph($"Loại xe: {inspection.Vehicle?.Brand ?? ""} {inspection.Vehicle?.Model ?? ""}", normalFont));
                pdfDoc.Add(new Paragraph($"Ngày kết luận: {DateTime.Now:dd/MM/yyyy HH:mm}", normalFont));

                // ✅ THÊM THÔNG TIN SỐ LẦN TÁI KIỂM (nếu có)
                if (inspection.Count_Re != null && inspection.Count_Re > 0)
                {
                    pdfDoc.Add(new Paragraph($"Số lần tái kiểm: {inspection.Count_Re}", normalFont));
                }

                pdfDoc.Add(new Paragraph(" ", normalFont)); // Khoảng trắng

                // Danh sách lỗi
                pdfDoc.Add(new Paragraph("DANH SÁCH LỖI PHÁT HIỆN:", headerFont));
                pdfDoc.Add(new Paragraph(" ", normalFont));

                // Lặp qua các công đoạn không đạt
                var failedStages = inspection.InspectionStages
                    .Where(s => s.StageResult == 2) // 2 = Không đạt
                    .OrderBy(s => s.SortOrder)
                    .ToList();

                Console.WriteLine($"📊 Số công đoạn không đạt: {failedStages.Count}");

                if (!failedStages.Any())
                {
                    pdfDoc.Add(new Paragraph("Không có công đoạn nào không đạt", normalFont));
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

                                // ✅ BỎ MÃ CHỈ TIÊU (DefectCode) - CHỈ HIỂN THỊ MÔ TẢ VÀ MỨC ĐỘ
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

                        pdfDoc.Add(new Paragraph(" ", normalFont)); // Khoảng trắng
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