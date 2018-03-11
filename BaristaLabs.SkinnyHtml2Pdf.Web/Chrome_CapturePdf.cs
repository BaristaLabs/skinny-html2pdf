namespace BaristaLabs.SkinnyHtml2Pdf.Web
{
    using BaristaLabs.ChromeDevTools.Runtime;
    using BaristaLabs.ChromeDevTools.Runtime.Page;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed partial class Chrome
    {
        public async Task<byte[]> CapturePdf(string url, int? width, int? height, bool? printBackground)
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
            byte[] pdfData = new byte[] { };

            try
            {
                using (var session = new ChromeSession(newSessionInfo.WebSocketDebuggerUrl))
                {
                    await session.Page.Enable();

                    session.Page.SubscribeToLoadEventFiredEvent(async (e) =>
                    {
                        var pdf = await session.Page.PrintToPDF(new PrintToPDFCommand()
                        {
                            PrintBackground = printBackground
                        }, millisecondsTimeout: 120 * 1000);

                        if (!String.IsNullOrWhiteSpace(pdf.Data))
                        {
                            s.Release();
                            pdfData = Convert.FromBase64String(pdf.Data);
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

            return pdfData;
        }
    }
}
