using System.ComponentModel.DataAnnotations;

namespace Blog.Models
{
    public enum UserRole
    {   
        User = 0,
        Admin = 1,
        Blogger = 2
    }


    public class Users
    {
        [Key]
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public string ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; } 
    }

    public class Auth
    {
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
    }

    public class AuthDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
