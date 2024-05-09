namespace Blog.Models
{
    public class CommentVotes
    {
        public int CommentVoteId { get; set; }
        public int UserId { get; set; }
        public int CommentId { get; set; }
        public int BlogId { get; set; }
        public string VoteType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Users User { get; set; }
        public Comments Comment { get; set; }
        public Blogs Blog { get; set; }
    }
}
