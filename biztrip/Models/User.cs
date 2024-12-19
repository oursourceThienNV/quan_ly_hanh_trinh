namespace RegisterForTour.Models
{
    public class User
    {
        public int Id { get; set; }         // ID của user (khóa chính)
        public string Username { get; set; } // Tên đăng nhập
        public string Password { get; set; } // Mật khẩu (hashed)
        public string FullName { get; set; } // Tên đầy đủ
        public string Role { get; set; }     // Vai trò (e.g., "Admin", "User")
        public string Status { get; set; }     // Trạng thái (true: active, false: inactive)
    }
}
