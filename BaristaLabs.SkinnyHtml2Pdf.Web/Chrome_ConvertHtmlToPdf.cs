namespace BaristaLabs.SkinnyHtml2Pdf.Web
{
    using BaristaLabs.ChromeDevTools.Runtime;
    using BaristaLabs.ChromeDevTools.Runtime.Page;
    using System;
    using System.Threading.Tasks;

    public sealed partial class Chrome
    {
        public async Task<byte[]> ConvertHtmlToPdf(string html, int? width, int? height, bool? printBackground)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                html = "<html><body>Hello, World!</body></html>";
            }

            if (!width.HasValue)
            {
                width = 1280;
            }

            if (!height.HasValue)
            {
                height = 1024;
            }

            var newSessionInfo = await CreateNewSession();

            try
            {
                using (var session = new ChromeSession(newSessionInfo.WebSocketDebuggerUrl))
                {
                    await session.Page.Enable();

                    //Set the viewport size
                    await session.Emulation.SetDeviceMetricsOverride(new ChromeDevTools.Runtime.Emulation.SetDeviceMetricsOverrideCommand()
                    {
                        Width = (long)width,
                        Height = (long)height,
                        DeviceScaleFactor = 0,
                        Mobile = false,
                    });

                    var frameTreeResponse = await session.Page.GetFrameTree(new GetFrameTreeCommand());

                    using (var navigatorWatcher = new NavigatorWatcher(session))
                    {
                        await navigatorWatcher.Start();

                        var setDocumentContentResult = session.Page.SetDocumentContent(new SetDocumentContentCommand()
                        {
                            FrameId = frameTreeResponse.FrameTree.Frame.Id,
                            Html = html
                        }, millisecondsTimeout: 120 * 1000);

                        await navigatorWatcher.WaitForNetworkIdle();
                    }

                    var pdf = await session.Page.PrintToPDF(new PrintToPDFCommand()
                    {
                        PrintBackground = printBackground
                    }, millisecondsTimeout: 120 * 1000);

                    if (!String.IsNullOrWhiteSpace(pdf.Data))
                    {
                        return Convert.FromBase64String(pdf.Data);
                    }

                    return new byte[] { };
                }
            }
            finally
            {
                await CloseSession(newSessionInfo);
            }
        }
    }
}
