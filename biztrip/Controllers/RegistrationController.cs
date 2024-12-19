using biztrip.Models;
using Microsoft.AspNetCore.Mvc;
using RegisterForTour.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.Data;
using biztrip.Request;
using Microsoft.EntityFrameworkCore;
using Azure.Core;
using System.Security.Claims;
using Microsoft.Data.SqlClient;
namespace biztrip.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RegistrationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/Registration
        [HttpPost("register")]
        public async Task<IActionResult> CreateRegistration([FromBody] RegistrationRequest registerRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {

                // Add the registration data
                Registration registration = new Registration();
                registration.Address = registerRequest.Address;
                registration.Children = registerRequest.Children;
                registration.Code = registerRequest.Code; // Nếu có hoặc để NULL
                registration.CreatedAt = DateTime.UtcNow; // Hoặc giá trị từ request nếu cần
                registration.Email = registerRequest.Email;
                registration.FullName = registerRequest.FullName;
                registration.Gender = registerRequest.Gender;
                registration.Guests = registerRequest.Guests;
                registration.Phone = registerRequest.Phone;
                registration.Reason = registerRequest.Reason;
                registration.UpdatedAt = DateTime.UtcNow;
                registration.Status = "00";// Hoặc giá trị từ request nếu cần
                registration.Description = "Nhập thông tin đã tư vấn tại đây";
                _context.Registration.Add(registration);
                await _context.SaveChangesAsync();

                // Generate and update the Code
                registration.Code = $"CODE-{registration.CreatedAt:yyyyMMdd}-{registration.RegistrationID:D3}";
                _context.Registration.Update(registration);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Registration created successfully", registration });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }
        [HttpPost("update")]
        public async Task<IActionResult> UpdateRegistration([FromBody] UpdateRegistrationRequest updateRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User ID is not available." });
                }

                // Query to fetch registration record
                var query = @"SELECT 
                        r.RegistrationID,
                r.FullName,
                r.Gender,
                r.Phone,
                r.Email,
                r.Address,
                r.Guests,
                r.Children,
                r.Reason,
                r.Code,
                r.Status,
                r.Description,
                ISNULL(r.TravelName,'') as TravelName, 
                ISNULL(r.TravelDescription,'') as TravelDescription,
                ISNULL(r.EndDate,'') as EndDate, 
                ISNULL(r.StartDate,'') as StartDate, 
                ISNULL(r.ApproverID,0) as ApproverID, 
                ISNULL(r.RecipientID,0) as  RecipientID,
                ISNULL(r.ResponsiblePersonID,0) as ResponsiblePersonID, 
                r.CreatedAt,
                r.UpdatedAt
                    FROM Registration r 
                    WHERE r.RegistrationID = {0}";

                var registration = await _context.Registration
                    .FromSqlRaw(query, updateRequest.RegistrationID)
                    .FirstOrDefaultAsync();

                if (registration == null)
                {
                    return NotFound(new { message = "Registration not found" });
                }

                // Update fields
                registration.Description = updateRequest.Description;
                registration.Status = updateRequest.Status;
                registration.UpdatedAt = DateTime.UtcNow;
                registration.RecipientID = int.Parse(userId);

                // Update database
                var updateQuery = @"UPDATE Registration SET 
                                Description = {0}, 
                                Status = {1}, 
                                UpdatedAt = {2}, 
                                RecipientID = {3} 
                            WHERE RegistrationID = {4}";

                await _context.Database.ExecuteSqlRawAsync(updateQuery,
                    registration.Description,
                    registration.Status,
                    registration.UpdatedAt,
                    registration.RecipientID,
                    registration.RegistrationID);

                return Ok(new { message = "Registration updated successfully", registration });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = "Database error occurred", error = dbEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }



        [HttpPost("search")]
        public async Task<IActionResult> SearchRegistrations([FromBody] SearchRequest request)
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID is not available." });
            }
            try
            {
                // Viết câu lệnh SQL để lấy dữ liệu
                string sqlQuery = @"
            SELECT 
                r.RegistrationID,
                r.FullName,
                r.Gender,
                r.Phone,
                r.Email,
                r.Address,
                r.Guests,
                r.Children,
                r.Reason,
                r.Code,
                r.Status,
                r.Description,
                ISNULL(r.EndDate,'') as EndDate, 
                ISNULL(r.StartDate,'') as StartDate, 
                ISNULL(r.ApproverID,0) as ApproverID, 
                ISNULL(r.RecipientID,0) as  RecipientID,
                ISNULL(r.ResponsiblePersonID,0) as ResponsiblePersonID, 
                ISNULL(ur.FullName, '') AS Recipient,  
                ISNULL(ua.FullName, '') AS Approver,    
                ISNULL(up.FullName, '') AS ResponsiblePerson,
                ISNULL(r.TravelName, '') AS TravelName, 
                ISNULL(r.TravelDescription, '') AS TravelDescription, 
                r.CreatedAt,
                r.UpdatedAt
            FROM 
                Registration r
            LEFT JOIN 
                Users ur ON r.RecipientID = ur.Id
            LEFT JOIN 
                Users ua ON r.ApproverID = ua.Id
            LEFT JOIN 
                Users up ON r.ResponsiblePersonID = up.Id
            WHERE 
                (@FullName IS NULL OR r.FullName LIKE '%' + @FullName + '%')
            ORDER BY 
                r.RegistrationID
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY
        ";

                // Định nghĩa các tham số SQL
                var parameters = new[]
                {
            new SqlParameter("@FullName", request.FullName ?? (object)DBNull.Value),
            new SqlParameter("@Skip", (request.Page - 1) * request.PageSize),
            new SqlParameter("@PageSize", request.PageSize)
        };

                // Thực thi truy vấn SQL với các tham số
                var registrations = await _context.RegistrationResponse
                    .FromSqlRaw(sqlQuery, parameters)
                    .ToListAsync();

                // Fetch the total record count for pagination
                string countQuery = @"
            SELECT COUNT(*) 
            FROM Registration r
            LEFT JOIN Users ur ON r.RecipientID = ur.Id
            LEFT JOIN Users ua ON r.ApproverID = ua.Id
            LEFT JOIN Users up ON r.ResponsiblePersonID = up.Id
            WHERE 
                (@FullName IS NULL OR r.FullName LIKE '%' + @FullName + '%')
        ";

                var totalRecords = await _context.Database.ExecuteSqlRawAsync(countQuery, parameters);

                // Return the formatted response
                return Ok(new
                {
                    page = request.Page,
                    pageSize = request.PageSize,
                    totalRecords = totalRecords,
                    users = registrations.Select(r => new
                    {
                        RegistrationID = r.RegistrationID,
                        fullName = r.FullName,
                        gender = r.Gender,
                        phone = r.Phone,
                        email = r.Email,
                        address = r.Address,
                        guests = r.Guests,
                        children = r.Children,
                        reason = r.Reason,
                        code = r.Code,
                        status = r.Status,
                        description = r.Description,
                        approverID = r.ApproverID,
                        recipientID = r.RecipientID,
                        responsiblePersonID = r.ResponsiblePersonID,
                        recipient = r.Recipient,
                        approver = r.Approver,
                        responsiblePerson = r.ResponsiblePerson,
                        travelName=r.TravelName,
                        travelDescription=r.TravelDescription,
                        startDate=r.StartDate,
                        endDate=r.EndDate,
                        createdAt = r.CreatedAt,
                        updatedAt = r.UpdatedAt
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
        [HttpPost("create-plan")]
        public async Task<IActionResult> UpdateRegistrationAndStages([FromBody] RequestTrip request)
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID is not available." });
            }
            // Kiểm tra nếu RegistrationID tồn tại
            var query = @"SELECT 
                    r.RegistrationID,
                    r.FullName,
                    r.Gender,
                    r.Phone,
                    r.Email,
                    r.Address,
                    r.Guests,
                    r.Children,
                    r.Reason,
                    r.Code,
                    r.Status,
                    r.Description,
                    ISNULL(r.TravelName,'') as TravelName, 
                    ISNULL(r.TravelDescription,'') as TravelDescription,
                    ISNULL(r.EndDate,'') as EndDate, 
                    ISNULL(r.StartDate,'') as StartDate, 
                    ISNULL(r.ApproverID,0) as ApproverID, 
                    ISNULL(r.RecipientID,0) as  RecipientID,
                    ISNULL(r.ResponsiblePersonID,0) as ResponsiblePersonID, 
                    r.CreatedAt,
                    r.UpdatedAt
                FROM Registration r 
                WHERE r.RegistrationID = @RegistrationID";

            // Thực hiện query để kiểm tra sự tồn tại của Registration
            var registration = await _context.Registration
                .FromSqlRaw(query, new SqlParameter("@RegistrationID", request.RegistrationID))
                .FirstOrDefaultAsync();

            if (registration == null)
            {
                return NotFound("Registration not found.");
            }

            // Cập nhật thông tin Registration
            registration.TravelName = request.TravelName;
            registration.TravelDescription = request.TravelDescription;
            registration.ResponsiblePersonID = request.ResponsiblePersonID;
            registration.StartDate = request.StartDate;
            registration.EndDate = request.EndDate;
            registration.UpdatedAt = DateTime.UtcNow;
            if (request.Status == "07"|| request.Status == "04")
            {
                
                registration.ApproverID = int.Parse(userId);
            }
            registration.Status = request.Status;// Cập nhật thời gian sửa
            _context.Registration.Update(registration);

            // Xóa tất cả các Stage cũ liên quan đến RegistrationID
            var deleteStagesQuery = "DELETE FROM Stages WHERE RegistrationID = @RegistrationID";
            await _context.Database.ExecuteSqlRawAsync(deleteStagesQuery, new SqlParameter("@RegistrationID", request.RegistrationID));

            // Xử lý thêm các bản ghi vào bảng Stages
            if (request.Stages != null && request.Stages.Count > 0)
            {
                foreach (var stage in request.Stages)
                {
                    var insertStageQuery = @"
            INSERT INTO Stages (StartTime, EndTime, Location, Description, RegistrationID, CreatedAt, UpdatedAt)
            VALUES (@StartTime, @EndTime, @Location, @Description, @RegistrationID, @CreatedAt, @UpdatedAt)";

                    await _context.Database.ExecuteSqlRawAsync(insertStageQuery,
                        new SqlParameter("@StartTime", stage.StartTime),
                        new SqlParameter("@EndTime", stage.EndTime),
                        new SqlParameter("@Location", stage.Location),
                        new SqlParameter("@Description", stage.Description ?? (object)DBNull.Value),
                        new SqlParameter("@RegistrationID", request.RegistrationID),
                        new SqlParameter("@CreatedAt", DateTime.UtcNow),
                        new SqlParameter("@UpdatedAt", DateTime.UtcNow)
                    );
                }
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Registration updated and stages added successfully."
            });
        }
    [HttpPost("stages")]
        public async Task<IActionResult> GetStagesByRegistrationId([FromBody] QueryRegistrationID request)
        {
            try
            {
                // SQL query to get the stages based on RegistrationID
                string sqlQuery = @"
                SELECT 
                    s.Id,
                    s.RegistrationID,
                    s.StartTime,
                    s.EndTime,
                    s.Location,
                    s.Description
                FROM 
                    Stages s
                WHERE 
                    s.RegistrationID = @RegistrationID";

                // Define the parameter for the SQL query
                var registrationIdParam = new SqlParameter("@RegistrationID", request.RegistrationID);

                // Execute the query and map the results to StageDto
                var stages = await _context.Stages
                    .FromSqlRaw(sqlQuery, registrationIdParam)
                    .ToListAsync();

                // Return the list of stages
                return Ok(stages);
            }
            catch (Exception ex)
            {
                // Handle errors and return a 500 status code
                return StatusCode(500, new { message = "An error occurred while fetching stages", error = ex.Message });
            }
        }
    }

}
