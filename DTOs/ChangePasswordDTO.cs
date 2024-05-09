using System.ComponentModel.DataAnnotations;

namespace Blog.DTOs
{
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage ="Please enter current password")]
        public string current_password {get;set; }
        [Required(ErrorMessage = "Please enter password")]
        public string  password{get;set; }
        [Required(ErrorMessage = "Please enter confirm password")]
        public string confirm_password {get;set; }
    }
}
