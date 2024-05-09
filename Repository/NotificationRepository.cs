using Blog.Data;
using Blog.DTOs;
using Blog.Interfaces;
using Dapper;
using System.Security.Claims;

namespace Blog.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public NotificationRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
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
        public List<dynamic> GetNotificationData()
        {
            var data_to_Return= new List<dynamic>();
            var userid = GetUserIdFromContext();

            var query = @"
                    select top 6 * from 
                    (
                    select 
	                        _comment.CommentText as notification_text,
		                    _user_comm.username as notification_refernece,
		                    _comment.CreatedAt as created_at
		                    from dbo.Comments _comment
                    inner join dbo.Blogs _blog on _blog.BlogId=_comment.BlogId 
                    left join dbo.Users _user_comm on _user_comm.UserId=_comment.UserId
                    where _blog.UserId=@userid

                    union all 

                    select 
                            case when _comment.Upvotes=1 then 'Liked'
			                    when _comment.Downvotes=1 then 'Dislike'
		                     end as notification_text,
		                    _user_for_comm.username as notification_refernece,
		                    _comment.CreatedAt as created_at
		                    from dbo.BlogVotes _comment
                    inner join dbo.Blogs _blog on _blog.BlogId=_comment.BlogId 
                    left join dbo.Users _user_for_comm on _user_for_comm.UserId=_comment.UserId
                    where _blog.UserId=@userid
                   )x
                ";

            using (var connection = _context.CreateConnection())
            {
                 data_to_Return =  connection.Query<dynamic>(query, new
                {
                  userid=userid
                }).ToList();
            }
            return data_to_Return;
        }
    }
}
