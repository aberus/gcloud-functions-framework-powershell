using Microsoft.AspNetCore.Http;
using System.Collections;

namespace Aberus.Google.Cloud.Functions.Framework;

/// <summary>
/// Custom type represent the context of the HTTP response.
/// </summary>
public class HttpResponse
{
    /// <summary>
    /// Gets or sets the Body of the HTTP response.
    /// </summary>
    public object? Body { get; set; }

    /// <summary>
    /// Gets or sets the Content type of the HTTP response.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the Headers of the HTTP response.
    /// </summary>
    public IDictionary? Headers { get; set; }

    /// <summary>
    /// Gets or sets the Status code of the HTTP response.
    /// </summary>
    public int StatusCode { get; set; } = StatusCodes.Status200OK;
}