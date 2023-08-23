using System.Collections;

namespace Aberus.Google.Cloud.Functions.Framework;

/// <summary>
/// Custom type represent the context of the Http response.
/// </summary>
public class HttpResponse
{
    /// <summary>
    /// Gets or sets the Body of the Http response.
    /// </summary>
    public object? Body { get; set; }

    /// <summary>
    /// Gets or sets the ContentType of the Http response.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the Headers of the Http response.
    /// </summary>
    public IDictionary? Headers { get; set; }

    /// <summary>
    /// Gets or sets the StatusCode of the Http response.
    /// </summary>
    public int StatusCode { get; set; } = 200;
}