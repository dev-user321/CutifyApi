using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CutifyApi.Controllers
{
    public class ValuesController : AppController
    {
        [HttpGet]
        [Authorize]
        public IActionResult GetValues()
        {
            return Ok(new string[] { "value1", "value2" });
        }
    }
}
