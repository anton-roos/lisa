using Microsoft.AspNetCore.Mvc;

namespace Lisa.Controllers
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