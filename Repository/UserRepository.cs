using Blog.Interfaces;
using Blog.Data;
using Blog.DTOs;
using Blog.Models;
using Dapper;
using Microsoft.AspNetCore.Identity;

namespace Blog.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserDto>> GetUsers()
        {
            var query = @"SELECT *,
                            case when role=0 then 'USER'
                             when role=1 then 'ADMIN'
                             when role=2 then 'BLOGGER' 
                        end as role_name
                        FROM Users where active=1";

            using (var connection = _context.CreateConnection())
            {
                var users = await connection.QueryAsync<UserDto>(query);

                return users.ToList();
            }
        }


        public async Task<UserDto> GetUserById(int id)
        {
            var query = @"SELECT *,
                        case when role=0 then 'USER'
                             when role=1 then 'ADMIN'
                             when role=2 then 'BLOGGER'
                        end as role_name
                    
                        FROM Users WHERE UserId = @UserId and active=1";

            using (var connection = _context.CreateConnection())
            {
                var user = await connection.QueryFirstOrDefaultAsync<UserDto>(query, new { UserId = id });

                return user;
            }
        }

        public UserProfile GetUserProfile(int userId)
        {
            var data = new UserProfile();
            try
            {
                var query = @"SELECT
                            UserId,
                            UserName as username,
                            Email as email,
                            role,
                            case when role=0 then 'USER'
			                 when role=1 then 'ADMIN'
			                 when role=2 then 'BLOGGER'
			                end as role_name
                        FROM Users WHERE UserId = @UserId and active=1";

                using (var connection = _context.CreateConnection())
                {
                    var user = connection.Query<dynamic>(query, new { UserId = userId }).ToList();

                     data = new UserProfile
                    {
                        userid=user[0].UserId,
                        username = user[0].username,
                        email = user[0].email,
                        role = user[0].role,
                        role_name = user[0].role_name
                    };
                   
                }
            }
            catch (Exception ex)
            {

            }

            return data;
        }

        public string UpdateUserProfile(UpdateUserProfileDTO data)
        {
            try
            {
                var query = @"update Users
                            set
                            username=@username,
                            email=@email
                           output inserted.*
                        FROM Users WHERE UserId = @user_id and active=1";

                using (var connection = _context.CreateConnection())
                {
                    var user = connection.Query<dynamic>(query, new { username=data.username,email=data.email,user_id=data.user_id }).ToList();
                    if (user.Count() == 0)
                    {
                        throw new Exception("User Not Found!!");
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return "User Profile Updated Succssfully" ;
        }

        public string DeleteUserProfile(int userid)
        {

            using (var connection = _context.CreateConnection())
            {
                var successMessage = "";

                var check_user_exist = connection.ExecuteScalar<int>(@"select count(*) from dbo.users where UserId=@userid and active=1", new { userid = userid });
                if (check_user_exist == 0)
                {
                    throw new Exception("User doesnot exist");
                }

                //Delete the user (To Delete i am going update the data and change it's active status to false)
                var delete_user = @"update dbo.users set active=0 
                                 output inserted.*
                                where userid=@user_id and active=1";
                var deletedData = connection.Query<dynamic>(delete_user, new { user_id = userid }).ToList();
                if (deletedData.Count > 0)
                {
                    {
                        successMessage = "User profile removed successfully.";
                    }

                }
                return successMessage;
            }
        }

        public string CreateUser(UserData data)
        {
            try
            {
                var query = @"insert into dbo.users(UserName,Email,Role,PasswordHash)
                                output inserted.*  
                                values(@username,@email,@role,@passwordhash)         
                            ";


                using (var connection = _context.CreateConnection())
                {
                    var checkuseremail = @"select count(*) from dbo.users where email=@email and active=1";
                    if (connection.ExecuteScalar<int>(checkuseremail, new {email=data.email}) > 0)
                    {
                        throw new Exception("Email address is already present");
                    }

                    var checkforValidUsername = @"select count(*) from dbo.users where username=@username and active=1";

                    if (connection.ExecuteScalar<int>(checkforValidUsername, new { username = data.username }) > 0)
                    {
                        throw new Exception("Username is already taken!");
                    }

                    data.password_hash = BCrypt.Net.BCrypt.HashPassword(data.username+"123");

                    var user = connection.Query<dynamic>(query, new 
                    {
                        username = data.username, 
                        email = data.email,
                        role=data.role,
                        passwordhash=data.password_hash
                    }).ToList();

                    if (user.Count() == 0)
                    {
                        throw new Exception("User Not Found!!");
                    }

                }
            }
            catch (Exception ex)
            {
               // throw new Exception(ex.Message);
            }
            return "User inserted successfully.";
        }

        public string UpdateUser(UpdateUserData data)
        {
            try
            {
                var query = @"
                       update 
                        dbo.users  set 
                        UserName=@username,
                        Email=@email,
                        Role=@role
                     output inserted.*
                      where userid =@userid  and active=1                                      
                    ";
                using (var connection = _context.CreateConnection())
                {
                    var checkuseremail = @"select count(*) from dbo.users where email=@email and active=1 and userid<>@userid";
                    if (connection.ExecuteScalar<int>(checkuseremail, new { email = data.email,userid=data.userid }) > 0)
                    {
                        throw new Exception("Email address is already taken");
                    }

                    var checkforValidUsername = @"select count(*) from dbo.users where username=@username and active=1 and userid<>@userid";

                    if (connection.ExecuteScalar<int>(checkforValidUsername, new { username = data.username, userid =data.userid }) > 0)
                    {
                        throw new Exception("Username is already taken!");
                    }

                    var user = connection.Query<dynamic>(query, new
                    {
                        userid=data.userid,
                        username = data.username,
                        email = data.email,
                        role = data.role,
                    }).ToList();

                    if (user.Count() == 0)
                    {
                        throw new Exception("User Not Found!!");
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return "User updated successfully.";
        }
    }
}
