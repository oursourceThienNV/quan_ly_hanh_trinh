namespace biztrip.Request
{
    public class RequestTrip
    {
        public int RegistrationID { get; set; }
        public string TravelName { get; set; }
        public string TravelDescription { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Status { get; set; }

        public int ResponsiblePersonID { get; set; }
        public List<StageDto> Stages { get; set; }
    }
}
