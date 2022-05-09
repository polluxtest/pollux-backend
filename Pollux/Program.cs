namespace Pollux
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Pollux.API;

    public class Program
    {
        private static string environment;

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    environment = hostingContext.HostingEnvironment.EnvironmentName;
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
                    config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    if (environment == "Development")
                    {
                        webBuilder.UseStartup<Startup>()
                        .UseUrls("http:localhost:5001");

                    }

                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel();

                });
    }
}
