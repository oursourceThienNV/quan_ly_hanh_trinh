namespace biztrip.Request
{
    public class UserSearchRequest
    {
        public string Username { get; set; }   // Tìm kiếm theo username
        public string FullName { get; set; }  // Tìm kiếm theo fullname
        public int Page { get; set; } = 1;    // Trang hiện tại
        public int PageSize { get; set; } = 10; // Kích thước trang
    }
}
