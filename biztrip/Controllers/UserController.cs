using Microsoft.AspNetCore.Mvc;
using RegisterForTour.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using biztrip.Request; // Import namespace cho UserSearchRequest
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
namespace RegisterForTour.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UserController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // 1. Thêm mới user
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegister request)
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID is not available." });
            }
            User user = new User();
            // Kiểm tra username đã tồn tại
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest("Username already exists.");
            user.Password = "123456aA@";
            // Hash mật khẩu bằng bcrypt
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.Status = "01";
            user.FullName = request.FullName;
            user.Username = request.Username;
            user.Role = request.Role;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        [HttpPost("hdv")]
        public async Task<IActionResult> GetUsersByRoleAndStatus()
        {
            // Filter users based on role "03" and status "01"
            var users = await _context.Users
                           .FromSqlRaw("SELECT * FROM Users WHERE Role = '03' AND Status = '01'")
                           .ToListAsync();

            if (users == null || users.Count == 0)
            {
                return NotFound("No users found with the given role and status.");
            }

            return Ok(new
            {
                message = "this data",
                data = users
            });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginInfo)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginInfo.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginInfo.Password, user.Password))
                return Unauthorized("Invalid username or password.");

            if (!user.Status.Equals("01"))
                return Unauthorized("Account is inactive.");

            var jwtSettings = _configuration.GetSection("Jwt").Get<JwtSettings>();
            var token = GenerateJwtToken(user, jwtSettings);

            return Ok(new
            {
                message = "Login successful.",
                token = token
            });
        }

        private string GenerateJwtToken(User user, JwtSettings jwtSettings)
        {
            var claims = new[]
            {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Role, user.Role)
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddMinutes(jwtSettings.ExpireMinutes);

            var token = new JwtSecurityToken(
                issuer: jwtSettings.Issuer,
                audience: jwtSettings.Audience,
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        // 3. Sửa thông tin user
        [HttpPost("update")]
        public async Task<IActionResult> Update(User userUpdate)
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID is not available." });
            }
            var user = await _context.Users.FindAsync(userUpdate.Id);
            if (user == null)
                return NotFound("User not found.");

            user.FullName = userUpdate.FullName ?? user.FullName;
            user.Role = userUpdate.Role ?? user.Role;
            user.Status = userUpdate.Status;

            // Cập nhật mật khẩu nếu có
            if (!string.IsNullOrEmpty(userUpdate.Password))
                user.Password = BCrypt.Net.BCrypt.HashPassword(userUpdate.Password);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User updated successfully.", user });
        }

        // 4. Xóa user
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User deleted successfully." });
        }


        // 5. Lấy thông tin user theo ID
        [HttpPost("getuser")]
        public async Task<IActionResult> GetUser([FromBody] int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            return Ok(user);
        }
        [HttpPost("list")]
        public async Task<IActionResult> GetUsers([FromBody] UserSearchRequest request)
        {
            // Truy vấn từ bảng Users
            var role = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var query = _context.Users.AsQueryable();
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID is not available." });
            }
            if (role != "00")
            {
                query = query.Where(u => u.Id == int.Parse(userId));
            }
            // Tìm kiếm giống theo Username và FullName nếu có
            if (!string.IsNullOrEmpty(request.Username))
            {
                var usernamePattern = $"%{request.Username}%";
                query = query.Where(u => EF.Functions.Like(u.Username, usernamePattern));
            }

            if (!string.IsNullOrEmpty(request.FullName))
            {
                var fullNamePattern = $"%{request.FullName}%";
                query = query.Where(u => EF.Functions.Like(u.FullName, fullNamePattern));
            }

            // Tổng số bản ghi phù hợp
            var totalRecords = await query.CountAsync();

            // Lấy dữ liệu theo trang
            var users = await query
                .OrderBy(u => u.Id) // Sắp xếp theo Id tăng dần
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            // Trả về kết quả
            return Ok(new
            {
                page = request.Page,
                pageSize = request.PageSize,
                totalRecords,
                users
            });
        }



    }
}
