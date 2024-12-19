using RegisterForTour.Models;
using System.ComponentModel.DataAnnotations;

namespace biztrip.Models
{
    public class Registration
    {
        public int RegistrationID { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public int? Guests { get; set; }  // Nullable int
        public int? Children { get; set; }  // Nullable int
        public string Reason { get; set; }
        public string Code { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public int? ApproverID { get; set; }  // Nullable int
        public int? RecipientID { get; set; }  // Nullable int
        public int? ResponsiblePersonID { get; set; }  // Nullable int
        public DateTime? CreatedAt { get; set; }  // Nullable DateTime
        public DateTime? UpdatedAt { get; set; }
        public string TravelName { get; set; }
        public string TravelDescription { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

}
