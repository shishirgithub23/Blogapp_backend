namespace Blog.DTOs
{
    public class BlogDto
    {
        public string BlogTitle { get; set; }
        public string BlogContent { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public IFormFile Image { get; set; }
    }

    public class UpdateBlogDTO
    {
        public int BlogId{get;set; }
        public string BlogTitle { get; set; }
        public string BlogContent { get; set; }
        public int CategoryId { get; set; }
      //  public string? ImageName { get; set; }
        public IFormFile? Image { get; set; }
    }

    public class BlogInsertDto
    {
        public string BlogTitle { get; set; }
        public string BlogContent { get; set; }
        public int CategoryId { get; set; }
        public IFormFile Image { get; set; }
    }

    public class BlogDisplayDto
    {
        public int BlogId { get; set; }
        public string BlogTitle { get; set; }
        public string BlogContent { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public string Image { get; set; }
        public List<CommentsData> comments{get; set; }
        public DateTime createdAt { get; set; }

        public int blog_like { get; set; }
        public int blog_dislike { get; set; }
        public string  UserName { get; set; }
        public string CategoryName { get; set; }

        public int CommentId { get; set; }
        public string CommentText { get; set; }
        public int CommentLike { get; set; }
        public int CommnetDislike { get; set; }
        public DateTime CommentCreatedAt { get; set; }
        public string Comment_UserName { get; set; }

    }

    public class VoteStatusDto
    {
        public int Upvotes { get; set; }
        public int Downvotes { get; set; }
    }


    public class CommentsData
    {
        public int CommentId { get; set; }
        public string CommentText { get; set; }

        public int CommentLike { get; set; }
        public int CommnetDislike {get;set; }

        public DateTime CommentCreatedAt { get; set; }
        public string Comment_UserName { get; set; }

    }

}
