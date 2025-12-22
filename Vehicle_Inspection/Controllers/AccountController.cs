using Microsoft.AspNetCore.Mvc;

namespace Vehicle_Inspection.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
