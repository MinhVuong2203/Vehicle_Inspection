using Vehicle_Inspection.Models;

namespace Vehicle_Inspection.ViewModels
{
    public class CreateProfileViewModel
    {
        public Owner Owner { get; set; } = new Owner { OwnerType = "PERSON" };
        public Vehicle Vehicle { get; set; } = new Vehicle();
        public Specification Specification { get; set; } = new Specification();
    }
}