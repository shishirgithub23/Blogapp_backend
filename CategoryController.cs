using Blog.Interfaces;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blog.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetAllCategories()
        {
            var categories = await _categoryRepository.GetAllCategories();

            if (categories == null)
            {
                return NotFound(new { message = "No categories found" });
            }

            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategoryById(int id)
        {
            var category = await _categoryRepository.GetCategoryById(id);

            if (category == null)
            {
                return NotFound(new { message = "Category not found with that Id" });
            }

            return Ok(category);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddCategory([FromBody] CategoryDto categoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var isSuccess = await _categoryRepository.AddCategory(categoryDto);
            if (!isSuccess)
            {
                return StatusCode(500, new { message = "Failed to add category" });
            }

            return Ok(new { message = "Category added successfully" });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryDto categoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedCategory = await _categoryRepository.UpdateCategory(id, categoryDto);
            if (updatedCategory == null)
            {
                return NotFound(new { message = "Category not found with that Id" });
            }

            return Ok(new { message = "Category updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            await _categoryRepository.DeleteCategory(id);
            return NoContent();
        }
    }
}
