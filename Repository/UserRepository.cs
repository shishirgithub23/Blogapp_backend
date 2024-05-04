using Blog.Interfaces;
using Blog.Data;
using Blog.DTOs;
using Blog.Models;
using Dapper;

namespace Blog.Repository
{
    public class UserRepository: IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserDto>> GetUsers()
        {
            var query = "SELECT * FROM Users";

            using (var connection = _context.CreateConnection())
            {
                var users = await connection.QueryAsync<UserDto>(query);

                return users.ToList();
            }
        }


        public async Task<UserDto> GetUserById(int id)
        {
            var query = "SELECT * FROM Users WHERE UserId = @UserId";

            using (var connection = _context.CreateConnection())
            {
                var user = await connection.QueryFirstOrDefaultAsync<UserDto>(query, new { UserId = id });

                return user;
            }
        }
    }
}
