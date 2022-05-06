using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Travel.API.Controllers
{
    [Route("api/shoudongapi")]
    //[Controller]
    public class ShoudongAPI : Controller
    {
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
    }
}
