using Blog.Data;
using Blog.Interfaces;
using Blog.Models;
using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

public class RevisionHistoryRepository : IRevisionHistoryRepository
{
    private readonly AppDbContext _context;

    public RevisionHistoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BlogRevision>> GetBlogRevisions(int blogId)
    {
        string query = "SELECT * FROM BlogRevisions WHERE BlogId = @BlogId ORDER BY CreatedAt DESC;";
        using (var connection = _context.CreateConnection())
        {
            return await connection.QueryAsync<BlogRevision>(query, new { BlogId = blogId });
        }
    }

    public async Task<BlogRevision> GetBlogRevision(int blogRevisionId)
    {
        string query = "SELECT * FROM BlogRevisions WHERE RevisionId = @RevisionId;";
        using (var connection = _context.CreateConnection())
        {
            return await connection.QueryFirstOrDefaultAsync<BlogRevision>(query, new { BlogRevisionId = blogRevisionId });
        }
    }

    public async Task<IEnumerable<CommentRevision>> GetCommentRevisions(int blogId)
    {
        string query = "SELECT * FROM CommentRevisions WHERE BlogId = @BlogId ORDER BY CreatedAt DESC;";
        using (var connection = _context.CreateConnection())
        {
            return await connection.QueryAsync<CommentRevision>(query, new { BlogId = blogId });
        }
    }

    public async Task<CommentRevision> GetCommentRevision(int commentRevisionId)
    {
        string query = "SELECT * FROM CommentRevisions WHERE CommentRevisionId = @CommentRevisionId;";
        using (var connection = _context.CreateConnection())
        {
            return await connection.QueryFirstOrDefaultAsync<CommentRevision>(query, new { CommentRevisionId = commentRevisionId });
        }
    }
}
