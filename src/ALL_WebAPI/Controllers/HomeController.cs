using Microsoft.AspNetCore.Mvc;
using PaperSystemApi.DTOs;
using PaperSystemApi.Interfaces;

namespace PaperSystemApi.Controllers
{
    [ApiController]
    [Route("api/v1/home")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWritingServiceS _writingService;
        private readonly IUserServiceS _userService;

        public HomeController(
            ILogger<HomeController> logger,
            IWritingServiceS writingService,
            IUserServiceS userService)
        {
            _logger = logger;
            _writingService = writingService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetHomeData([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Getting home page data");

                var featuredWorks = await _writingService.GetWorksAsync(1, 5, isPublished: true);
                var recentWorks = await _writingService.GetWorksAsync(1, pageSize, isPublished: true);
                var worksCount = await _writingService.GetWorksCountAsync(isPublished: true);
                var usersCount = await _userService.GetUsersCountAsync();

                var categories = await _writingService.GetCategoriesAsync();

                return Ok(new
                {
                    featured = featuredWorks,
                    recent = recentWorks,
                    categories = categories,
                    stats = new
                    {
                        totalWorks = worksCount,
                        totalUsers = usersCount,
                        totalCategories = categories.Count()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting home page data");
                return StatusCode(500, new { error = "An error occurred while getting home page data" });
            }
        }
    }
}
