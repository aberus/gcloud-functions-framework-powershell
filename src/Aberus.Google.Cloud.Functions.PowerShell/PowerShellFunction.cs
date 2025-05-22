using Google.Cloud.Functions.Framework;
using Google.Cloud.Functions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Aberus.Google.Cloud.Functions.Framework;

/// <summary>
/// Base class for PowerShell functions in Google Cloud Functions.
/// </summary>
/// <param name="powerShellRunner">The PowerShell runner to execute the PowerShell script.</param>
/// <param name="logger">The logger to use to report errors.</param>
[FunctionsStartup(typeof(PowerShellFunctionStartup))]
public abstract class PowerShellFunction(
    IPowerShellRunner powerShellRunner,
    ILogger<PowerShellFunction> logger) : ITypedFunction<HttpRequest, HttpResponse>
{
    private readonly IPowerShellRunner _powerShellRunner = powerShellRunner;
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Asynchronously handles an incoming request
    /// </summary>
    /// <param name="request">
    /// The HTTP context containing the request and response.
    /// The request payload, deserialized from the incoming request.</param>
    /// <param name="cancellationToken">A cancellation token which indicates if the request is aborted.</param>
    /// <returns>A <see cref="HttpResponse"/> containing the result of the PowerShell script execution. If an error occurs, the
    /// response will include an appropriate status code and error message.</returns>
    public async Task<HttpResponse> HandleAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var scriptFilePath = GetScriptFilePath();
            var script = await File.ReadAllTextAsync(scriptFilePath, cancellationToken).ConfigureAwait(false);
            return await _powerShellRunner.RunScriptAsync(script, request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return new HttpResponse
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Body = e.Message
            };
        }
    }

    /// <summary>
    /// Retrieves the name of the PowerShell script file used by the application.
    /// </summary>
    /// <returns>The name of the PowerShell script file, as a string.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the PowerShell script file is not found in the assembly resources. Ensure the script is included in
    /// the package.</exception>
    private static string GetScriptFilePath()
    {
        const string powerShellScriptPath = "function.ps1";

        if(!File.Exists(powerShellScriptPath))
        {
            throw new FileNotFoundException($"PowerShell script {powerShellScriptPath} not found in the assembly resources. Ensure the script is included in the package.");
        }
        return powerShellScriptPath;
    }
}