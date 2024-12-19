using System.ComponentModel.DataAnnotations;

namespace biztrip.Request
{
    public class UpdateRegistrationRequest
    {
        [Required]
        public int RegistrationID { get; set; }

        [MaxLength(4000)]
        public string Description { get; set; }

        [MaxLength(50)]
        public string Status { get; set; }
    }

}
