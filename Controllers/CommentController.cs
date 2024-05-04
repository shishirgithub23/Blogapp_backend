using Blog.DTOs;
using Blog.Interfaces;
using Blog.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Controllers
{
    [Route("api/v1/comments")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentRepository _commentRepository;

        public CommentController(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        [HttpGet("blogs/{blogId}")]
        public async Task<IActionResult> GetCommentsByBlogId(int blogId)
        {
            var comments = await _commentRepository.GetCommentsByBlogId(blogId);

            if (comments == null || !comments.Any())
            {
                return NotFound(new { message = "Comments not found for this blog" });
            }

            return Ok(comments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommentById(int id)
        {
            var comment = await _commentRepository.GetCommentById(id);

            if (comment == null)
            {
                return NotFound(new { message = "Comment not found" });
            }

            return Ok(comment);
        }

        [HttpPost("blogs/{blogId}")]
        [Authorize(Roles = "Blogger")]
        public async Task<IActionResult> AddCommentToBlog(int blogId, [FromBody] CommentPostDto commentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var isSuccess = await _commentRepository.AddCommentToBlog(blogId, commentDto);

            if (!isSuccess)
            {
                return StatusCode(500, new { message = "Failed to add comment to blog" });
            }

            return Ok(new { message = "Comment added successfully" });
        }


        [HttpPut("{commentId}")]
        [Authorize(Roles = "Blogger")]
        public async Task<IActionResult> UpdateComment(int commentId, [FromBody] CommentPutDto commentDto)
        {
            try
            {
                var isUpdated = await _commentRepository.UpdateComment(commentId, commentDto);
                if (isUpdated)
                {
                    return Ok(new { message = "Comment updated successfully." });
                }
                return NotFound(new { message = "Comment not found or user is not authorized." });
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, new { message = "User is not authorized to edit this comment." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }




        [HttpDelete("{id}")]
        [Authorize(Roles = "Blogger")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var isSuccess = await _commentRepository.DeleteComment(id);

            if (isSuccess)
            {
                return NoContent();
            }
            else
            {
                return NotFound(new { message = "Comment not found or could not be deleted" });
            }
        }

        // Upvotes and Downvotes for comments

        [HttpPost("{commentId}/upvote")]
        [Authorize(Roles = "User, Blogger")]
        public async Task<IActionResult> UpvoteComment(int commentId)
        {
            if (!_commentRepository.IsUserAuthenticated())
            {
                return Unauthorized(new { message = "User is not authenticated." });
            }

            var isSuccess = await _commentRepository.UpvoteComment(commentId);
            if (isSuccess)
            {
                return Ok(new { message = "Comment upvoted successfully" });
            }
            return BadRequest(new { message = "Failed to upvote comment" });
        }


        [HttpPost("{commentId}/downvote")]
        [Authorize(Roles = "User, Blogger")]
        public async Task<IActionResult> DownvoteComment(int commentId)
        {
            var isSuccess = await _commentRepository.DownvoteComment(commentId);
            if (isSuccess)
            {
                return Ok(new { message = "Comment downvoted successfully" });
            }
            return BadRequest(new { message = "Failed to downvote comment" });
        }

        [HttpGet("{commentId}/vote/status")]
        [Authorize(Roles = "User, Blogger")]
        public async Task<IActionResult> GetCommentVoteStatus(int commentId)
        {
            var voteStatus = await _commentRepository.GetCommentVoteStatus(commentId);
            if (voteStatus != null)
            {
                return Ok(voteStatus);
            }
            return NotFound(new { message = "Vote status not found" });
        }

        [HttpGet("{commentId}/upvotes")]
        [Authorize(Roles = "User, Blogger")]
        public async Task<IActionResult> GetUpvotesCountForComment(int commentId)
        {
            var count = await _commentRepository.GetUpvotesCountForComment(commentId);
            if (count == 0)
            {
                return NotFound(new { message = "No upvotes found for the specified comment." });
            }
            return Ok(new { upvotes = count });
        }

        [HttpGet("{commentId}/downvotes")]
        [Authorize(Roles = "User, Blogger")]
        public async Task<IActionResult> GetDownvotesCountForComment(int commentId)
        {
            var count = await _commentRepository.GetDownvotesCountForComment(commentId);
            if (count == 0)
            {
                return NotFound(new { message = "No downvotes found for the specified comment." });
            }
            return Ok(new { downvotes = count });
        }
    }
}
