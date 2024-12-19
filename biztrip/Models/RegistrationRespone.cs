using System;
using System.ComponentModel.DataAnnotations;

namespace biztrip.Models
{
    public class RegistrationRespone
    {
        public int RegistrationID { get; set; }  // Primary Key
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public int Guests { get; set; }
        public int Children { get; set; }
        public string Reason { get; set; }
        public string Code { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public int ApproverID { get; set; }
        public int RecipientID { get; set; }
        public int ResponsiblePersonID { get; set; }
        public string Recipient { get; set; }
        public string Approver { get; set; }
        public string ResponsiblePerson { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string TravelName { get; set; }
        public string TravelDescription { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
}
