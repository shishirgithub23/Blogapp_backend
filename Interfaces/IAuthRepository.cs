using System.Threading.Tasks;
using Blog.DTOs;

namespace Blog.Interfaces
{
    public interface IAuthRepository
    {
        Task<bool> UserExistsByUsername(string username);
        Task<bool> UserExistsByEmail(string email);
        Task<bool> Register(RegisterDto registerDto);
        Task<string> Login(LoginDto loginDto);
        Task<bool> SendResetLink(string email);
        Task<bool> ChangePasswordWithToken(string token, string email, string newPassword);
    }
}
