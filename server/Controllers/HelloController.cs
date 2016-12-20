using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace hutel.Controllers
{
    [Route("api/[controller]")]
    public class HelloController : Controller
    {
        // GET api/hello
        [HttpGet]
        public IDictionary<string, string> Get()
        {
            return new Dictionary<string, string> { { "hello", "world" } };
        }
    }
}