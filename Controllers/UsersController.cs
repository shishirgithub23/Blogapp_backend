using Blog.Data;
using Blog.Interfaces;
using Blog.Models;
using Blog.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.Security.Claims;

namespace Blog.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {

        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;
        public UsersController(AppDbContext context, IHttpContextAccessor httpContextAccessor, IUserRepository userRepository)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
        }

        private int GetUserIdFromContext()
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (!_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                throw new ApplicationException("User is not authenticated.");
            }
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            throw new ApplicationException("User ID claim not found in JWT token.");
        }


        [HttpGet("getusers")]
        [Authorize(Roles = "Admin")]
       // [Authorize]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userRepository.GetUsers();

            if (users == null)
            {
                return NotFound();
            }

            return Ok(users);
        }


        [HttpGet("{id}")]
        [Authorize(Roles = "Blogger, Admin")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userRepository.GetUserById(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpGet("GetUserProfile")]
        [Authorize]
        public IActionResult GetUserProfile()
        {
            var userId = GetUserIdFromContext();
            var ProfileData = _userRepository.GetUserProfile(userId);
            

            if (ProfileData == null)
            {
                return NotFound();
            }

            return Ok(ProfileData);
        }
        [HttpPost("updateUserProfile")]
        [Authorize]
        public IActionResult UpdateUserProfile(UpdateUserProfileDTO data)
        {
            var userId = GetUserIdFromContext();
            data.user_id = userId;
            var ProfileData = _userRepository.UpdateUserProfile(data);
            

            if (ProfileData == null)
            {
                return NotFound();
            }

            return Ok(ProfileData);
        }

        [HttpPost("deleteprofile")]
        [Authorize]
        public IActionResult DeleteProfile(int userid)
        {
            var result = _userRepository.DeleteUserProfile(userid);
            return Ok(result);
        }

        #region User
        [HttpPost("createuser")]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateUser(UserDTO dtodata)
        {
            var data = new UserData
            {
                username = dtodata.username,
                email = dtodata.email,
                password_hash = "",
                role = dtodata.role
            };

            var final_data = _userRepository.CreateUser(data);

            return Ok(data);
        }

        [HttpPost("updateuser")]
        [Authorize]
        public IActionResult UpdateUser(UpdateUserDTO updateDTO)
        {
            var data = new UpdateUserData
            {
                userid=updateDTO.userid,
                username = updateDTO.username.Trim(),
                email = updateDTO.email.Trim(),
                password_hash = "",
                role = updateDTO.role
            };

            var final_data = _userRepository.UpdateUser(data);

            return Ok(final_data);
        }

        #endregion
    }
}
