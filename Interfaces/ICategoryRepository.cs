using Blog.Models;

namespace Blog.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllCategories();
        Task<Category> GetCategoryById(int id);
        Task<bool> AddCategory(CategoryDto categoryDto);
        Task<CategoryDto> UpdateCategory(int id, CategoryDto categoryDto);
        Task DeleteCategory(int id);
    }
}
