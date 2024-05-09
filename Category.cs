using System.ComponentModel.DataAnnotations;

namespace Blog.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }    
    }

    public class CategoryDto
    {
        public string CategoryName { get; set; }
    }
}
