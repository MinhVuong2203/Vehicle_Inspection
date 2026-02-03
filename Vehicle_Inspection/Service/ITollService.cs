using Vehicle_Inspection.Models;

namespace Vehicle_Inspection.Service
{
    public interface ITollService
    {
        List<Inspection> GetInspections(string? search, short? status);
        Inspection? GetInspectionDetails(string inspectionCode);
        public Payment? GetPaymentByOrderCode(long? orderCode);
        public string CollectPayment(int paymentId, string paymentMethod, string? note, Guid userId);

        Task<(bool Success, string Message, Payment? NewPayment)> CreateAdditionalPaymentAsync(int inspectionId);
        Task<(bool Success, string Message, Payment? NewPayment)> CreateAdditionalPaymentByPaymentIdAsync(int paymentId);

        Inspection getInspectionByOrderCode(long? orderCode);
    }
}
