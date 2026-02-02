using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vehicle_Inspection.Service;
using Vehicle_Inspection.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Vehicle_Inspection.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogin _loginService;
        public AccountController(ILogin loginService)
        {
            _loginService = loginService;
        }

        // GET: Account/Login (Login page)
        public IActionResult Login()
        {
            //string a = BCrypt.Net.BCrypt.HashPassword("ThiMai@123");
            //Debug.WriteLine("-------------------- " + a + " ------------------");
            // Nếu user đã đăng nhập, redirect về trang chủ
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, bool remember = false)
        {
            // Kiểm tra input
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ thông tin!";
                return RedirectToAction("Login", new { error = true });
            }

            // Validate user (thay bằng logic database của bạn)
            Account? account = _loginService.login(username, password);
            if (account != null)
            {
               
                if (!_loginService.checkRoleLogin(account.UserId))
                {
                    Console.Write("--------------------- " + account.UserId);
                    TempData["ErrorMessage"] = "Tài khoản của bạn không có quyền truy cập hệ thống!\nHãy liên hệ quản trị viên";

                    return RedirectToAction("Login", new { error = true });
                }

                // Tạo claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.NameIdentifier, account.UserId.ToString()),
                    new Claim(ClaimTypes.Role, "User")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = remember,
                    ExpiresUtc = remember ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(2)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);
                TempData["InfoMessage"] = "Chào mừng bạn trở lại!";
                return RedirectToAction("Index", "Home");
            }

            // Login thất bại
            TempData["ErrorMessage"] = "Sai tên đăng nhập hoặc mật khẩu!";
            return RedirectToAction("Login", new { error = true });
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email, string cccd)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(cccd))
            {
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin!" });
            }

            var success = await _loginService.SendPasswordResetOtpAsync(email, cccd);

            if (!success)
            {
                return Json(new { success = false, message = "Email hoặc CCCD không chính xác!" });
            }

            return Json(new { success = true, message = "Mã OTP đã được gửi đến email của bạn!" });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string email, string otp, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp) || string.IsNullOrWhiteSpace(newPassword))
            {
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin!" });
            }

            var success = await _loginService.VerifyOtpAndResetPasswordAsync(email, otp, newPassword);

            if (!success)
            {
                return Json(new { success = false, message = "Mã OTP không chính xác hoặc đã hết hạn!" });
            }

            return Json(new { success = true, message = "Đặt lại mật khẩu thành công!" });
        }

    

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }




    }
}
