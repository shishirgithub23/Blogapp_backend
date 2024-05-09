using Blog.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardRepository _repo;
        public DashboardController(IDashboardRepository repo)
        {
            _repo=repo;
        }

        [HttpGet("GetDashboardData")]
        [Authorize(Roles = "Admin,Blogger")]
        public IActionResult GetDashboardData()
        {
            var dashboardData=_repo.GetDashboardData();
            return Ok(dashboardData);
        }
    }
}
