namespace biztrip.Request
{
    public class SearchRequest
    {
        public string FullName { get; set; }
        public int Page { get; set; } = 1; // Default to page 1
        public int PageSize { get; set; } = 10; // Default to 10 items per page
    }
}
