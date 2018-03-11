namespace BaristaLabs.SkinnyHtml2Pdf.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    [Route("api/[controller]")]
    public class Html2ImageController : Controller
    {
        public Html2ImageController(Chrome chrome)
        {
            Chrome = chrome;
        }

        public Chrome Chrome
        {
            get;
        }

        // GET api/html2image
        [HttpGet]
        public async Task<IActionResult> Get(string url, int? width, int? height, string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            }

            var screenshotData = await Chrome.CaptureImage(url, width, height);
            return new FileContentResult(screenshotData, "image/png");
        }
    }
}
