using Blog.DTOs;
using Blog.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using static Blog.Repository.UserRepository;

namespace Blog.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserDto>> GetUsers();
        Task<UserDto> GetUserById(int id);
        UserProfile GetUserProfile(int userId);
        string UpdateUserProfile(UpdateUserProfileDTO data);
        string DeleteUserProfile(int userid);


        string CreateUser(UserData data);
        string UpdateUser(UpdateUserData data);
    }
}
