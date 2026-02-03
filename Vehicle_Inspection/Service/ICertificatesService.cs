using Vehicle_Inspection.Models;

namespace Vehicle_Inspection.Service
{
    public interface ICertificatesService
    {
        Task<List<Inspection>> GetCompletedInspectionsAsync();
        Task<Inspection?> GetInspectionForCertificateAsync(int inspectionId);
        int CalculateValidityMonths(Vehicle vehicle);
        Task UpdateInspectionStatusAsync(int inspectionId, int newStatus);


    }
}