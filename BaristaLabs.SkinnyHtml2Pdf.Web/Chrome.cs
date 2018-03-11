namespace BaristaLabs.SkinnyHtml2Pdf.Web
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents and manages a chrome process instance
    /// </summary>
    public sealed partial class Chrome : IDisposable
    {
        private static readonly string[] DefaultArgs = {
          "--disable-background-networking",
          "--disable-background-timer-throttling",
          "--disable-client-side-phishing-detection",
          "--disable-default-apps",
          "--disable-extensions",
          "--disable-hang-monitor",
          "--disable-popup-blocking",
          "--disable-prompt-on-repost",
          "--disable-sync",
          "--disable-translate",
          "--disable-dev-shm-usage",
          "--metrics-recording-only",
          "--no-first-run",
          "--safebrowsing-disable-auto-update",
          "--bwsi"
        };

        private static readonly string[] AutomationArgs = {
          "--enable-automation",
          "--password-store=basic",
          "--use-mock-keychain",
          "--user-data-dir=/tmp/chrome-test-profile",
          "--disable-web-security",
        };

        private static readonly string[] HeadlessArgs ={
            "--headless",
            "--disable-gpu",
            "--hide-scrollbars",
            "--mute-audio",
        };

        private DirectoryInfo m_userDirectory;

        internal Chrome(Process chromeProcess, DirectoryInfo userDirectory, int remoteDebuggingPort)
        {
            Process = chromeProcess ?? throw new ArgumentNullException(nameof(chromeProcess));
            m_userDirectory = userDirectory ?? throw new ArgumentNullException(nameof(userDirectory));
            RemoteDebuggingPort = remoteDebuggingPort;
        }

        /// <summary>
        /// Gets the Process object for the Chrome instance
        /// </summary>
        public Process Process { get; private set; }

        /// <summary>
        /// Gets the Remote Debugging Port that Chrome is listening to.
        /// </summary>
        public int RemoteDebuggingPort { get; }

        private HttpClient GetDebuggerClient()
        {
            var chromeHttpClient = new HttpClient()
            {
                BaseAddress = new Uri($"http://localhost:{RemoteDebuggingPort}")
            };

            return chromeHttpClient;
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

        #region IDisposable Support
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Kill the chrome process.
                if (Process != null)
                {
                    if (Process.HasExited == false)
                    {
                        Process.WaitForExit(1000);
                    }

                    Process.Kill();
                    Process.Dispose();
                    Process = null;
                }

                //Clean up the target user directory.
                if (m_userDirectory != null)
                {

                    //for (int i = 0; i < 10; i++)
                    //{
                    //    if (m_userDirectory.Exists == false)
                    //        continue;

                    //    try
                    //    {
                    //        Thread.Sleep(500);
                    //        m_userDirectory.Delete(true);
                    //    }
                    //    catch
                    //    {
                    //        //Do Nothing.
                    //    }
                    //}

                    //if (m_userDirectory.Exists)
                    //    throw new InvalidOperationException($"Unable to delete the user directory at {m_userDirectory.FullName} after 10 tries.");

                    m_userDirectory = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        /// <summary>
        /// Creates a new chrome instance
        /// </summary>
        /// <param name="remoteDebuggingPort"></param>
        /// <returns></returns>
        public static Chrome OpenChrome(int? remoteDebuggingPort = null, bool headless = true)
        {
            var chromePath = Environment.GetEnvironmentVariable("CHROME_BIN");
            if (String.IsNullOrWhiteSpace(chromePath))
            {
                if (PlatformApis.IsWindows)
                {
                    chromePath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
                }
                else if (PlatformApis.IsLinux)
                {
                    chromePath = @"/usr/bin/chromium-browser"; //"google-chrome"
                }
                else if (PlatformApis.IsDarwin)
                {
                    chromePath = @"/Applications/Google Chrome.app/Contents/MacOS/Google Chrome";
                }
                else
                {
                    throw new InvalidOperationException("Unknown or unsupported platform.");
                }
            }

            var chromeDebuggingPort = Environment.GetEnvironmentVariable("CHROME_REMOTE_DEBUGGING_PORT");
            if (!remoteDebuggingPort.HasValue && int.TryParse(chromeDebuggingPort, out int remoteDebuggingPortFromEnv))
            {
                remoteDebuggingPort = remoteDebuggingPortFromEnv;
            }
            else
            {
                remoteDebuggingPort = 9222;
            }

            string path = Path.GetRandomFileName();
            var directoryInfo = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), path));
            var remoteDebuggingArg = $"--remote-debugging-port={remoteDebuggingPort.Value}";
            var userDirectoryArg = $"--user-data-dir=\"{directoryInfo.FullName}\"";
            var chromeProcessArgs = $@"{String.Join(" ", DefaultArgs)} {String.Join(" ", AutomationArgs)}";

            if (headless)
            {
                chromeProcessArgs += $@" {String.Join(" ", HeadlessArgs)}";
            }

            chromeProcessArgs += $@" {remoteDebuggingArg} {userDirectoryArg}";
        
            if(!String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CHROME_NO_SANDBOX")))
            {
                chromeProcessArgs += " --no-sandbox";
            }

            var processId = Process.GetCurrentProcess().Id;
            var chromeProcess = Process.Start(new ProcessStartInfo
            {
                FileName = chromePath,
                Arguments = chromeProcessArgs,
                RedirectStandardOutput = true,
            });
            chromeProcess.Start();

            return new Chrome(chromeProcess, directoryInfo, remoteDebuggingPort.Value);
        }
    }
}
