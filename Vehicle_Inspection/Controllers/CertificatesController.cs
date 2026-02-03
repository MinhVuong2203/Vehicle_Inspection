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

                // Render view thành HTML string
                var htmlContent = await RenderViewToStringAsync("PrintCertificate", inspection);

                // Launch Puppeteer browser
                browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
                });

                var page = await browser.NewPageAsync();
                await page.SetContentAsync(htmlContent);

                // Export PDF - A4 landscape
                byte[] pdfBytes = await page.PdfDataAsync(new PdfOptions
                {
                    Format = PaperFormat.A4,
                    Landscape = true,
                    PrintBackground = true
                });

                // Đóng browser ngay
                await browser.CloseAsync();
                browser = null;

                // Tạo tên file: BienSoXe_TenChuXe.pdf
                var plateNo = inspection.Vehicle?.PlateNo?.Replace(" ", "").Replace("-", "") ?? "Unknown";
                var ownerName = inspection.Vehicle?.Owner?.FullName?.Replace(" ", "_") ?? "Unknown";
                var fileName = $"{plateNo}_{ownerName}.pdf";

                // Lưu file vào wwwroot/downloads
                var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "downloads");
                Directory.CreateDirectory(savePath);
                var fullPath = Path.Combine(savePath, fileName);
                System.IO.File.WriteAllBytes(fullPath, pdfBytes);

                // Trả về file để tải xuống
                return File(pdfBytes, "application/pdf", fileName);
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