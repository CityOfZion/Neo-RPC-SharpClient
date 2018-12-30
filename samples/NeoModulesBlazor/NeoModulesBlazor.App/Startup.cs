using Microsoft.AspNetCore.Blazor.Builder;
using Microsoft.Extensions.DependencyInjection;
using NeoModules.Rest.Interfaces;
using NeoModules.Rest.Services;

namespace NeoModulesBlazor.App
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Since Blazor is running on the server, we can use an application service
            // to read the forecast data.
            services.AddSingleton<IHappyNodesService>(new HappyNodesService());
            services.AddSingleton<INeoscanService>(new NeoScanRestService(NeoScanNet.MainNet));
        }

        public void Configure(IBlazorApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
