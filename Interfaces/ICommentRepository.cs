using Blog.DTOs;

namespace Blog.Interfaces
{
    public interface ICommentRepository
    {
        Task<IEnumerable<CommentDto>> GetCommentsByBlogId(int blogId);
        Task<CommentDto> GetCommentById(int commentId);
        Task<bool> AddCommentToBlog(int blogId, CommentPostDto commentDto);
        Task<bool> UpdateComment(int commentId, CommentPutDto commentDto);
        Task<bool> DeleteComment(int CommentId);
        Task<bool> CommentExists(int id);
        Task<IEnumerable<CommentDto>> GetCommentsByBlogIdAsync(int blogId);
        bool IsUserAuthenticated();

        // Upvote and downvote
        Task<bool> UpvoteComment(int commentId);
        Task<bool> DownvoteComment(int commentId);
        Task<VoteStatusDto> GetCommentVoteStatus(int commentId);
        Task<int> GetUpvotesCountForComment(int commentId);
        Task<int> GetDownvotesCountForComment(int commentId);
    }
}
