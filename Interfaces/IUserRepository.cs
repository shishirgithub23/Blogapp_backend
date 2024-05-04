using Blog.DTOs;
using Blog.Models;
using System.Collections.Generic;

namespace Blog.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserDto>> GetUsers();
        Task<UserDto> GetUserById(int id);
    }
}
