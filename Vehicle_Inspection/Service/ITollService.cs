using Vehicle_Inspection.Models;

namespace Vehicle_Inspection.Service
{
    public interface ITollService
    {
        List<Inspection> GetInspections(string? search, short? status);
        Inspection? GetInspectionDetails(string inspectionCode);
        string CollectPayment(string inspectionCode, string paymentMethod, string? note, Guid userId);
    }
}
