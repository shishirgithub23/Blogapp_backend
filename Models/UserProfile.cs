namespace Blog.Models
{
    public class UpdateUserProfileDTO
    {
        public int user_id {get; set;}
        public string username { get; set; }
        public string email { get; set; }
    }
    public class UserProfile
    {
        public int userid {get; set;}
        public string username { get; set; }
        public string email { get; set; }
        public int role { get; set; }
        public string? role_name { get; set; }
    }
}
