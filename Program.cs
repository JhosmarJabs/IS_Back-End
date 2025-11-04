using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;

namespace IS_Back_End
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // Obtiene el puerto de la variable de entorno (o usa 8080 si no existe)
                    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
                    webBuilder
                        .UseStartup<Startup>()
                        .UseUrls($"http://*:{port}");
                });
    }
}
