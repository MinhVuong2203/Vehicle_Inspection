using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using Vehicle_Inspection.Data;
using Vehicle_Inspection.Models;

namespace Vehicle_Inspection.Service
{
    public class Login : ILogin
    {
        private readonly VehInsContext _context;
        private readonly IConfiguration _configuration;


        public Login(VehInsContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public Account? login(string username, string password)
        {
            try
            {
                var account = _context.Accounts.FirstOrDefault(a => a.Username == username && !a.IsLocked);
                if (account == null || string.IsNullOrEmpty(account.PasswordHash))
                    return null;
                if (!BCrypt.Net.BCrypt.Verify(password, account.PasswordHash))
                //if (account.PasswordHash != password)
                    return null;
                return account;
            }
            catch (Exception ex) {
                throw new Exception("Có gì đó sai sai!", ex);
            }
        }

        public bool checkRoleLogin(Guid id)
        {
            return _context.Users.Include(u => u.Roles).Any(u => u.UserId == id && u.Roles.Any(r => r.RoleCode == "LOGIN"));
        }

        public async Task<User> GetUserByEmailAndCccdAsync(string email, string cccd)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.CCCD == cccd && u.IsActive);
        }


        public async Task<bool> SendPasswordResetOtpAsync(string email, string cccd)
        {
            var user = await GetUserByEmailAndCccdAsync(email, cccd);
            if (user == null)
                return false;

            // Generate 6-digit OTP
            var otp = new Random().Next(100000, 999999).ToString();
            var otpHash = BCrypt.Net.BCrypt.HashPassword(otp);

            // Get or create password recovery record
            var recovery = await _context.PasswordRecoveries
                .FirstOrDefaultAsync(p => p.UserId == user.UserId);

            if (recovery == null)
            {
                recovery = new PasswordRecovery
                {
                    UserId = user.UserId,
                    ResetOtpHash = otpHash,
                    ResetOtpExpiresAt = DateTime.Now.AddMinutes(10),
                    ResetOtpAttemptCount = 0
                };
                _context.PasswordRecoveries.Add(recovery);
            }
            else
            {
                recovery.ResetOtpHash = otpHash;
                recovery.ResetOtpExpiresAt = DateTime.Now.AddMinutes(10);
                recovery.ResetOtpAttemptCount = 0;
            }

            await _context.SaveChangesAsync();

            // Send email
            try
            {
                await SendOtpEmailAsync(email, user.FullName, otp);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                // For development, just log the OTP
                Console.WriteLine($"OTP for {email}: {otp}");
                return true; // Return true for development
            }
        }

        public async Task<bool> VerifyOtpAndResetPasswordAsync(string email, string otp, string newPassword)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

            if (user == null)
                return false;

            var recovery = await _context.PasswordRecoveries
                .FirstOrDefaultAsync(p => p.UserId == user.UserId);

            if (recovery == null)
                return false;

            // Check if OTP expired
            if (recovery.ResetOtpExpiresAt < DateTime.Now)
                return false;

            // Check attempt count
            if (recovery.ResetOtpAttemptCount >= 5)
                return false;

            // Verify OTP
            if (!BCrypt.Net.BCrypt.Verify(otp, recovery.ResetOtpHash))
            {
                recovery.ResetOtpAttemptCount++;
                await _context.SaveChangesAsync();
                return false;
            }

            // Update password
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == user.UserId);

            if (account == null)
                return false;

            account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            account.IsLocked = false;
            account.FailedCount = 0;

            // Clear recovery data
            recovery.ResetOtpHash = null;
            recovery.ResetOtpExpiresAt = null;
            recovery.ResetOtpAttemptCount = 0;

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task SendOtpEmailAsync(string toEmail, string fullName, string otp)
        {
            var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var fromEmail = _configuration["Email:FromEmail"];
            var fromPassword = _configuration["Email:FromPassword"];

            if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(fromPassword))
            {
                throw new Exception("Email configuration not found");
            }

            var subject = "Mã OTP khôi phục mật khẩu - Trung tâm đăng kiểm";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background: #f9f9f9; }}
                        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: white; padding: 30px; border-radius: 0 0 10px 10px; }}
                        .otp-box {{ background: #f0f0f0; border-left: 4px solid #667eea; padding: 20px; margin: 20px 0; font-size: 24px; font-weight: bold; text-align: center; letter-spacing: 5px; }}
                        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
                        .warning {{ color: #ea4335; font-weight: 600; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🔐 Khôi phục mật khẩu</h1>
                        </div>
                        <div class='content'>
                            <p>Xin chào <strong>{fullName}</strong>,</p>
                            <p>Bạn đã yêu cầu khôi phục mật khẩu cho tài khoản tại Trung tâm đăng kiểm.</p>
                            <p>Mã OTP của bạn là:</p>
                            <div class='otp-box'>{otp}</div>
                            <p class='warning'>⚠️ Mã OTP này có hiệu lực trong 10 phút và chỉ được nhập tối đa 5 lần.</p>
                            <p>Nếu bạn không yêu cầu khôi phục mật khẩu, vui lòng bỏ qua email này.</p>
                        </div>
                        <div class='footer'>
                            <p>© 2024 Trung tâm đăng kiểm xe. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(fromEmail, fromPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, "Trung tâm đăng kiểm"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
        }
    }


}
