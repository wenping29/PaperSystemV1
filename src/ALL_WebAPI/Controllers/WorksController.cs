using Microsoft.AspNetCore.Mvc;
using PaperSystemApi.DTOs;
using PaperSystemApi.Interfaces;

namespace PaperSystemApi.Controllers
{
    [ApiController]
    [Route("api/v1/works")]
    public class WorksController : ControllerBase
    {
        private readonly ILogger<WorksController> _logger;
        private readonly IWritingServiceS _writingService;

        public WorksController(ILogger<WorksController> logger, IWritingServiceS writingService)
        {
            _logger = logger;
            _writingService = writingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetWorks([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            _logger.LogInformation("Getting works - Page: {Page}, PageSize: {PageSize}", page, pageSize);
            var works = await _writingService.GetWorksAsync(page, pageSize);
            var count = await _writingService.GetWorksCountAsync();
            return Ok(new
            {
                page,
                pageSize,
                totalCount = count,
                totalPages = (int)Math.Ceiling(count / (double)pageSize),
                data = works
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorkById(long id)
        {
            _logger.LogInformation("Getting work with ID: {WorkId}", id);
            var work = await _writingService.GetWorkByIdAsync(id);
            if (work == null)
            {
                return NotFound(new { error = "Work not found" });
            }
            return Ok(work);
        }

        [HttpGet("{id}/versions")]
        public async Task<IActionResult> GetWorkVersions(long id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            _logger.LogInformation("Getting versions for work {WorkId}", id);
            var versions = await _writingService.GetWorkVersionsAsync(id, page, pageSize);
            return Ok(new { workId = id, data = versions });
        }
    }
}