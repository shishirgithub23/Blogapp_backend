using Blog.Interfaces;
using Blog.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blog.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly IBlogRepository _blogRepository;

        public BlogController(IBlogRepository blogRepository)
        {
            _blogRepository = blogRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetBlogs()
        {
            var blogs = await _blogRepository.GetBlogs();

            if (blogs == null)
            {
                return NotFound(new { message = "Something Went Wrong While Fetching" });
            }

            return Ok(blogs);
        }


        [HttpGet("{blogId}")]
        public async Task<IActionResult> GetBlogById(int blogId)
        {
            var blog = await _blogRepository.GetBlogById(blogId);

            if (blog == null)
            {
                return NotFound(new { message = "Blog not found With That Id" });
            }

            return Ok(blog);
        }


        [HttpPost]
        [Authorize(Roles = "Blogger")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateBlog([FromForm] BlogInsertDto blogDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var isSuccess = await _blogRepository.CreateBlog(blogDto);

            if (!isSuccess)
            {
                return BadRequest(new { message = "Failed to create the blog" });
            }

            if (blogDto.Image != null && blogDto.Image.Length > 3 * 1024 * 1024)
            {
                return BadRequest(new { message = "Image size should be less than or equal to 3 MB" });
            }

            return Ok(new { message = "Blog created successfully" });
        }




        [HttpPut("{blogId}")]
        [Authorize(Roles = "Blogger")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateBlog(int blogId, [FromForm] BlogInsertDto blogDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var isSuccess = await _blogRepository.UpdateBlog(blogId, blogDto);

            if (!isSuccess)
            {
                return BadRequest(new { message = "Image size should be less than or equal to 3 MB" });
            }

            var blogExists = await _blogRepository.BlogExists(blogId);

            if (!blogExists)
            {
                return NotFound(new { message = "Blog not found with that ID" });
            }

            return Ok(new { message = "Blog updated successfully" });
        }



        [HttpDelete("{blogId}")]
        [Authorize(Roles = "Blogger")]
        public async Task<IActionResult> DeleteBlog(int blogId)
        {
            var isDeleted = await _blogRepository.DeleteBlog(blogId);

            if (isDeleted)
            {
                return NoContent();
            }
            else
            {
                return NotFound(new { message = "Blog not found" });
            }
        }


        // Blog Votes Endpoints Here

        [HttpPost("{blogId}/upvote")]
        [Authorize(Roles = "User, Blogger")]
        public async Task<IActionResult> UpvoteBlog(int blogId)
        {
            int userId = _blogRepository.GetCurrentUserId();
            var result = await _blogRepository.UpvoteBlog(blogId, userId);
            return Ok(result);
        }

        [HttpPost("{blogId}/downvote")]
        [Authorize(Roles = "User, Blogger")]
        public async Task<IActionResult> DownvoteBlog(int blogId)
        {
            int userId = _blogRepository.GetCurrentUserId();
            var result = await _blogRepository.DownvoteBlog(blogId, userId);
            return Ok(result);
        }

        [HttpGet("{blogId}/vote-status")]
        public async Task<IActionResult> GetVoteStatus(int blogId)
        {
            int userId = _blogRepository.GetCurrentUserId();  // Fetching userId from context
            var voteStatus = await _blogRepository.GetVoteStatus(blogId, userId);
            if (voteStatus == null)
            {
                return NotFound(new { message = "Vote status not found" });
            }
            return Ok(voteStatus);
        }


        [HttpGet("{blogId}/upvotes")]
        public async Task<IActionResult> GetUpvotesCount(int blogId)
        {
            var count = await _blogRepository.GetUpvotesCount(blogId);
            return Ok(new { Upvotes = count });
        }

        [HttpGet("{blogId}/downvotes")]
        public async Task<IActionResult> GetDownvotesCount(int blogId)
        {
            var count = await _blogRepository.GetDownvotesCount(blogId);
            return Ok(new { Downvotes = count });
        }

    }
}