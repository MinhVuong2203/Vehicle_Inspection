using System.ComponentModel.DataAnnotations.Schema;

namespace Vehicle_Inspection.Models
{
    public partial class User
    {
        [NotMapped]
        public string? AddressLine { get; set; }   // số nhà, đường...

        [NotMapped]
        public string? ProvinceName { get; set; }  // tên tỉnh

        [NotMapped]
        public string? WardName { get; set; }      // tên phường/xã
    }
}
