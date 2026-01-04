using System.Text.Json;

namespace Vehicle_Inspection.Models
{
    public class ProvinceDto
    {
        public JsonElement matinhBNV { get; set; } // string/number đều đọc được
        public JsonElement matinhTMS { get; set; }
        public string tentinhmoi { get; set; } = null!;
        public List<WardDto> phuongxa { get; set; } = new();
    }

    public class WardDto
    {
        public long maphuongxa { get; set; }
        public string tenphuongxa { get; set; } = null!;
    }
}