using Google.Cloud.Functions.Framework;
using Google.Cloud.Functions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Aberus.Google.Cloud.Functions.Framework;

public class PowerShellFunctionStartup : FunctionsStartup
{
    public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection services) =>
        services
            .AddSingleton<IPowerShellRunner, PowerShellRunner>()
            .AddSingleton<IHttpRequestReader<HttpRequest>, HttpRequestReader>()
            .AddSingleton<IHttpResponseWriter<HttpResponse>, HttpResponseWriter>();
}