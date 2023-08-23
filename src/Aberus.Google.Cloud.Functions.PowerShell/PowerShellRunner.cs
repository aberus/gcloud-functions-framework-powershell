using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using System.Threading.Tasks;

namespace Aberus.Google.Cloud.Functions.Framework;

public class PowerShellRunner : IPowerShellRunner
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger _logger;

    private readonly PowerShell _powerShell;

    public PowerShellRunner(IWebHostEnvironment environment, ILogger<PowerShellRunner> logger)
    {
        _environment = environment;
        _logger = logger;

        _powerShell = Create();
    }

    private PowerShell Create()
    {
        var accelerator = typeof(PSObject).Assembly.GetType("System.Management.Automation.TypeAccelerators");
        var addMethod = accelerator?.GetMethod("Add", new Type[] { typeof(string), typeof(Type) });
        addMethod?.Invoke(null, new object[] { "HttpResponse", typeof(HttpResponse) });
        addMethod?.Invoke(null, new object[] { "HttpRequest", typeof(HttpRequest) });

        var state = InitialSessionState.CreateDefault2();

        if (Platform.IsWindows)
        {
            state.ExecutionPolicy = Microsoft.PowerShell.ExecutionPolicy.Unrestricted;
        }

        string functionModulePath = Path.Join(_environment.ContentRootPath, "Modules");
        state.EnvironmentVariables.Add(new SessionStateVariableEntry("PSModulePath", functionModulePath, description: null));

        var powerShell = PowerShell.Create(state);
        powerShell.Streams.Error.DataAdding += LogErrorDataAdding;
        powerShell.Streams.Warning.DataAdding += LogWarningDataAdding;
        powerShell.Streams.Information.DataAdding += LogInformationDataAdding;
        powerShell.Streams.Verbose.DataAdding += LogVerboseDataAdding;
        powerShell.Streams.Debug.DataAdding += LogDebugDataAdding;
        powerShell.Streams.Progress.DataAdding += LogProgressDataAdding;

        return powerShell;
    }

    public async Task<HttpResponse> RunScriptAsync(string script, HttpRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _powerShell.Commands?.Clear();
            _powerShell.Streams.Verbose?.Clear();
            _powerShell.Streams.Error?.Clear();
            _powerShell.Runspace?.ResetRunspaceState();

            _powerShell.AddScript(script);
            _powerShell.AddParameter("Request", request);

            var results = await _powerShell.InvokeAsync().WithCancellation(cancellationToken).ConfigureAwait(false);

            if (_powerShell.HadErrors)
            {
                string errorMessage = string.Join(Environment.NewLine, _powerShell.Streams.Error!);
                _logger.LogError("PowerShell errors: {message}", errorMessage);
            }

            var lastResult = results?.LastOrDefault();

            var result = lastResult switch
            {
                var obj when obj?.BaseObject is HttpResponse baseObject => baseObject,
                var obj when obj?.BaseObject is { } baseObject => new HttpResponse { Body = baseObject },
                null => new HttpResponse { Body = string.Empty },
                _ => new HttpResponse { Body = string.Empty },
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception occurred: {message}", ex.Message);
            return new HttpResponse { StatusCode = 500 };
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