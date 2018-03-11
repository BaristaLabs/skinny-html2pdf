namespace BaristaLabs.SkinnyHtml2Pdf.Web
{
    using BaristaLabs.ChromeDevTools.Runtime;
    using BaristaLabs.ChromeDevTools.Runtime.Page;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed partial class Chrome
    {
        public async Task<byte[]> CaptureImage(string url, int? width, int? height)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                url = "http://www.rdacorp.com/";
            }

            if (!width.HasValue)
            {
                width = 1280;
            }

            if (!height.HasValue)
            {
                height = 1024;
            }

            var s = new SemaphoreSlim(0, 1);
            var newSessionInfo = await CreateNewSession();
            byte[] screenshotData = new byte[] { };

            try
            {
                using (var session = new ChromeSession(newSessionInfo.WebSocketDebuggerUrl))
                {
                    await session.Page.Enable();

                    session.Page.SubscribeToLoadEventFiredEvent(async (e) =>
                    {
                        var screenshot = await session.Page.CaptureScreenshot(new CaptureScreenshotCommand()
                        {
                            Format = "png",
                            Clip = new Viewport()
                            {
                                X = 0,
                                Y = 0,
                                Width = width.Value,
                                Height = height.Value,
                                Scale = 1.0,
                            }
                        }, millisecondsTimeout: 120 * 1000);

                        if (!String.IsNullOrWhiteSpace(screenshot.Data))
                        {
                            s.Release();
                            screenshotData = Convert.FromBase64String(screenshot.Data);
                        }
                    });

                    //Set the viewport size
                    await session.Emulation.SetDeviceMetricsOverride(new ChromeDevTools.Runtime.Emulation.SetDeviceMetricsOverrideCommand()
                    {
                        Width = (long)width,
                        Height = (long)height,
                        DeviceScaleFactor = 0,
                        Mobile = false,
                    });

                    var navigateResult = await session.Page.Navigate(new NavigateCommand
                    {
                        Url = url
                    }, millisecondsTimeout: 120 * 1000);

                    await s.WaitAsync();
                }
            }
            finally
            {
                await CloseSession(newSessionInfo);
                s.Dispose();
            }

            return screenshotData;
        }
    }
}
