using System;
using System.Collections.Generic;

namespace Aberus.Google.Cloud.Functions.Framework;

/// <summary>
/// Custom type represent the context of the in-coming HTTP request.
/// </summary>
public class HttpRequest
{
    /// <summary>
    /// Gets the Headers of the HTTP request.
    /// </summary>
    public IDictionary<string, IEnumerable<string?>> Headers { get; set; }

    /// <summary>
    /// Gets the Query of the HTTP request.
    /// </summary>
    public IDictionary<string, string?> Query { get; set; }

    /// <summary>
    /// Gets the Body of the HTTP request.
    /// </summary>
    public object? Body { get; set; }

    /// <summary>
    /// Gets the Url of the HTTP request.
    /// </summary>
    public string Path { get; set; } = "";

    /// <summary>
    /// Gets the Protocol of the HTTP request.
    /// </summary>
    public string Protocol { get; set; } = "";

    /// <summary>
    /// Gets the Method of the HTTP request.
    /// </summary>
    public string Method { get; set; } = "GET";

    /// <summary>
    /// Constructor for <see cref="HttpRequest"/>.
    /// </summary>
    public HttpRequest()
    {
        Headers = new Dictionary<string, IEnumerable<string?>>(StringComparer.OrdinalIgnoreCase);
        Query = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
    }
}