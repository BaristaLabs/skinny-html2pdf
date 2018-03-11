namespace BaristaLabs.SkinnyHtml2Pdf.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BaristaLabs.ChromeDevTools.Runtime;
    using Microsoft.AspNetCore.Mvc;
    using BaristaLabs.ChromeDevTools.Runtime.Page;

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

        // GET api/values
        [HttpGet]
        public async Task<IEnumerable<string>> Get(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                url = "http://www.rdacorp.com/";
            }

            var sessions = await Chrome.GetActiveSessions();
            Console.WriteLine(sessions.Count);
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
