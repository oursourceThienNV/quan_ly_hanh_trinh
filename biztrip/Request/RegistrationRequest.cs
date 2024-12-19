namespace biztrip.Request
{
    public class RegistrationRequest
    {
        public string FullName { get; set; }

        public string Gender { get; set; }

        public string Phone { get; set; }


        public string Email { get; set; }


        public string Address { get; set; }


        public int Guests { get; set; }


        public int Children { get; set; }


        public string Reason { get; set; }

        public string Code { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
