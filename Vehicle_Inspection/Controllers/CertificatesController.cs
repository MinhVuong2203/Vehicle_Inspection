using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.IO;
using Vehicle_Inspection.Service;

namespace Vehicle_Inspection.Controllers
{
    public class CertificatesController : Controller
    {
        private readonly ICertificatesService _certificatesService;
        private readonly ICompositeViewEngine _viewEngine;
        private static bool _browserDownloaded = false;

        public CertificatesController(
            ICertificatesService certificatesService,
            ICompositeViewEngine viewEngine)
        {
            _certificatesService = certificatesService;
            _viewEngine = viewEngine;
        }

        public async Task<IActionResult> Index()
        {
            var inspections = await _certificatesService.GetCompletedInspectionsAsync();
            return View(inspections);
        }

        // Xem trước giấy chứng nhận trong trình duyệt
        public async Task<IActionResult> PrintCertificate(int id)
        {
            var inspection = await _certificatesService.GetInspectionForCertificateAsync(id);
            if (inspection == null)
            {
                return NotFound();
            }

            int validityMonths = _certificatesService.CalculateValidityMonths(inspection.Vehicle);
            ViewBag.ValidityMonths = validityMonths;

            return View(inspection);
        }

        // Xuất PDF bằng PuppeteerSharp - Install-Package PuppeteerSharp
        // Xuất PDF bằng PuppeteerSharp - In cả Chứng nhận và Tem
        public async Task<IActionResult> ExportCertificatePdf(int id)
        {
            var inspection = await _certificatesService.GetInspectionForCertificateAsync(id);
            if (inspection == null)
            {
                return NotFound();
            }

            int validityMonths = _certificatesService.CalculateValidityMonths(inspection.Vehicle);
            ViewBag.ValidityMonths = validityMonths;

            IBrowser browser = null;

            try
            {
                // Download browser nếu chưa có (chỉ lần đầu)
                if (!_browserDownloaded)
                {
                    await new BrowserFetcher().DownloadAsync();
                    _browserDownloaded = true;
                }

                // Launch Puppeteer browser
                browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
                });

                var plateNo = inspection.Vehicle?.PlateNo?.Replace(" ", "").Replace("-", "") ?? "Unknown";
                var ownerName = inspection.Vehicle?.Owner?.FullName?.Replace(" ", "_") ?? "Unknown";

                // Tạo thư mục lưu file
                var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "downloads");
                Directory.CreateDirectory(savePath);

                // 1. Xuất PDF Chứng nhận
                var certificateHtml = await RenderViewToStringAsync("PrintCertificate", inspection);
                var certificatePage = await browser.NewPageAsync();
                await certificatePage.SetContentAsync(certificateHtml);

                byte[] certificatePdfBytes = await certificatePage.PdfDataAsync(new PdfOptions
                {
                    Format = PaperFormat.A4,
                    Landscape = true,
                    PrintBackground = true
                });

                var certificateFileName = $"{plateNo}_{ownerName}.pdf";
                var certificateFullPath = Path.Combine(savePath, certificateFileName);
                System.IO.File.WriteAllBytes(certificateFullPath, certificatePdfBytes);

                await certificatePage.CloseAsync();

                // 2. Xuất PDF Tem
                var stampHtml = await RenderViewToStringAsync("temkd", inspection);
                var stampPage = await browser.NewPageAsync();
                await stampPage.SetContentAsync(stampHtml);

                byte[] stampPdfBytes = await stampPage.PdfDataAsync(new PdfOptions
                {
                    Format = PaperFormat.A4,
                    Landscape = false,
                    PrintBackground = true
                });

                var stampFileName = $"Tem_{plateNo}.pdf";
                var stampFullPath = Path.Combine(savePath, stampFileName);
                System.IO.File.WriteAllBytes(stampFullPath, stampPdfBytes);

                await stampPage.CloseAsync();

                // Đóng browser
                await browser.CloseAsync();
                browser = null;

                // Cập nhật status thành 7 sau khi xuất PDF thành công
                await _certificatesService.UpdateInspectionStatusAsync(id, 7);

                // Tạo URL tương đối cho 2 file
                var certificateUrl = $"/downloads/{certificateFileName}";
                var stampUrl = $"/downloads/{stampFileName}";

                // Trả về HTML để mở cả 2 file PDF cùng lúc
                var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Xem PDF</title>
    <style>
        body {{
            margin: 0;
            padding: 0;
            font-family: Arial, sans-serif;
        }}
        .header {{
            background: #333;
            color: white;
            padding: 10px 20px;
            text-align: center;
        }}
        .header a {{
            color: white;
            text-decoration: none;
            margin: 0 10px;
        }}
        .container {{
            display: flex;
            height: calc(100vh - 50px);
        }}
        .pdf-viewer {{
            flex: 1;
            border: none;
        }}
        .divider {{
            width: 2px;
            background: #ccc;
        }}
        @media (max-width: 768px) {{
            .container {{
                flex-direction: column;
            }}
            .divider {{
                width: 100%;
                height: 2px;
            }}
        }}
    </style>
</head>
<body>
    <div class='header'>
        <strong>Xem Chứng Nhận & Tem Kiểm Định</strong>
        <a href='{certificateUrl}' target='_blank'>📄 Mở Chứng Nhận</a>
        <a href='{stampUrl}' target='_blank'>📋 Mở Tem</a>
        <a href='/Certificates'>← Quay lại</a>
    </div>
    <div class='container'>
        <iframe class='pdf-viewer' src='{certificateUrl}' title='Chứng Nhận Kiểm Định'></iframe>
        <div class='divider'></div>
        <iframe class='pdf-viewer' src='{stampUrl}' title='Tem Kiểm Định'></iframe>
    </div>
</body>
</html>";

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tạo PDF: {ex.Message}";
                return RedirectToAction("PrintCertificate", new { id });
            }
            finally
            {
                if (browser != null)
                {
                    try { await browser.CloseAsync(); } catch { }
                }
            }
        }

        // Helper method để render view thành HTML string
        private async Task<string> RenderViewToStringAsync(string viewName, object model)
        {
            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"View '{viewName}' không tìm thấy");
                }

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    sw,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);

                return sw.GetStringBuilder().ToString();
            }
        }
    }
}