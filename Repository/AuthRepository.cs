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
using Microsoft.AspNetCore.Identity;

namespace Blog.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailRepository _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthRepository(AppDbContext context, IConfiguration configuration, IEmailRepository emailService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> UserExistsByUsername(string username)
        {
            var query = "SELECT COUNT(*) FROM Users WHERE UserName = @UserName and active=1";
            using (var connection = _context.CreateConnection())
            {
                var result = await connection.ExecuteScalarAsync<int>(query, new { UserName = username });
                return result > 0;
            }
        }

        public async Task<bool> UserExistsByEmail(string email)
        {
            var query = "SELECT COUNT(*) FROM Users WHERE Email = @Email and active=1";
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

        public async Task<LoginResponseData> Login(LoginDto loginDto)
        {
            var query = "SELECT * FROM Users WHERE email = @UserName and active=1";
            using (var connection = _context.CreateConnection())
            {
                var user =  connection.Query<Users>(query, new { UserName = loginDto.UserName }).ToList();

                if (user.Count() ==0)
                {
                    throw new Exception("User Does Not Exist!");
                }

                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user[0].PasswordHash))
                {
                    throw new Exception("User Name Or Password Is Not Valid!");
                }

                var role = (UserRole)user[0].Role;
                var role1 = (int)role;

                var key = _configuration["JwtSettings:SecretKey"];
                var issuer = _configuration["JwtSettings:Issuer"];
                var audience = _configuration["JwtSettings:Audience"];

                var generated_token = GenerateJwtToken(user[0].UserId, role, key, issuer, audience);

               var login_response_data = new LoginResponseData {
                   Token=generated_token,
                   Role= role1==1?"ADMIN": role1 == 0?"USER":"BLOGGER"
               };

                return login_response_data;
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
            var query = "SELECT * FROM Users WHERE Email = @Email and active=1";
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

            var query = "UPDATE Users SET ResetToken = @ResetToken, ResetTokenExpiry = @ResetTokenExpiry WHERE UserId = @UserId and active=1";
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
                    throw new Exception("Some unexpected error occured during handling of request.");
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

        public string ChangePassword(ChangePasswordDTO data)
        {
            var user_id =GetUserIdFromContext();

            var query = "SELECT * FROM Users WHERE UserId = @UserId and active=1";

            using (var connection = _context.CreateConnection())
            {
                var user = connection.Query<Users>(query, new { UserId =user_id }).ToList();

                if (user.Count == 0)
                {
                    throw new Exception("User does not exist.");
                }

                if (data.password != data.confirm_password)
                {
                    throw new Exception("Password and confirm password doesnot match.");
                }

                if (user == null || !BCrypt.Net.BCrypt.Verify(data.current_password, user[0].PasswordHash))
                {
                    throw new Exception("Old password doesnot match!");
                }

                var hashed_password= BCrypt.Net.BCrypt.HashPassword(data.password);

                var updateQuery = @"UPDATE Users 
                                    SET PasswordHash = @PasswordHash
                                    WHERE UserId = @UserId and active=1";

                connection.Execute(updateQuery, new
                {
                    PasswordHash = hashed_password,
                    UserId = user_id
                });

            
                var key = _configuration["JwtSettings:SecretKey"];
                var issuer = _configuration["JwtSettings:Issuer"];
                var audience = _configuration["JwtSettings:Audience"];

                return GenerateJwtToken(user[0].UserId, user[0].Role, key, issuer, audience);
            }

        }

        private int GetUserIdFromContext()
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (!_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                throw new ApplicationException("User is not authenticated.");
            }
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            throw new ApplicationException("User ID claim not found in JWT token.");
        }
    }
}
