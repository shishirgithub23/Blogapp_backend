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
    }

    public class VoteStatusDto
    {
        public int Upvotes { get; set; }
        public int Downvotes { get; set; }
    }

}
