﻿using Blog.Models;

namespace Blog.DTOs
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }

        public string? role_name { get; set; }
    }


    // Auth Dtos
    


}
