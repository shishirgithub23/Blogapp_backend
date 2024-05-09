using Blog.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Controllers
{
    [ApiController]
    [Route("api/revisions")]
    public class RevisionHistoryController : ControllerBase
    {
        private readonly IRevisionHistoryRepository _revisionHistoryRepository;

        public RevisionHistoryController(IRevisionHistoryRepository revisionHistoryRepository)
        {
            _revisionHistoryRepository = revisionHistoryRepository;
        }

        [HttpGet("blogs/{blogId}/revisions")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBlogRevisions(int blogId)
        {
            var revisions = await _revisionHistoryRepository.GetBlogRevisions(blogId);
            return Ok(revisions);
        }

        [HttpGet("blogs/revisions/{blogRevisionId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBlogRevision(int blogRevisionId)
        {
            var revision = await _revisionHistoryRepository.GetBlogRevision(blogRevisionId);
            if (revision == null)
            {
                return NotFound(new { message = "Blog revision not found." });
            }
            return Ok(revision);
        }

        [HttpGet("comments/{blogId}/revisions")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCommentRevisions(int blogId)
        {
            var revisions = await _revisionHistoryRepository.GetCommentRevisions(blogId);
            return Ok(revisions);
        }

        [HttpGet("comments/revisions/{commentRevisionId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCommentRevision(int commentRevisionId)
        {
            var revision = await _revisionHistoryRepository.GetCommentRevision(commentRevisionId);
            if (revision == null)
            {
                return NotFound(new { message = "Comment revision not found." });
            }
            return Ok(revision);
        }
    }

}
