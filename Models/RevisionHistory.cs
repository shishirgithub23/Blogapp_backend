using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Blog.Models
{
    public class BlogRevision
    {
        [Key]
        public int RevisionId { get; set; }

        [ForeignKey("Blogs")]
        public int BlogId { get; set; }

        [Required]
        public string BlogTitle { get; set; }
        public string BlogContent { get; set; }

        [ForeignKey("Users")]
        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class CommentRevision
    {
        [Key]
        public int CommentRevisionId { get; set; }

        [Required]
        public string CommentText { get; set; }

        [ForeignKey("Users")]
        public int UserId { get; set; }

        [ForeignKey("Blogs")]
        public int BlogId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
