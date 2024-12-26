using Microsoft.AspNetCore.Mvc;

namespace Lisa.Controllerse
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetStudents()
        {
            return Ok("Here are the students");
        }
    }
}
