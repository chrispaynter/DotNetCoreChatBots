using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Paynter.FacebookMessenger.Services;
using Paynter.FacebookMessenger.Configuration;
using Serilog;
using Microsoft.Extensions.Logging;
using Paynter.WitAi.Services;
using Paynter.WitAi.Configuration;
using Paynter.WitAi.Sessions;
using Paynter.Harvest.Configuration;
using DotNetCoreChatBots.Helpers;
using Paynter.Harvest.Services;

namespace DotNetCoreChatBots
{
    public class Startup
    {
        public IConfiguration Configuration;

        public Startup(IHostingEnvironment env)
        {
            Log.Logger = new LoggerConfiguration()
                            .MinimumLevel.Debug()
                            .Enrich.FromLogContext()
                            .WriteTo.File("log.txt")
                            .WriteTo.LiterateConsole()
                            .CreateLogger();
            
            var builder = new ConfigurationBuilder()
                                .SetBasePath(env.ContentRootPath + "/src/DotNetCoreChatBots")
                                .AddJsonFile("appsettings.json");

            if(env.IsDevelopment())
            {
                builder.AddUserSecrets();
            }

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddJsonOptions(opt => {
                opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            services.AddOptions();
            services.Configure<FacebookOptions>(Configuration.GetSection("dotNetCoreFacebookMessenger"));
            services.Configure<WitAiOptions>(Configuration.GetSection("dotNetCoreWitAi"));
            services.Configure<HarvestOptions>(Configuration.GetSection("dotNetCoreHarvest"));

            services.AddSingleton<WitAiService, WitAiService>(); 
            services.AddSingleton<FacebookMessengerService, FacebookMessengerService>();
            services.AddSingleton<ChatBotHelper, ChatBotHelper>();
            services.AddSingleton<WitSessionHelper, WitSessionHelper>(); // Important this is singleton as it holds cross crequest sessions
            services.AddSingleton<HarvestService, HarvestService>();
            services.AddSingleton<HarvestDataHelper, HarvestDataHelper>();
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddSerilog();
            app.UseMvc();
        }
    }
}
    