namespace BaristaLabs.SkinnyHtml2Pdf.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [Route("api/[controller]")]
    public class ActiveSessionsController : Controller
    {
        public ActiveSessionsController(Chrome chrome)
        {
            Chrome = chrome;
        }

        public Chrome Chrome
        {
            get;
        }

        // GET api/Sessions
        [HttpGet]
        public async Task<ICollection<ChromeSessionInfo>> Get()
        {
            return await Chrome.GetActiveSessions();
        }
    }
}
