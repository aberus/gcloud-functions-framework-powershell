using Google.Cloud.Functions.Framework;
using Google.Cloud.Functions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Aberus.Google.Cloud.Functions.Framework;

/// <summary>
/// Adds the production dependency to the service collection.
/// </summary>
public class PowerShellFunctionStartup : FunctionsStartup
{
    /// <inheritdoc/>
    public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection services) =>
        services
            .AddSingleton<IPowerShellRunner, PowerShellRunner>()
            .AddSingleton<IHttpRequestReader<HttpRequest>, HttpRequestReader>()
            .AddSingleton<IHttpResponseWriter<HttpResponse>, HttpResponseWriter>();
}