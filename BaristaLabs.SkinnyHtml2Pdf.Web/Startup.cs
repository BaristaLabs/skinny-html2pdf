namespace BaristaLabs.SkinnyHtml2Pdf.Web
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            Chrome chrome;
            if (Environment.GetEnvironmentVariable("CHROME_LAUNCH_LOCAL") == "true")
            {
                Console.WriteLine("Launching Chrome...");
                var chromeProcess = ChromeProcess.LaunchChrome();

                //TODO: We should probably wait here for a spell as it takes a few seconds for chrome to fire up...

                Console.WriteLine($"Chrome running at {chromeProcess.RemoteDebuggingPort}");
                services.AddSingleton(chromeProcess);

                chrome = new Chrome($"http://localhost:{chromeProcess.RemoteDebuggingPort}");
            }
            else
            {
                chrome = new Chrome();
            }

            var serviceCollection = new ServiceCollection();
            services.AddSingleton(chrome);

            services.AddMvc(options =>
            {
                options.InputFormatters.Add(new TextPlainInputFormatter());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
