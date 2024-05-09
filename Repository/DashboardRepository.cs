using Azure;
using Blog.Data;
using Blog.Interfaces;
using Blog.Models;
using Dapper;
using System.Security.Claims;

namespace Blog.Repository
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DashboardRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public IDictionary<string, object> GetDashboardData()
        {

            var data= new Dictionary<string, object>();

            var user_id = GetUserIdFromContext();

            var query = @"
                        select UserName from dbo.users where UserId=@user_id;
                        select count(*) as blogCount from dbo.Blogs where UserID=@user_id;
                        select count(*) as blogCommentCount from dbo.Comments where UserId=@user_id;
                        select count(Upvotes) as liked, count(downvotes) as dislike  from dbo.BlogVotes where UserId=@user_id;    
                        
                        SELECT  _blog.BlogId, 
		                        _blog.BlogTitle, 
		                     --   _blog.BlogContent,
		                        _blog.createdAt,
		                        coalesce(_votes.Upvotes,0) as blog_like,
		                        coalesce(_votes.Downvotes,0) as blog_dislike
                        FROM Blogs _blog
                        left join (select sum(Upvotes) as upVotes, sum(Downvotes) as downvotes , BlogId 
                        from  dbo.BlogVotes group by BlogId)_votes on _votes.BlogId=_blog.BlogId
                       where _blog.UserId=@user_id;";
            try
            {

                using (var connection = _context.CreateConnection())
                {
                    using (var multi =  connection.QueryMultiple(query, new { user_id = user_id }))
                    {

                        var username = multi.Read<dynamic>().ToList();
                        var total_blogs = multi.Read<dynamic>().ToList();
                        var total_comments = multi.Read<dynamic>().ToList();
                        var like_dislike = multi.Read<dynamic>().ToList();
                        var blog_Data=multi.Read<dynamic>().ToList();

                        var like = like_dislike[0].liked;
                        var dislike = like_dislike[0].dislike;
                       
                        data.Add("username", username[0].UserName);
                        data.Add("total_blogs", total_blogs[0].blogCount);
                        data.Add("total_comments", total_comments[0].blogCommentCount);
                        data.Add("total_like", like);
                        data.Add("total_dislike", dislike);
                        data.Add("blogData", blog_Data);

                    }
                }
            }
            catch (Exception ex)
            {

            }
            return data ;
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
    }
}
