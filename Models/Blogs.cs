using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blog.Models
{
    public class Blogs
    {
        [Key]
        public int BlogId { get; set; }
        public string BlogTitle { get; set; }
        public string BlogContent { get; set; }
        public string Image { get; set; }


        [ForeignKey("Users")]
        public int UserId { get; set; }
        public Users User { get; set; }


        [ForeignKey("Category")]
        public int? CategoryId { get; set; }
        public Category Category { get; set; }


        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }


        public Blogs()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
