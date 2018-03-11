namespace BaristaLabs.SkinnyHtml2Pdf.Web
{
    using BaristaLabs.ChromeDevTools.Runtime;
    using BaristaLabs.ChromeDevTools.Runtime.Page;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed partial class Chrome
    {
        public async Task<byte[]> ConvertHtmlToPdf(string html, int? width, int? height, bool? printBackground, int? delayMs = 500)
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

            if (!delayMs.HasValue || delayMs.Value < 1)
            {
                delayMs = 500;
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

                    var setDocumentContentResult = session.Page.SetDocumentContent(new SetDocumentContentCommand()
                    {
                        FrameId = frameTreeResponse.FrameTree.Frame.Id,
                        Html = html
                    }, millisecondsTimeout: 120 * 1000);

                    await Task.Delay(delayMs.Value);

                    var pdf = await session.Page.PrintToPDF(new PrintToPDFCommand()
                    {
                        PrintBackground = printBackground
                    }, millisecondsTimeout: 120 * 1000);

                    return Convert.FromBase64String(pdf.Data);
                }
            }
            finally
            {
                await CloseSession(newSessionInfo);
            }
        }
    }
}
