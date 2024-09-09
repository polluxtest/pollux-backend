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
                   if (environment == "Development")
                   {
                       config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true);
                   }
                   else
                   {
                       config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
                   }
               })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel();
                });
    }
}
