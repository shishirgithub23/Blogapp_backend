using System;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Blog.Data;
using Blog.DTOs;
using Blog.Interfaces;
using Blog.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Blog.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailRepository _emailService;

        public AuthRepository(AppDbContext context, IConfiguration configuration, IEmailRepository emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<bool> UserExistsByUsername(string username)
        {
            var query = "SELECT COUNT(*) FROM Users WHERE UserName = @UserName";
            using (var connection = _context.CreateConnection())
            {
                var result = await connection.ExecuteScalarAsync<int>(query, new { UserName = username });
                return result > 0;
            }
        }

        public async Task<bool> UserExistsByEmail(string email)
        {
            var query = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
            using (var connection = _context.CreateConnection())
            {
                var result = await connection.ExecuteScalarAsync<int>(query, new { Email = email });
                return result > 0;
            }
        }

        public async Task<bool> Register(RegisterDto registerDto)
        {
            if (await UserExistsByUsername(registerDto.UserName) || await UserExistsByEmail(registerDto.Email))
            {
                return false;
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var query = @"INSERT INTO Users (UserName, PasswordHash, Email, Role)
                          VALUES (@UserName, @PasswordHash, @Email, @Role)";

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, new
                {
                    UserName = registerDto.UserName,
                    PasswordHash = passwordHash,
                    Email = registerDto.Email,
                    Role = UserRole.User
                });
            }

            return true;
        }

        public async Task<string> Login(LoginDto loginDto)
        {
            var query = "SELECT * FROM Users WHERE UserName = @UserName";
            using (var connection = _context.CreateConnection())
            {
                var user = await connection.QueryFirstOrDefaultAsync<Users>(query, new { UserName = loginDto.UserName });

                if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                    return null;

                var role = (UserRole)user.Role;

                var key = _configuration["JwtSettings:SecretKey"];
                var issuer = _configuration["JwtSettings:Issuer"];
                var audience = _configuration["JwtSettings:Audience"];

                return GenerateJwtToken(user.UserId, role, key, issuer, audience);
            }
        }

        private string GenerateJwtToken(int userId, UserRole role, string key, string issuer, string audience)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var byteKey = Encoding.ASCII.GetBytes(key);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Role, role.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(byteKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task<Users> GetUserByEmail(string email)
        {
            var query = "SELECT * FROM Users WHERE Email = @Email";
            using (var connection = _context.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Users>(query, new { Email = email });
            }
        }

        public async Task<bool> SendResetLink(string email)
        {
            var user = await GetUserByEmail(email);
            if (user == null)
            {
                return false;
            }

            var resetToken = Guid.NewGuid().ToString();
            var resetLinkFormat = _configuration.GetValue<string>("ResetLink:ResetLinkFormat");
            var resetLink = string.Format(resetLinkFormat, resetToken);

            var subject = "Reset Your Password";
            var message = $"Click <a href='{resetLink}'>here</a> to reset your password.";

            await _emailService.SendEmailAsync(user.Email, "Reset Password", message);

            user.ResetToken = resetToken;
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);

            var query = "UPDATE Users SET ResetToken = @ResetToken, ResetTokenExpiry = @ResetTokenExpiry WHERE UserId = @UserId";
            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, new
                {
                    ResetToken = user.ResetToken,
                    ResetTokenExpiry = user.ResetTokenExpiry,
                    UserId = user.UserId
                });
            }

            return true;
        }

        public async Task<bool> ChangePasswordWithToken(string token, string email, string newPassword)
        {
            var query = @"SELECT UserId, ResetTokenExpiry FROM Users 
                          WHERE ResetToken = @ResetToken AND Email = @Email AND ResetTokenExpiry > @CurrentTime";

            using (var connection = _context.CreateConnection())
            {
                var user = await connection.QueryFirstOrDefaultAsync<Users>(query, new
                {
                    ResetToken = token,
                    Email = email,
                    CurrentTime = DateTime.UtcNow
                });

                if (user == null)
                {
                    return false;
                }

                var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

                var updateQuery = @"UPDATE Users 
                                    SET PasswordHash = @PasswordHash, ResetToken = NULL, ResetTokenExpiry = NULL 
                                    WHERE UserId = @UserId";

                var rowsAffected = await connection.ExecuteAsync(updateQuery, new
                {
                    PasswordHash = newPasswordHash,
                    UserId = user.UserId
                });

                return rowsAffected > 0;
            }
        }
    }
}
