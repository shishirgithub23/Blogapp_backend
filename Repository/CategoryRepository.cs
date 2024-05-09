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
            var query = @"SELECT * FROM Category where active=1";
            var categories = new List<Category>();
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    categories = connection.Query<Category>(query).ToList();
                }

            }catch (Exception ex)
            {
            }
            return categories;
        }

        public async Task<Category> GetCategoryById(int id)
        {
            var query = @"SELECT * FROM Category WHERE CategoryId = @CategoryId and active=1";

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
                    VALUES (@CategoryName)";

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

        public async Task<CategoryDto> UpdateCategory(UpdateCategoryDTO categoryDto)
        {
            var query = @"
                UPDATE Category 
                SET CategoryName = @CategoryName
                WHERE CategoryId = @CategoryId and active=1;

                SELECT * FROM Category WHERE CategoryId = @CategoryId and active=1";

            using (var connection = _context.CreateConnection())
            {
                var updatedCategory = await connection.QueryFirstOrDefaultAsync<Category>(query, new { CategoryId = categoryDto.CategoryId, CategoryName = categoryDto.CategoryName });

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
                var query = @"update dbo.category 
                                set active=0
                               output inserted.*
                             WHERE CategoryId = @CategoryId
                                and active=1
                                    ";

                using (var connection = _context.CreateConnection())
                {
                    var removedData=connection.Query<dynamic>(query, new { CategoryId = id });
                    if (removedData.Count() == 0)
                    {
                        throw new Exception("Data Not Found!");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting category with ID {id}: {ex.Message}", ex);
            }
        }
    }
}
