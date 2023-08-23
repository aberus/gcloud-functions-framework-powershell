using Google.Cloud.Functions.Framework;
using Google.Cloud.Functions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Aberus.Google.Cloud.Functions.Framework;

[FunctionsStartup(typeof(PowerShellFunctionStartup))]
public abstract class PowerShellFunction : IHttpFunction
{
    private readonly IPowerShellRunner _powerShellRunner;
    private readonly IHttpRequestReader<HttpRequest> _requestReader;
    private readonly IHttpResponseWriter<HttpResponse> _responseWriter;
    private readonly ILogger _logger;

    protected PowerShellFunction(
        IPowerShellRunner powerShellRunner,
        IHttpRequestReader<HttpRequest> requestReader,
        IHttpResponseWriter<HttpResponse> responseWriter,
        ILogger<PowerShellFunction> logger)
    {
        _powerShellRunner = powerShellRunner;
        _requestReader = requestReader;
        _responseWriter = responseWriter;
        _logger = logger;
    }

    public async Task HandleAsync(HttpContext context)
    {
        HttpRequest data;
        try
        {
            data = await _requestReader.ReadRequestAsync(context.Request).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var script = await File.ReadAllTextAsync("function.ps1", context.RequestAborted).ConfigureAwait(false);
        var response = await _powerShellRunner.RunScriptAsync(script, data, context.RequestAborted).ConfigureAwait(false);
        try
        {
            await _responseWriter.WriteResponseAsync(context.Response, response).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return;
        }
    }
}