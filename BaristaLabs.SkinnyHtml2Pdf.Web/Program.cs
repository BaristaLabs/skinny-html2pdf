namespace BaristaLabs.SkinnyHtml2Pdf.Web
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using System;

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("hi.");
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://*:80") // <----- This will fix "Err Empty Response"
                .UseStartup<Startup>()
                .Build();
    }
}
