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

    public class UpdateCategoryDTO
    {
        public int CategoryId { get; set; }
        [Required(ErrorMessage ="Please Enter Category Name")]
        public string CategoryName { get; set; }
    }
}
