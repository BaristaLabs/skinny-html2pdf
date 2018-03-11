namespace BaristaLabs.SkinnyHtml2Pdf.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    public class TestController : Controller
    {
        // GET api/Test
        [HttpGet]
        public string Get()
        {
            return "Hello, World!";
        }
    }
}
