using Microsoft.AspNetCore.Mvc;

namespace Vehicle_Inspection.Controllers
{
    public class InspectionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
