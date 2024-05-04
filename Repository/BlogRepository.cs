using Blog.Data;
using Blog.DTOs;
using Blog.Interfaces;
using Blog.Models;
using Dapper;
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
            var query = @"SELECT BlogId, BlogTitle, BlogContent, UserId, Image, CategoryId FROM Blogs";

            using (var connection = _context.CreateConnection())
            {
                var blogs = await connection.QueryAsync<BlogDisplayDto>(query);

                return blogs.ToList();
            }
        }

        public async Task<BlogDisplayDto> GetBlogById(int id)
        {
            var query = @"SELECT BlogId, BlogTitle, BlogContent, UserId, Image, CategoryId FROM Blogs WHERE BlogId = @BlogId";

            using (var connection = _context.CreateConnection())
            {
                var blog = await connection.QueryFirstOrDefaultAsync<BlogDisplayDto>(query, new { BlogId = id });

                return blog;
            }
        }

        public async Task<bool> CreateBlog(BlogInsertDto blogDto)
        {
            string imagePath = null;
            try
            {
                var userId = GetUserIdFromContext();

                if (blogDto.Image != null && blogDto.Image.Length > 0 && blogDto.Image.Length <= 3 * 1024 * 1024)
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
                    INSERT INTO Blogs (BlogTitle, BlogContent, UserId, Image, CategoryId)
                    VALUES (@BlogTitle, @BlogContent, @UserId, @Image, @CategoryId);";

                using (var connection = _context.CreateConnection())
                {
                    var rowsAffected = await connection.ExecuteAsync(query, new
                    {
                        BlogTitle = blogDto.BlogTitle,
                        BlogContent = blogDto.BlogContent,
                        UserId = userId,
                        Image = imagePath,
                        CategoryId = blogDto.CategoryId
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

        public async Task<bool> UpdateBlog(int blogId, BlogInsertDto blogDto)
        {
            try
            {
                var userId = GetUserIdFromContext();
                var oldBlog = await GetBlogById(blogId);
                string imagePath = oldBlog.Image;

                if (blogDto.Image != null && blogDto.Image.Length > 0 && blogDto.Image.Length <= 3 * 1024 * 1024)
                {
                    var imageName = Guid.NewGuid().ToString() + Path.GetExtension(blogDto.Image.FileName);
                    imagePath = Path.Combine("Uploads", imageName);

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await blogDto.Image.CopyToAsync(fileStream);
                    }

                    if (!string.IsNullOrEmpty(oldBlog.Image) && oldBlog.Image != imagePath)
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldBlog.Image);
                        if (File.Exists(oldImagePath))
                        {
                            File.Delete(oldImagePath);
                        }
                    }
                }
                else if (blogDto.Image != null)
                {
                    return false;
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
                        BlogId = blogId,
                        CategoryId = blogDto.CategoryId,
                    });

                    if (rowsAffected > 0)
                    {
                        await SaveBlogRevision(blogId, oldBlog);
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
            using (var connection = _context.CreateConnection())
            {
                var deleteCommentsQuery = @"DELETE FROM Comments WHERE BlogId = @BlogId;";
                await connection.ExecuteAsync(deleteCommentsQuery, new { BlogId = blogId });

                var deleteBlogQuery = @"DELETE FROM Blogs WHERE BlogId = @BlogId;";
                var rowsAffected = await connection.ExecuteAsync(deleteBlogQuery, new { BlogId = blogId });

                return rowsAffected > 0;
            }
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

    }
}