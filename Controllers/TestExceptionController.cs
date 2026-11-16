
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagementAPI.Controllers
{
    [ApiController]
    [Route("api/test-exception")]
    public class TestExceptionController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public TestExceptionController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpGet]
        public IActionResult TestException()
        {
            if (!_environment.IsDevelopment())
            {
                return NotFound();
            }

            throw new Exception("This is a test exception.");
        }
    }
}
