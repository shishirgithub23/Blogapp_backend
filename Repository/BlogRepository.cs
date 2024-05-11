using Blog.Data;
using Blog.DTOs;
using Blog.Interfaces;
using Blog.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Security.Claims;

namespace Blog.Repository
{
    public class BlogRepository : IBlogRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public BlogRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor)
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

        public int GetCurrentUserId()
        {
            return GetUserIdFromContext();
        }

        public async Task<IEnumerable<BlogDisplayDto>> GetBlogs()
        {
            var query = @"SELECT _blog.BlogId, 
		 _blog.BlogTitle, 
		 _blog.BlogContent,
		 _blog.UserId, 
		 _blog.Image, 
		 _blog.CategoryId,
		 _blog.createdAt,
		 _comm.CommentText,
         _comm.CommentId,
		 coalesce(_votes.Upvotes,0) as blog_like,
		 coalesce(_votes.Downvotes,0) as blog_dislike,
		 _user.UserName,
        _category.CategoryName
	FROM Blogs _blog
  left join dbo.Comments _comm on _comm.BlogId= _blog.BlogId
  left join (select sum(Upvotes) as upVotes, sum(Downvotes) as downvotes , BlogId 
  
	from  dbo.BlogVotes group by BlogId)_votes on _votes.BlogId=_blog.BlogId
	inner join dbo.Users _user on _user.userId=_blog.UserId
    inner join dbo.Category _category on _category.CategoryId=_blog.CategoryId
order by _blog.CreatedAt desc
";

            using (var connection = _context.CreateConnection())
            {
                var blogs = await connection.QueryAsync<BlogDisplayDto>(query);

                var grouped_data = blogs.GroupBy(x => new 
                {
                    x.BlogId,
                    x.BlogTitle,
                    x.BlogContent,
                    x.UserId,
                    x.Image,
                    x.CategoryId,
                    x.createdAt,
                    x.blog_like,
                    x.blog_dislike,
                    x.UserName,
                    x.CategoryName

                }).Select(y => new BlogDisplayDto
                {
                    BlogId=y.Key.BlogId,
                    BlogTitle = y.Key.BlogTitle,
                    BlogContent = y.Key.BlogContent,
                    UserId = y.Key.UserId,
                    Image = y.Key.Image,
                    CategoryId = y.Key.CategoryId,
                    createdAt = y.Key.createdAt,
                    blog_like = y.Key.blog_like,
                    blog_dislike = y.Key.blog_dislike,
                    UserName = y.Key.UserName,
                    CategoryName = y.Key.CategoryName,
                    comments = y.Select(z => new CommentsData { CommentId=z.CommentId,CommentText=z.CommentText }).ToList()    
                }).ToList();
                return grouped_data;
            }
        }

        public async Task<List<BlogDisplayDto>> GetBlogById(int id)
        {
            var query = @"SELECT _blog.BlogId, 
		         _blog.BlogTitle, 
		         _blog.BlogContent,
		         _blog.UserId, 
		         _blog.Image, 
		         _blog.CategoryId,
		         _blog.createdAt,

		         coalesce(_comm.CommentText,'') as CommentText,
                 coalesce(_comm.CommentId,0) as CommentId ,
                 coalesce(_comm.createdAt,0) as CommentCreatedAt ,
                 

                 coalesce(cm_v.comment_like,0) as CommentLike,
	             coalesce(cm_v.comment_dislike,0) as CommnetDislike,
                 --coalesce(cm_v.createdAt,'') as Comment_CreatedAt,
                 coalesce(cm_v.username ,'' ) as Comment_UserName,

		         coalesce(_votes.Upvotes,0) as blog_like,
		         coalesce(_votes.Downvotes,0) as blog_dislike,
		         _user.UserName,
                _category.CategoryName
	        FROM Blogs _blog
          left join dbo.Comments _comm on _comm.BlogId= _blog.BlogId
          left join (select sum(Upvotes) as upVotes, sum(Downvotes) as downvotes , BlogId 
  
	        from  dbo.BlogVotes group by BlogId)_votes on _votes.BlogId=_blog.BlogId
	        inner join dbo.Users _user on _user.userId=_blog.UserId
                inner join dbo.Category _category on _category.CategoryId=_blog.CategoryId
            left join (
                        select 
                            sum(_cm_v.UpVotes) as comment_like , 
                            sum(_cm_v.DownVotes) as comment_dislike ,
                            _cm_v.commentId,
                            _user.username,
                            _cm_v.createdAt
                            
                         from dbo.commentvotes  _cm_v
                        inner join dbo.users _user on _user.UserId =_cm_v.UserId

                        group by commentId , _user.username,_cm_v.CreatedAt

                )cm_v 

		    on cm_v.commentId=_comm.CommentId 

        WHERE _blog.BlogId = @BlogId
        ";

            using (var connection = _context.CreateConnection())
            {
                var blog =  connection.Query<BlogDisplayDto>(query, new { BlogId = id }).ToList();

                var grouped_data = blog.GroupBy(x => new
                {
                    x.BlogId,
                    x.BlogTitle,
                    x.BlogContent,
                    x.UserId,
                    x.Image,
                    x.CategoryId,
                    x.createdAt,
                    x.blog_like,
                    x.blog_dislike,
                    x.UserName,
                    x.CategoryName

                }).Select(y => new BlogDisplayDto
                {
                    BlogId=y.Key.BlogId,
                    BlogTitle = y.Key.BlogTitle,
                    BlogContent = y.Key.BlogContent,
                    UserId = y.Key.UserId,
                    Image = y.Key.Image,
                    CategoryId = y.Key.CategoryId,
                    createdAt = y.Key.createdAt,
                    blog_like = y.Key.blog_like,
                    blog_dislike = y.Key.blog_dislike,
                    comment_count=y.Count(x=>x.CommentId>0),
                    UserName = y.Key.UserName,
                    CategoryName = y.Key.CategoryName,
                    comments = y.Select(z => new CommentsData { CommentId = z.CommentId, 
                                            CommentText = z.CommentText ,
                                            CommentLike=z.CommentLike, 
                                            CommnetDislike=z.CommnetDislike ,
                                            CommentCreatedAt = z.CommentCreatedAt, 
                                            Comment_UserName=z.Comment_UserName 
                    }).ToList().Where(x=>x.CommentId>0).ToList()
                }).ToList();

                return grouped_data;
            }
        }

        public async Task<bool> CreateBlog(BlogInsertDto blogDto)
        {
            string imagePath = null;
            try
            {
                var userId = GetUserIdFromContext();

                if (blogDto.Image != null && blogDto.Image.Length > 0 && blogDto.Image.Length <= 5 * 1024 * 1024)
                {
                    var imageName = Guid.NewGuid().ToString() + Path.GetExtension(blogDto.Image.FileName);
                    imagePath = Path.Combine("Uploads", imageName);

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await blogDto.Image.CopyToAsync(fileStream);
                    }
                }
                else if (blogDto.Image != null)
                {
                    return false;
                }

                var query = @"
                    INSERT INTO Blogs (BlogTitle, BlogContent, UserId, Image, CategoryId,CreatedAt)
                    VALUES (@BlogTitle, @BlogContent, @UserId, @Image, @CategoryId,@CreatedAt);
                ";

                using (var connection = _context.CreateConnection())
                {
                    var rowsAffected = await connection.ExecuteAsync(query, new
                    {
                        BlogTitle = blogDto.BlogTitle,
                        BlogContent = blogDto.BlogContent,
                        UserId = userId,
                        Image = imagePath,
                        CategoryId = blogDto.CategoryId,
                        CreatedAt=DateTime.Now
                    });

                    return rowsAffected > 0;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }


        public async Task<bool> BlogExists(int blogId)
        {
            var query = "SELECT COUNT(1) FROM Blogs WHERE BlogId = @BlogId";

            using (var connection = _context.CreateConnection())
            {
                var count = await connection.ExecuteScalarAsync<int>(query, new { BlogId = blogId });

                return count > 0;
            }
        }

        public async Task<bool> UpdateBlog(UpdateBlogDTO blogDto)
        {
            try
            {
                var userId = GetUserIdFromContext();
                var oldBlog = await GetBlogById(blogDto.BlogId);
                string imagePath = oldBlog[0].Image;

                if (blogDto.Image != null && blogDto.Image.Length > 0 && blogDto.Image.Length <= 3 * 1024 * 1024)
                {
                    var imageName = Guid.NewGuid().ToString() + Path.GetExtension(blogDto.Image.FileName);
                    imagePath = Path.Combine("Uploads", imageName);

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await blogDto.Image.CopyToAsync(fileStream);
                    }

                    if (!string.IsNullOrEmpty(oldBlog[0].Image) && oldBlog[0].Image != imagePath)
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldBlog[0].Image);
                        if (File.Exists(oldImagePath))
                        {
                            File.Delete(oldImagePath);
                        }
                    }
                }
                else if (blogDto.Image != null)
                {
                    throw new Exception("Please Select Image");
                }

                var query = @"
                    UPDATE Blogs 
                    SET BlogTitle = @BlogTitle, BlogContent = @BlogContent, UserId = @UserId, Image = @Image, CategoryId = @CategoryId
                    WHERE BlogId = @BlogId;";

                using (var connection = _context.CreateConnection())
                {
                    var rowsAffected = await connection.ExecuteAsync(query, new
                    {
                        BlogTitle = blogDto.BlogTitle,
                        BlogContent = blogDto.BlogContent,
                        UserId = userId,
                        Image = imagePath,
                        BlogId = blogDto.BlogId,
                        CategoryId = blogDto.CategoryId,
                    });

                    if (rowsAffected > 0)
                    {
                        await SaveBlogRevision(blogDto.BlogId, oldBlog[0]);
                    }
                }

                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("Unauthorized: " + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }




        private async Task SaveBlogRevision(int blogId, BlogDisplayDto oldBlog)
        {
            try
            {
                var userId = GetUserIdFromContext();
                var BlogTitle = $"{oldBlog.BlogTitle}";
                var BlogContent = $"{oldBlog.BlogContent}";

                var query = @"
                    INSERT INTO BlogRevisions (BlogId, BlogTitle, BlogContent, UserId, CreatedAt)
                    VALUES (@BlogId, @BlogTitle, @BlogContent, @UserId, @CreatedAt);";

                using (var connection = _context.CreateConnection())
                {
                    var rowsAffected = await connection.ExecuteAsync(query, new
                    {
                        BlogId = blogId,
                        BlogTitle = BlogTitle,
                        BlogContent = BlogContent,
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public async Task<bool> DeleteBlog(int blogId)
        {
            var return_data = false;
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var deletefromRevision = @"DELETE FROM dbo.BlogRevisions WHERE BlogId = @BlogId;";
                    var deletefromRevision_ = await connection.ExecuteAsync(deletefromRevision, new { BlogId = blogId });

                    var deletefromCommentVote = @"DELETE FROM dbo.CommentVotes
                                                    WHERE commentId IN (
                                                        SELECT _comm.commentId
                                                        FROM Comments _comm
                                                        WHERE _comm.blogId = @BlogId
                                                    )";
                    var deletefromcommentVote_ = await connection.ExecuteAsync(deletefromCommentVote, new { BlogId = blogId });

                    var deletefromCommentRevision = @"DELETE FROM dbo.CommentRevisions WHERE BlogId = @BlogId;";
                    var deletefromCommentRevision_ = await connection.ExecuteAsync(deletefromCommentRevision, new { BlogId = blogId });


                    var deleteCommentsQuery = @"DELETE FROM Comments WHERE BlogId = @BlogId;";
                    await connection.ExecuteAsync(deleteCommentsQuery, new { BlogId = blogId });

                    var deleteFromBlogvotes = @"Delete from dbo.blogVotes where BlogId=@BlogId;";
                    await connection.ExecuteAsync(deleteFromBlogvotes, new { BlogId = blogId });

                    var deleteBlogQuery = @"DELETE FROM Blogs WHERE BlogId = @BlogId;";
                    var rowsAffected = await connection.ExecuteAsync(deleteBlogQuery, new { BlogId = blogId });
                    if (rowsAffected > 0)
                    {
                        return_data = true;
                    }

               
                }
            }
            catch (Exception ex)
            {

            }
             return return_data;
        }

        // Blog Votes From Here

        public async Task<bool> UpvoteBlog(int blogId, int userId)
        {
            var query = @"
                IF NOT EXISTS (SELECT 1 FROM BlogVotes WHERE BlogId = @BlogId AND UserId = @UserId)
                BEGIN
                    INSERT INTO BlogVotes (BlogId, UserId, Upvotes, Downvotes)
                    VALUES (@BlogId, @UserId, 1, 0)
                END
                ELSE
                BEGIN
                    IF (SELECT Upvotes FROM BlogVotes WHERE BlogId = @BlogId AND UserId = @UserId) = 1
                    BEGIN
                        UPDATE BlogVotes 
                        SET Upvotes = 0
                        WHERE BlogId = @BlogId AND UserId = @UserId
                    END
                    ELSE
                    BEGIN
                        UPDATE BlogVotes 
                        SET Upvotes = 1, Downvotes = 0
                        WHERE BlogId = @BlogId AND UserId = @UserId
                    END
                END";

            using (var connection = _context.CreateConnection())
            {
                var rowsAffected = await connection.ExecuteAsync(query, new { BlogId = blogId, UserId = userId });
                return rowsAffected > 0;
            }
        }


        public async Task<bool> DownvoteBlog(int blogId, int userId)
        {
            var query = @"
                IF NOT EXISTS (SELECT 1 FROM BlogVotes WHERE BlogId = @BlogId AND UserId = @UserId)
                BEGIN
                    INSERT INTO BlogVotes (BlogId, UserId, Upvotes, Downvotes)
                    VALUES (@BlogId, @UserId, 0, 1)
                END
                ELSE
                BEGIN
                    IF (SELECT Downvotes FROM BlogVotes WHERE BlogId = @BlogId AND UserId = @UserId) = 1
                    BEGIN
                        UPDATE BlogVotes 
                        SET Downvotes = 0
                        WHERE BlogId = @BlogId AND UserId = @UserId
                    END
                    ELSE
                    BEGIN
                        IF (SELECT Upvotes FROM BlogVotes WHERE BlogId = @BlogId AND UserId = @UserId) = 1
                        BEGIN
                            UPDATE BlogVotes 
                            SET Upvotes = 0, Downvotes = 1
                            WHERE BlogId = @BlogId AND UserId = @UserId
                        END
                        ELSE
                        BEGIN
                            UPDATE BlogVotes
                            SET Downvotes = 1
                            WHERE BlogId = @BlogId AND UserId = @UserId
                        END
                    END
                END";

            using (var connection = _context.CreateConnection())
            {
                var rowsAffected = await connection.ExecuteAsync(query, new { BlogId = blogId, UserId = userId });
                return rowsAffected > 0;
            }
        }


        public async Task<VoteStatusDto> GetVoteStatus(int blogId, int userId)
        {
            var query = @"
                SELECT Upvotes, Downvotes,
                CASE
                    WHEN UserId = @UserId AND Upvotes = 1 THEN 1
                    WHEN UserId = @UserId AND Downvotes = 1 THEN 0
                    ELSE NULL
                END AS UserVote
                FROM BlogVotes
                WHERE BlogId = @BlogId";

            using (var connection = _context.CreateConnection())
            {
                var voteStatus = await connection.QueryFirstOrDefaultAsync<VoteStatusDto>(query, new { BlogId = blogId, UserId = userId });
                return voteStatus;
            }
        }

        public async Task<int> GetUpvotesCount(int blogId)
        {
            var query = "SELECT SUM(Upvotes) FROM BlogVotes WHERE BlogId = @BlogId";

            using (var connection = _context.CreateConnection())
            {
                var count = await connection.ExecuteScalarAsync<int>(query, new { BlogId = blogId });
                return count;
            }
        }

        public async Task<int> GetDownvotesCount(int blogId)
        {
            var query = "SELECT SUM(Downvotes) FROM BlogVotes WHERE BlogId = @BlogId";

            using (var connection = _context.CreateConnection())
            {
                var count = await connection.ExecuteScalarAsync<int>(query, new { BlogId = blogId });
                return count;
            }
        }

        public  Dictionary<string, object> CategoryDropDown()
        {
            var data= new Dictionary<string, object>();
            var query = "select Categoryid as value , CategoryName as label from dbo.Category ";
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var count =  connection.Query<dynamic>(query, new {});
                    data.Add("categories", count);
                }
            }
            catch (Exception ex)
            {
            }
            return data;
        }

        public  List<dynamic> GetBlogInfoByCategory()
        {

            var data = new List<dynamic>();

            var query = @" select _cat.CategoryName as name, 
                                  count(_blog.BlogId) as postNumber,
                                 _cat.categoryId  
                        from Blogs _blog
                         inner join dbo.Category _cat on _cat.CategoryId=_blog.CategoryId
                          group by _cat.CategoryName,_cat.categoryId";
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    data =  connection.Query<dynamic>(query, new { }).ToList();
                }
            }
            catch (Exception ex)
            {
        
            }
            return data;
        }

        public  List<dynamic> GetRecentBlogPost()
        {
            var query = @"SELECT top 10
                  _blog.BlogId, 
		         _blog.BlogTitle as title, 
		         _blog.Image as image, 
		         _blog.createdAt as date,
                 _blog.CategoryId
	        FROM Blogs _blog
         order by _blog.createdAt desc
        ";

            using (var connection = _context.CreateConnection())
            {
                var blogs =  connection.Query<dynamic>(query).ToList();
                return blogs;
            }
        }

        public async Task<bool> UpvoteBlogComment(int commentid)
        {
            var userid = GetUserIdFromContext();

            bool rowsAffected_res = false;
            var query = @"
                 IF NOT EXISTS (SELECT 1 FROM CommentVotes WHERE CommentId = @CommentId AND UserId = @UserId)
                BEGIN
                    INSERT INTO CommentVotes (UserId,CommentId, Upvotes, Downvotes,CreatedAt)
                    VALUES (@UserId, @CommentId, 1, 0,@CreatedAt)
                END
                ELSE
                BEGIN
                    IF (SELECT Upvotes FROM CommentVotes WHERE CommentId = @CommentId AND UserId = @UserId) = 1
                    BEGIN
                        UPDATE CommentVotes 
                        SET Upvotes = 0
                        WHERE CommentId = @CommentId AND UserId = @UserId
                    END
                    ELSE
                    BEGIN
                        UPDATE CommentVotes 
                        SET Upvotes = 1, Downvotes = 0
                        WHERE CommentId = @CommentId AND UserId = @UserId
                    END
                END";
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    int rowsAffected = await connection.ExecuteAsync(query, new { CommentId = commentid, UserId = userid, CreatedAt = DateTime.Now });

                    rowsAffected_res= rowsAffected > 0;
                   
                }
            }
            catch (Exception ex)
            {

            }
           return rowsAffected_res;
        }

        public async Task<bool> DownvoteBlogComment(int commentid)
        {
            var userid = GetUserIdFromContext();
            bool rowsAffected_res = false;

            var query = @"
                IF NOT EXISTS (SELECT 1 FROM CommentVotes WHERE CommentId = @CommentId AND UserId = @UserId)
                BEGIN
                    INSERT INTO CommentVotes (UserId, CommentId , Upvotes, Downvotes,CreatedAt)
                    VALUES (@UserId, @CommentId, 0, 1,@CreatedAt)
                END
                ELSE
                BEGIN
                    IF (SELECT Downvotes FROM CommentVotes WHERE CommentId = @CommentId AND UserId = @UserId) = 1
                    BEGIN
                        UPDATE CommentVotes 
                        SET Downvotes = 0
                        WHERE CommentId = @CommentId AND UserId = @UserId
                    END
                    ELSE
                    BEGIN
                        IF (SELECT Upvotes FROM CommentVotes WHERE  CommentId = @CommentId AND UserId = @UserId) = 1
                        BEGIN
                            UPDATE CommentVotes 
                            SET Upvotes = 0, Downvotes = 1
                            WHERE CommentId = @CommentId AND UserId = @UserId
                        END
                        ELSE
                        BEGIN
                            UPDATE CommentVotes
                            SET Downvotes = 1
                            WHERE CommentId = @CommentId AND UserId = @UserId
                        END
                    END
                END";
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var rowsAffected = await connection.ExecuteAsync(query, new { CommentId = commentid, UserId = userid, CreatedAt = DateTime.Now });
                    rowsAffected_res= rowsAffected > 0;
                    
                }
            }
            catch (Exception ex)
            {

            }
           return rowsAffected_res;
        }
    }
}