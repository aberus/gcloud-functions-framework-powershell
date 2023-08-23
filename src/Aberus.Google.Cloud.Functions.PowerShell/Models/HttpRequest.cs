using System;
using System.Collections.Generic;

namespace Aberus.Google.Cloud.Functions.Framework;

/// <summary>
/// Custom type represent the context of the in-coming Http request.
/// </summary>
public class HttpRequest
{
    /// <summary>
    /// Gets the Headers of the Http request.
    /// </summary>
    public IDictionary<string, string> Headers { get; set; }

    /// <summary>
    /// Gets the Query of the Http request.
    /// </summary>
    public IDictionary<string, string> Query { get; set; }

    /// <summary>
    /// Gets the Cookies of the Http request.
    /// </summary>
    public IDictionary<string, string> Cookies { get; set; }

    /// <summary>
    /// Gets the Body of the Http request.
    /// </summary>
    public object? Body { get; set; }

    /// <summary>
    /// Gets the Url of the Http request.
    /// </summary>
    public string Path { get; set; } = "";

    /// <summary>
    /// Gets the Protocol of the Http request.
    /// </summary>
    public string Protocol { get; set; } = "";

    /// <summary>
    /// Gets the Method of the Http request.
    /// </summary>
    public string Method { get; set; } = "GET";

    /// <summary>
    /// Constructor for <see cref="HttpRequest"/>.
    /// </summary>
    public HttpRequest()
    {
        Query = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Cookies = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}