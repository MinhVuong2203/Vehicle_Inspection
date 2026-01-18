using Vehicle_Inspection.Models;

namespace Vehicle_Inspection.Service
{
    public interface ILogin
    {
        public Account? login(string username, string password);
        public bool checkRoleLogin(Guid id);
    }
}
