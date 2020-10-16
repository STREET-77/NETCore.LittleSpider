using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace NETCore.LittleSpider.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddLittleSpider();
                    services.AddHostedService<HostService>();
                });

            hostBuilder.Build().Run();
        }
    }
}
