using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RegisterForTour.Models;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace RegisterForTour
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure DbContext with SQL Server
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Configure CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                    builder.AllowAnyOrigin()  // Cho phép tất cả các nguồn
                           .AllowAnyMethod()  // Cho phép tất cả các phương thức (POST, GET, PUT, DELETE)
                           .AllowAnyHeader());  // Cho phép tất cả các header
            });

            // Add JWT Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false; // Set to true if running on HTTPS
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured."),
                        ValidAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured."),
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.")))
                    };
                });

            // Add controllers and endpoints
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // Add Swagger for API testing and documentation (optional for development)
           

            var app = builder.Build();

            // Use Swagger UI for API documentation in development environment
            

            // Use CORS before other middlewares
            app.UseCors("AllowAll");

            // Use Authentication and Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Map controllers
            app.MapControllers();

            // Run the app
            app.Run();
        }
    }
}
