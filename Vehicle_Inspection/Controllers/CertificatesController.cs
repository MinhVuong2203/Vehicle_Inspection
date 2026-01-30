using Microsoft.AspNetCore.Mvc;
using Vehicle_Inspection.Service;

namespace Vehicle_Inspection.Controllers
{
    public class CertificatesController : Controller
    {
        private readonly ICertificatesService _certificatesService;

        public CertificatesController(ICertificatesService certificatesService)
        {
            _certificatesService = certificatesService;
        }

        public async Task<IActionResult> Index()
        {
            var inspections = await _certificatesService.GetCompletedInspectionsAsync();
            return View(inspections);
        }

        // Action để in giấy chứng nhận
        public async Task<IActionResult> PrintCertificate(int id)
        {
            var inspection = await _certificatesService.GetInspectionForCertificateAsync(id);

            if (inspection == null)
            {
                return NotFound();
            }

            // Tính thời hạn đăng kiểm
            int validityMonths = _certificatesService.CalculateValidityMonths(inspection.Vehicle);
            ViewBag.ValidityMonths = validityMonths;

            return View(inspection);
        }
    }
}