using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blog.Models
{
    public class Comments
    {
        [Key]
        public int CommentId { get; set; }
        public string CommentText { get; set; }


        [ForeignKey("Blogs")]
        public int BlogId { get; set; }
        public required Blogs Blog { get; set; }


        [ForeignKey("Users")]
        public int UserId { get; set; }
        public required Users User { get; set; }


        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
