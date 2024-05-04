namespace Blog.Models
{
    public class BlogVotes
    {
        public int Id { get; set; }
        public int Upvotes { get; set; }
        public int Downvotes { get; set; }

        public int BlogId { get; set; }
        public Blogs Blog { get; set; }

        public int UserId { get; set; }
        public Users User { get; set; }

    }
}
