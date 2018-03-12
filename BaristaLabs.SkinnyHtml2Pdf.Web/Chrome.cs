namespace BaristaLabs.SkinnyHtml2Pdf.Web
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a chrome remote instance.
    /// </summary>
    public sealed partial class Chrome
    {
        private readonly string m_chromeRemoteUrl;

        public Chrome()
        {
        }

        public Chrome(string chromeRemoteUrl)
            : this()
        {
            m_chromeRemoteUrl = chromeRemoteUrl;
        }

        private HttpClient GetDebuggerClient()
        {
            var chromeHttpClient = new HttpClient()
            {
                BaseAddress = new Uri(ChromeRemoteUrl)
            };

            return chromeHttpClient;
        }

        public string ChromeRemoteUrl
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(m_chromeRemoteUrl))
                    return m_chromeRemoteUrl;

                string chromeRemoteUrl = Environment.GetEnvironmentVariable("CHROME_REMOTE_URL");
                if (!String.IsNullOrWhiteSpace(chromeRemoteUrl))
                    return chromeRemoteUrl;

                throw new InvalidOperationException("Unable to determine Chrome Remote Url.");
            }
        }

        public async Task<ICollection<ChromeSessionInfo>> GetActiveSessions()
        {
            using (var webClient = GetDebuggerClient())
            {
                var remoteSessions = await webClient.GetStringAsync("/json");
                return JsonConvert.DeserializeObject<ICollection<ChromeSessionInfo>>(remoteSessions);
            }
        }

        public async Task<ChromeSessionInfo> CreateNewSession()
        {
            using (var webClient = GetDebuggerClient())
            {
                var result = await webClient.PostAsync("/json/new", null);
                var contents = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ChromeSessionInfo>(contents);
            }
        }

        public async Task CloseSession(ChromeSessionInfo sessionInfo)
        {
            using (var webClient = GetDebuggerClient())
            {
                var result = await webClient.PostAsync("/json/close/" + sessionInfo.Id, null);
                var contents = await result.Content.ReadAsStringAsync();
                //Assert contents == "Target is closing"
            }
        }
    }
}
