using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace DotNetCoreChatBots
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var currentDir = Directory.GetCurrentDirectory();
            
            var host = new WebHostBuilder()
                            .UseContentRoot(Directory.GetCurrentDirectory())
                            .UseKestrel()
                            .UseStartup<Startup>()
                            .Build();

            host.Run();
        }
    }
}
