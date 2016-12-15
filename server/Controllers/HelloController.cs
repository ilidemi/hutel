using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
 
namespace hutel.Controllers
{
    [Route("api/[controller]")]
    public class HelloController : Controller
    {
        // GET api/values
        [HttpGet]
        public IDictionary<string, string> Get()
        {
            return new Dictionary<string, string> { { "hello", "world" } };
        }
    }
}