using Microsoft.AspNetCore.Mvc;
using TaskCenter.Properties;

namespace TaskCenter.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class TestAllController : SampleController
    {
        public TestAllController(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
            DumpRequest = AppSettings.Default.EnableRequestDump;
        }

        [HttpGet("Index")]
        public IActionResult Index()
        {
            return Json(new { message = "TestAllController is working!" });
        }
    }
}
