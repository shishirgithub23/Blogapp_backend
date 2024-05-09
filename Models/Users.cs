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


    public class UserDTO
    {
        [Required(ErrorMessage="Please enter username!")]
        public string username { get; set; }

        [Required(ErrorMessage ="Please enter email")]
        [EmailAddress]
        public string email { get; set; }

        [Required(ErrorMessage = "Please enter user role")]
        public int role { get; set; }
    }

    public class UserData
    {
        public string username { get; set; }

        public string email { get; set; }

        public int role { get; set; }

        public string password_hash { get; set; }
    }


    public class UpdateUserDTO
    {

        public int userid {get;set; }
        [Required(ErrorMessage = "Please enter username!")]
        public string username { get; set; }

        [Required(ErrorMessage = "Please enter email")]
        [EmailAddress]
        public string email { get; set; }

        [Required(ErrorMessage = "Please enter user role")]
        public int role { get; set; }
    }

    public class UpdateUserData
    {

        public int userid { get; set; }
        public string username { get; set; }

        public string email { get; set; }

        public int role { get; set; }

        public string password_hash { get; set; }
    }
}
