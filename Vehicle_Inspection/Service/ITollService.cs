using Vehicle_Inspection.Models;

namespace Vehicle_Inspection.Service
{
    public interface ITollService
    {
        List<object> GetInspections(string? search, int? status);
    }
}
