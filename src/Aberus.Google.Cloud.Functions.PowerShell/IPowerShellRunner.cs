﻿using System.Threading;
using System.Threading.Tasks;

namespace Aberus.Google.Cloud.Functions.Framework;

/// <summary>
/// Defines a contract for executing PowerShell scripts in the context of an HTTP request.
/// </summary>
/// <remarks>Implementations of this interface provide functionality to execute PowerShell scripts asynchronously
/// and return the resulting HTTP response. Ensure that the provided scripts are valid and compatible with the execution
/// environment.</remarks>
public interface IPowerShellRunner
{
    /// <summary>
    /// Executes the specified script asynchronously and returns the resulting HTTP response.
    /// </summary>
    /// <remarks>The method executes the provided script in the context of the given HTTP request.  Ensure
    /// that the script is valid and compatible with the execution environment.</remarks>
    /// <param name="script">The script to execute. Must be a valid script supported by the system.</param>
    /// <param name="request">The HTTP request context in which the script is executed. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. Passing a canceled token will terminate the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response generated by the
    /// script execution.</returns>
    Task<HttpResponse> RunScriptAsync(string script, HttpRequest request, CancellationToken cancellationToken);
}