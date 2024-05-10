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

        [HttpGet("getallcategorieshome")]
        public async Task<ActionResult<IEnumerable<Category>>> GetAllCategoriesForHome()
        {
            var categories = await _categoryRepository.GetAllCategories();

            if (categories == null)
            {
                return NotFound(new { message = "No categories found" });
            }

            return Ok(categories);
        }


        [HttpGet("getallcategories")]
        [Authorize]
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

        [HttpPost("createcategory")]
        [Authorize]
       // [Authorize(Roles = "Admin")]
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

        [HttpPost("updatecategory")]
        //[Authorize(Roles = "Admin")]
        [Authorize]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryDTO categoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedCategory = await _categoryRepository.UpdateCategory(categoryDto);
            if (updatedCategory == null)
            {
                return NotFound(new { message = "Category not found with that Id" });
            }

            return Ok(new { message = "Category updated successfully" });
        }

        [HttpPost("deletecategory")]
      //  [Authorize(Roles = "Admin")]
        [Authorize]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            await _categoryRepository.DeleteCategory(categoryId);
            return NoContent();
        }
    }
}
