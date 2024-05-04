using System.ComponentModel.DataAnnotations;

namespace Blog.DTOs
{
    public class RegisterDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }

    public class LoginDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class ResetPasswordDto
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }

    public class EmailDto
    {
        public string Email { get; set; }
    }

    public class ChangePasswordWithTokenDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string ResetToken { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }
}
