namespace BaristaLabs.SkinnyHtml2Pdf.Web.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    public class Html2PdfController : Controller
    {
        public Html2PdfController(Chrome chrome)
        {
            Chrome = chrome;
        }

        public Chrome Chrome
        {
            get;
        }

        // GET api/Html2Pdf
        [HttpGet]
        public async Task<IActionResult> Get(string url, int? width, int? height, bool? printBackground, string fileName)
        {
            var pdfData = await Chrome.CapturePdf(url, width, height, printBackground);
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            }
            return new FileContentResult(pdfData, "application/pdf");
        }

        // POST api/Html2Pdf
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]string html, [FromQuery]int? width, [FromQuery]int? height, [FromQuery]int? delayMs, [FromQuery]string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = "converted.pdf";
            }

            var pdfData = await Chrome.ConvertHtmlToPdf(html, width, height, false, delayMs);
            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return new FileContentResult(pdfData, "application/pdf");
        }
    }
}
