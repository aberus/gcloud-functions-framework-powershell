using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;

namespace Aberus.Google.Cloud.Functions.Framework;

/// <summary>
/// Provides functionality to read and convert an incoming <see cref="Microsoft.AspNetCore.Http.HttpRequest"/> into a
/// custom <see cref="HttpRequest"/> object.
/// </summary>
/// <remarks>This class is designed to facilitate the extraction of key components from an HTTP request, such as
/// headers,  query parameters, cookies, path, protocol, method, and body, and encapsulate them into a strongly-typed
/// <see cref="HttpRequest"/> object. It is particularly useful in scenarios where custom processing of HTTP  requests
/// is required. <para> Note that the <see cref="ReadRequestAsync(Microsoft.AspNetCore.Http.HttpRequest)"/> method reads
/// the entire  request body into memory. Use caution when handling large request bodies to avoid excessive memory
/// usage. </para></remarks>
public sealed class HttpRequestReader : IHttpRequestReader<HttpRequest>
{
    /// <summary>
    /// Reads the specified <see cref="Microsoft.AspNetCore.Http.HttpRequest"/> and converts it into a custom <see
    /// cref="HttpRequest"/> object.
    /// </summary>
    /// <remarks>This method reads the entire body of the incoming HTTP request into memory. Use caution when
    /// handling large request bodies to avoid excessive memory usage.</remarks>
    /// <param name="httpRequest">The incoming HTTP request to be read and converted.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="HttpRequest"/> object
    /// populated with the headers, query parameters, cookies, path, protocol, method, and body of the specified
    /// <paramref name="httpRequest"/>.</returns>
    public async Task<HttpRequest> ReadRequestAsync(Microsoft.AspNetCore.Http.HttpRequest httpRequest)
    {
        var request = new HttpRequest
        {
            Path = httpRequest.Path,
            Protocol = httpRequest.Protocol,
            Method = httpRequest.Method
        };

        foreach (var pair in httpRequest.Headers)
        {
            request.Headers.TryAdd(pair.Key, httpRequest.Headers.GetCommaSeparatedValues(pair.Key));
        }

        foreach (var pair in httpRequest.Query)
        {
            request.Query.TryAdd(pair.Key, pair.Value);
        }

        await using var memoryStream = new MemoryStream();
        await httpRequest.Body.CopyToAsync(memoryStream, httpRequest.HttpContext.RequestAborted).ConfigureAwait(false);
        request.Body = memoryStream.ToArray();

        return request;
    }
}
