using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace hutel.Controllers
{
    [Route("api/[controller]")]
    public class HelloController : Controller
    {
        // GET api/hello
        [HttpGet]
        public IActionResult Get()
        {
            return Json(new Dictionary<string, string> { { "hello", "world" } });
        }
    }
}