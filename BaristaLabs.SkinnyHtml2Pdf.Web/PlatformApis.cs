namespace BaristaLabs.SkinnyHtml2Pdf.Web
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    [ExcludeFromCodeCoverage]
    public static class PlatformApis
    {
        static PlatformApis()
        {
            IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            IsDarwin = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        }

        public static bool IsWindows
        {
            get;
        }

        public static bool IsDarwin
        {
            get;
        }

        public static bool IsLinux
        {
            get;
        }
    }
}