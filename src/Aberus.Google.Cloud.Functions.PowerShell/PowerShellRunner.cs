using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using System.Threading.Tasks;

namespace Aberus.Google.Cloud.Functions.Framework;

/// <summary>
/// Provides functionality to execute PowerShell scripts in the context of Google Cloud Functions.
/// </summary>
public sealed class PowerShellRunner : IPowerShellRunner
{
    private readonly IWebHostEnvironment _environment;
    private readonly PowerShell _powerShell;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PowerShellRunner"/> class.
    /// </summary>
    /// <param name="environment">The hosting environment for the application.</param>
    /// <param name="logger">The logger instance for logging messages.</param>
    public PowerShellRunner(IWebHostEnvironment environment, ILogger<PowerShellRunner> logger)
    {
        _environment = environment;
        _logger = logger;

        _powerShell = Create();
    }

    /// <summary>
    /// Creates and initializes a new instance of the <see cref="PowerShell"/> class with a customized session state.
    /// </summary>
    /// <remarks>This method configures the PowerShell instance with default session state settings and adds
    /// custom type accelerators  for <see cref="HttpResponse"/> and <see cref="HttpRequest"/>. It also attaches event
    /// handlers to the PowerShell  streams to log various types of output, such as errors, warnings, and progress
    /// updates.  On Windows platforms, the execution policy is set to <see
    /// cref="Microsoft.PowerShell.ExecutionPolicy.Unrestricted"/>.</remarks>
    /// <returns>A fully initialized <see cref="PowerShell"/> instance ready for execution.</returns>
    private PowerShell Create()
    {
        var accelerator = typeof(PSObject).Assembly.GetType("System.Management.Automation.TypeAccelerators");
        var addMethod = accelerator?.GetMethod("Add", [typeof(string), typeof(Type)]);
        addMethod?.Invoke(null, ["HttpResponse", typeof(HttpResponse)]);
        addMethod?.Invoke(null, ["HttpRequest", typeof(HttpRequest)]);

        var state = InitialSessionState.CreateDefault2();

        if (Platform.IsWindows)
        {
            state.ExecutionPolicy = Microsoft.PowerShell.ExecutionPolicy.Unrestricted;
        }

        string functionModulePath = Path.Join(_environment.ContentRootPath, "Modules");
#if !DEBUG
        state.EnvironmentVariables.Add(new SessionStateVariableEntry("PSModulePath", functionModulePath, description: null));
#endif

        Directory.SetCurrentDirectory(_environment.ContentRootPath);

        var powerShell = PowerShell.Create(state);
        powerShell.Streams.Error.DataAdding += LogErrorDataAdding;
        powerShell.Streams.Warning.DataAdding += LogWarningDataAdding;
        powerShell.Streams.Information.DataAdding += LogInformationDataAdding;
        powerShell.Streams.Verbose.DataAdding += LogVerboseDataAdding;
        powerShell.Streams.Debug.DataAdding += LogDebugDataAdding;
        powerShell.Streams.Progress.DataAdding += LogProgressDataAdding;

        return powerShell;
    }

    /// <summary>
    /// Executes the specified PowerShell script asynchronously and returns the resulting HTTP response.
    /// </summary>
    /// <remarks>If the PowerShell script encounters errors during execution, they are logged, but the method
    /// will still return an <see cref="HttpResponse"/> object. In the event of an unhandled exception, the method
    /// returns an <see cref="HttpResponse"/> with a status code of 500 (Internal Server Error).</remarks>
    /// <param name="script">The PowerShell script to execute. This must be a valid script string.</param>
    /// <param name="request">The HTTP request object to pass as a parameter to the script. This can be used by the script for processing.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The operation will be canceled if the token is triggered.</param>
    /// <returns>An <see cref="HttpResponse"/> object representing the result of the script execution. If the script produces an
    /// <see cref="HttpResponse"/> object, it is returned directly. If the script produces another type of object, it is
    /// wrapped in an <see cref="HttpResponse"/> with the object as the body. If no result is produced, an empty <see
    /// cref="HttpResponse"/> is returned.</returns>
    public async Task<HttpResponse> RunScriptAsync(string script, HttpRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _powerShell.Commands?.Clear();
            _powerShell.Streams.Error?.Clear();
            _powerShell.Streams.Warning?.Clear();
            _powerShell.Streams.Information?.Clear();
            _powerShell.Streams.Verbose?.Clear();
            _powerShell.Streams.Debug?.Clear();
            _powerShell.Streams.Progress?.Clear();

            _powerShell.Runspace?.ResetRunspaceState();

            _powerShell.AddScript(script);
            _powerShell.AddParameter("Request", request);

            var results = await _powerShell.InvokeAsync().WaitAsync(cancellationToken).ConfigureAwait(false);

            if (_powerShell.HadErrors)
            {
                string errorMessage = string.Join(Environment.NewLine, _powerShell.Streams.Error!);
                _logger.LogError("PowerShell errors: {message}", errorMessage);
            }

            var lastResult = results?.LastOrDefault();

            return lastResult switch
            {
                var obj when obj?.BaseObject is HttpResponse baseObject => baseObject,
                var obj when obj?.BaseObject is { } baseObject => new HttpResponse { Body = baseObject },
                _ or null => new HttpResponse { Body = string.Empty }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception occurred: {message}", ex.Message);
            return new HttpResponse { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }

    private void LogErrorDataAdding(object? sender, DataAddingEventArgs e)
    {
        if (e.ItemAdded is ErrorRecord record)
        {
            _logger.LogError(record.Exception, "ERROR: {record} {positionMessage}", record.ToString(), record.InvocationInfo.PositionMessage);
        }
    }

    private void LogWarningDataAdding(object? sender, DataAddingEventArgs e)
    {
        if (e.ItemAdded is WarningRecord record)
        {
            _logger.LogWarning("WARNING: {message}", record.Message);
        }
    }

    private void LogInformationDataAdding(object? sender, DataAddingEventArgs e)
    {
        if (e.ItemAdded is InformationRecord record)
        {
            if (record.Tags.Count == 1 && (string.Equals(record.Tags[0], "__PipelineObject__", StringComparison.Ordinal) || string.Equals(record.Tags[0], "PSHOST", StringComparison.Ordinal)))
            {
                _logger.LogInformation("OUTPUT: {messageData}", record.MessageData);
            }
            else
            {
                _logger.LogInformation("INFORMATION: {messageData}", record.MessageData);
            }
        }
    }

    private void LogVerboseDataAdding(object? sender, DataAddingEventArgs e)
    {
        if (e.ItemAdded is VerboseRecord record)
        {
            _logger.LogTrace("VERBOSE: {message}", record.Message);
        }
    }

    private void LogDebugDataAdding(object? sender, DataAddingEventArgs e)
    {
        if (e.ItemAdded is DebugRecord record)
        {
            _logger.LogDebug("DEBUG: {message}", record.Message);
        }
    }

    private void LogProgressDataAdding(object? sender, DataAddingEventArgs e)
    {
        if (e.ItemAdded is ProgressRecord record)
        {
            _logger.LogTrace("PROGRESS: {message}", record.StatusDescription);
        }
    }
}