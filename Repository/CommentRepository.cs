using Blog.Interfaces;
using Blog.Data;
using Blog.DTOs;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Blog.Models;
using System.Data.SqlClient;
using System.ComponentModel.Design;

namespace Blog.Repository
{
    public class CommentRepository : ICommentRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CommentRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetUserIdFromContext()
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (!_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            throw new ApplicationException("User ID claim not found in JWT token.");
        }

        public bool IsUserAuthenticated()
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            return _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated && userIdClaim != null;
        }

        public int GetCurrentUserId()
        {
            return GetUserIdFromContext();
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsByBlogId(int blogId)
        {
            var query = @"SELECT * FROM Comments WHERE BlogId = @BlogId";

            using (var connection = _context.CreateConnection())
            {
                var comments = await connection.QueryAsync<CommentDto>(query, new { BlogId = blogId });

                return comments.ToList();
            }
        }

        public async Task<CommentDto> GetCommentById(int commentId)
        {
            var query = @"SELECT * FROM Comments WHERE CommentId = @CommentId";

            using (var connection = _context.CreateConnection())
            {
                var comment = await connection.QueryFirstOrDefaultAsync<CommentDto>(query, new { CommentId = commentId });

                return comment;
            }
        }


        public async Task<bool> AddCommentToBlog(int blogId, CommentPostDto commentDto)
        {
            var userId = GetUserIdFromContext();

            var query = @"
                INSERT INTO Comments (BlogId, UserId, CommentText, CreatedAt)
                OUTPUT INSERTED.CommentId, INSERTED.BlogId, INSERTED.CommentText, INSERTED.CreatedAt
                VALUES (@BlogId, @UserId, @CommentText, @CreatedAt);";

            using (var connection = _context.CreateConnection())
            {
                var rowsAffected = await connection.ExecuteAsync(query, new
                {
                    BlogId = blogId,
                    UserId = userId,
                    CommentText = commentDto.CommentText,
                    CreatedAt = DateTime.Now
                });

                return rowsAffected > 0;
            }
        }

        public async Task<Comments> GetCommentByIdAsync(int commentId)
        {
            var query = "SELECT * FROM Comments WHERE CommentId = @CommentId";
            var parameters = new { CommentId = commentId };

            using (var connection = _context.CreateConnection())
            {
                var comment = await connection.QueryFirstOrDefaultAsync<Comments>(query, parameters);

                if (comment == null)
                {
                    throw new ApplicationException("Comment not found.");
                }

                return comment;
            }
        }

        public async Task<bool> CanUseCommentAsync(int commentId)
        {
            var comment = await GetCommentByIdAsync(commentId);
            var currentUserId = GetUserIdFromContext();

            if (_httpContextAccessor.HttpContext.User.IsInRole("Admin"))
            {
                return true;
            }

            return comment.UserId == currentUserId;
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsByBlogIdAsync(int blogId)
        {
            var query = "SELECT * FROM Comments WHERE BlogId = @BlogId";
            var parameters = new { BlogId = blogId };

            using (var connection = _context.CreateConnection())
            {
                var comments = await connection.QueryAsync<CommentDto>(query, parameters);
                return comments.ToList();
            }
        }

        public async Task<bool> CommentExists(int id)
        {
            var query = "SELECT COUNT(1) FROM Comments WHERE CommentId = @CommentId";

            using (var connection = _context.CreateConnection())
            {
                var count = await connection.ExecuteScalarAsync<int>(query, new { CommentId = id });

                return count > 0;
            }
        }


        public async Task<bool> UpdateComment(int commentId, CommentPutDto commentDto)
        {
            if (await CanUseCommentAsync(commentId))
            {
                var oldComment = await GetCommentById(commentId);

                var query = @"
                    UPDATE Comments
                    SET CommentText = @CommentText
                    WHERE CommentId = @CommentId;";

                using (var connection = _context.CreateConnection())
                {
                    var rowsAffected = await connection.ExecuteAsync(query, new
                    {
                        CommentText = commentDto.CommentText,
                        CommentId = commentId
                    });

                    if (rowsAffected > 0)
                    {
                        await SaveCommentRevision(commentId, oldComment);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                throw new UnauthorizedAccessException("User is not authorized to edit this comment.");
            }
        }


        private async Task SaveCommentRevision(int commentId, CommentDto oldComment)
        {
            try
            {
                var userId = GetUserIdFromContext();
                var CommentText = $"{oldComment.CommentText}";
                var blogId = oldComment.BlogId;

                var query = @"
                    INSERT INTO CommentRevisions (CommentText, UserId, BlogId, CreatedAt)
                    VALUES (@CommentText, @UserId, @BlogId, @CreatedAt);";

                using (var connection = _context.CreateConnection())
                {
                    var rowsAffected = await connection.ExecuteAsync(query, new
                    {
                        CommentText = CommentText,
                        UserId = userId,
                        BlogId = blogId,
                        CreatedAt = DateTime.UtcNow
                    });

                    if (rowsAffected != 1)
                    {
                        throw new Exception("Failed to save comment revision.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred : {ex.Message}");
                throw;
            }
        }


        public async Task<bool> DeleteComment(int commentId)
        {
            try
            {
                if (await CanUseCommentAsync(commentId))
                {
                    var query = @"DELETE FROM Comments WHERE CommentId = @CommentId";

                    using (var connection = _context.CreateConnection())
                    {
                        var rowsAffected = await connection.ExecuteAsync(query, new { CommentId = commentId });
                        return rowsAffected > 0;
                    }
                }
                else
                {
                    throw new UnauthorizedAccessException("User is not authorized to edit this comment.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpvoteComment(int commentId)
        {
            try
            {
                int userId = GetCurrentUserId();

                var query = @"
                    IF NOT EXISTS (SELECT 1 FROM CommentVotes WHERE CommentId = @CommentId AND UserId = @UserId)
                    BEGIN
                        INSERT INTO CommentVotes (CommentId, UserId, Upvotes, Downvotes)
                        VALUES (@CommentId, @UserId, 1, 0)
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

                using (var connection = _context.CreateConnection())
                {
                    var rowsAffected = await connection.ExecuteAsync(query, new { CommentId = commentId, UserId = userId });
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }


        public async Task<bool> DownvoteComment(int commentId)
        {
            try
            {
                int userId = GetCurrentUserId();

                var query = @"
                    IF NOT EXISTS (SELECT 1 FROM CommentVotes WHERE CommentId = @CommentId AND UserId = @UserId)
                    BEGIN
                        INSERT INTO CommentVotes (CommentId, UserId, Upvotes, Downvotes)
                        VALUES (@CommentId, @UserId, 0, 1)
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
                            IF (SELECT Upvotes FROM CommentVotes WHERE CommentId = @CommentId AND UserId = @UserId) = 1
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

                using (var connection = _context.CreateConnection())
                {
                    var rowsAffected = await connection.ExecuteAsync(query, new { CommentId = commentId, UserId = userId });
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }


        public async Task<VoteStatusDto> GetCommentVoteStatus(int commentId)
        {
            try
            {
                int userId = GetCurrentUserId();

                var query = @"
                SELECT Upvotes, Downvotes,
                CASE
                    WHEN UserId = @UserId AND Upvotes = 1 THEN 1
                    WHEN UserId = @UserId AND Downvotes = 1 THEN 0
                    ELSE NULL
                END AS UserVote
                FROM CommentVotes
                WHERE CommentId = @CommentId";

                using (var connection = _context.CreateConnection())
                {
                    var voteStatus = await connection.QueryFirstOrDefaultAsync<VoteStatusDto>(query, new { CommentId = commentId, UserId = userId });
                    return voteStatus;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        public async Task<int> GetUpvotesCountForComment(int commentId)
        {
            try
            {
                var query = "SELECT SUM(Upvotes) FROM CommentVotes WHERE CommentId = @CommentId";

                using (var connection = _context.CreateConnection())
                {
                    var count = await connection.ExecuteScalarAsync<int>(query, new { CommentId = commentId });
                    return count;
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> GetDownvotesCountForComment(int commentId)
        {
            try
            {
                var query = "SELECT SUM(Downvotes) FROM CommentVotes WHERE CommentId = @CommentId";

                using (var connection = _context.CreateConnection())
                {
                    var count = await connection.ExecuteScalarAsync<int>(query, new { CommentId = commentId });
                    return count;
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return 0;
            }
        }

    }
}
