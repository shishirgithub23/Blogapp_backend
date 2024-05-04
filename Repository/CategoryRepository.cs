using Blog.Data;
using Blog.Interfaces;
using Blog.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Blog.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllCategories()
        {
            var query = @"SELECT * FROM Category";

            using (var connection = _context.CreateConnection())
            {
                var categories = await connection.QueryAsync<Category>(query);

                return categories;
            }
        }

        public async Task<Category> GetCategoryById(int id)
        {
            var query = @"SELECT * FROM Category WHERE CategoryId = @CategoryId";

            using (var connection = _context.CreateConnection())
            {
                var category = await connection.QueryFirstOrDefaultAsync<Category>(query, new { CategoryId = id });

                return category;
            }
        }

        public async Task<bool> AddCategory(CategoryDto categoryDto)
        {
            try
            {
                var query = @"
                    INSERT INTO Category (CategoryName)
                    VALUES (@CategoryName);";

                using (var connection = _context.CreateConnection())
                {
                    var rowsAffected = await connection.ExecuteAsync(query, new { CategoryName = categoryDto.CategoryName });

                    return rowsAffected > 0;
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error adding category: {ex.Message}");
                return false;
            }
        }

        public async Task<CategoryDto> UpdateCategory(int id, CategoryDto categoryDto)
        {
            var query = @"
                UPDATE Category 
                SET CategoryName = @CategoryName
                WHERE CategoryId = @CategoryId;
                SELECT * FROM Category WHERE CategoryId = @CategoryId;";

            using (var connection = _context.CreateConnection())
            {
                var updatedCategory = await connection.QueryFirstOrDefaultAsync<Category>(query, new { CategoryId = id, CategoryName = categoryDto.CategoryName });

                if (updatedCategory == null)
                {
                    return null;
                }

                var updateCategoryDto = new CategoryDto
                {
                    CategoryName = updatedCategory.CategoryName
                };

                return updateCategoryDto;
            }
        }

        public async Task DeleteCategory(int id)
        {
            try
            {
                var query = @"DELETE FROM Category WHERE CategoryId = @CategoryId";

                using (var connection = _context.CreateConnection())
                {
                    await connection.ExecuteAsync(query, new { CategoryId = id });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting category with ID {id}: {ex.Message}", ex);
            }
        }
    }
}
