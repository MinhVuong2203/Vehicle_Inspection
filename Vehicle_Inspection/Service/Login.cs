using Vehicle_Inspection.Data;
using Vehicle_Inspection.Models;

namespace Vehicle_Inspection.Service
{
    public class Login : ILogin
    {
        private readonly VehInsContext _context;

        public Login(VehInsContext context)
        {
            _context = context;
        }

        public Account? login(string username, string password)
        {
            try
            {
                var account = _context.Accounts.FirstOrDefault(a => a.Username == username && !a.IsLocked);
                if (account == null || string.IsNullOrEmpty(account.PasswordHash))
                    return null;
                if (!BCrypt.Net.BCrypt.Verify(password, account.PasswordHash))
                    return null;
                return account;
            }
            catch (Exception ex) {
                throw new Exception("Có gì đó sai sai!", ex);
            }
        }
    }
}
