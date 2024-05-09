namespace Blog.DTOs
{
    public class CommentDto
    {
        public int CommentId { get; set; }
        public int BlogId { get; set; }
        public string CommentText { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class CommentPostDto
    {
        public int blogId { get; set; }
        public string CommentText { get; set; }
    }

    public class CommentPutDto
    {
        public string CommentText { get; set; }

    }

    public class CommentVoteStatusDto
    {
        public int Upvotes { get; set; }
        public int Downvotes { get; set; }
    }
}
