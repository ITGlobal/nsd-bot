using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.Configuration;

namespace NSD.Bot2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseIISIntegration()
                .UseContentRoot(Path.Combine(Directory.GetCurrentDirectory()))
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .UseUrls("http://*:51405")
                .Build();

            host.Run();
        }

        public static IConfigurationRoot Configuration
        {
            get { return ConfigurationManager.Configuration; }
            set { ConfigurationManager.Configuration = value; }
        }
    }
}
