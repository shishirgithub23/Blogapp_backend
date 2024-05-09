using Blog.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blog.Interfaces
{
    public interface IBlogRepository
    {
        Task<IEnumerable<BlogDisplayDto>> GetBlogs();
        Task<List<BlogDisplayDto>> GetBlogById(int blogId);
        Task<bool> CreateBlog(BlogInsertDto blogDto);
        Task<bool> UpdateBlog(UpdateBlogDTO blogDto);
        Task<bool> DeleteBlog(int blogId);
        Task<bool> BlogExists(int blogId);

        // For Upvote and Downvote
        Task<bool> UpvoteBlog(int blogId, int userId);
        Task<bool> DownvoteBlog(int blogId, int userId);
        Task<VoteStatusDto> GetVoteStatus(int blogId, int userId);
        Task<int> GetUpvotesCount(int blogId);
        Task<int> GetDownvotesCount(int blogId);
        
        int GetCurrentUserId();

        Dictionary<string, object> CategoryDropDown();

       List<dynamic> GetBlogInfoByCategory();
       List<dynamic> GetRecentBlogPost();
    }
}