using Vehicle_Inspection.Models;

namespace Vehicle_Inspection.Service
{
    public interface ILogin
    {
        public Account? login(string username, string password);
        public bool checkRoleLogin(Guid id);
        Task<bool> SendPasswordResetOtpAsync(string email, string cccd);
        Task<bool> VerifyOtpAndResetPasswordAsync(string email, string otp, string newPassword);
        Task<User> GetUserByEmailAndCccdAsync(string email, string cccd);

    }
}
