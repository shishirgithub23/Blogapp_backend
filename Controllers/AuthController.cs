using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Blog.DTOs;
using Blog.Interfaces;

namespace Blog.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;

        public AuthController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var result = await _authRepository.Register(registerDto);
            if (result)
            {
                return Ok(new { Message = "Registration successful" });
            }

            return BadRequest(new { Message = "User already exists with that username or email" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var return_data = await _authRepository.Login(loginDto);
            return Ok(return_data);
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult TestAdmin()
        {
            return Ok(new { Message = "Admin endpoint test" });
        }

        [Authorize]
        [HttpPost("test-login")]
        public IActionResult SecureEndpoint()
        {
            return Ok(new { Message = "Test login" });
        }

        [HttpPost("send-reset-link")]
        public async Task<IActionResult> SendResetLink([FromBody] EmailDto emailDto)
        {
            var result = await _authRepository.SendResetLink(emailDto.Email);

            if (result)
            {
                return Ok(new { Message = "Reset link sent successfully" });
            }

            return BadRequest(new { Message = "Failed to send reset link" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ChangePasswordWithToken(ChangePasswordWithTokenDto changePasswordWithTokenDto)
        {
            var result = await _authRepository.ChangePasswordWithToken(changePasswordWithTokenDto.ResetToken, changePasswordWithTokenDto.Email, changePasswordWithTokenDto.NewPassword);

            if (result)
            {
                return Ok(new { Message = "Password changed successfully" });
            }

            return BadRequest(new { Message = "Invalid token or token expired" });
        } 

        [HttpPost("changepassword")]
        [Authorize]
        public IActionResult ChangePassword(ChangePasswordDTO data)
        {
            var result = _authRepository.ChangePassword(data);

            if (result!=null)
            {
                return Ok(new { Message = "Password changed successfully",token=result});
            }
            return BadRequest(new { Message = "Invalid!!!" });
        }
    }
}
