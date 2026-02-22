using Microsoft.AspNetCore.Mvc;
using StudentTrackingSystem.Api.Services;

namespace StudentTrackingSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassController : ControllerBase
    {
        private readonly ClassService _classService;

        public ClassController(ClassService classService)
        {
            _classService = classService;
        }

        [HttpGet("all-with-count")]
        public async Task<IActionResult> GetClassesWithCount()
        {
            var data = await _classService.GetAllClassesWithStudentCountAsync();
            return Ok(data);
        }
    }
}