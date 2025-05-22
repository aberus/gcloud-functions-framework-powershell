using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Aberus.Google.Cloud.Functions.Framework;

/// <summary>
/// Provides functionality to write an <see cref="HttpResponse"/> to an <see
/// cref="Microsoft.AspNetCore.Http.HttpResponse"/> object.
/// </summary>
/// <remarks>This class is responsible for mapping the properties of an <see cref="HttpResponse"/> instance, such
/// as status code, headers, cookies, and body content, to an <see cref="Microsoft.AspNetCore.Http.HttpResponse"/>
/// object in an ASP.NET Core context. It supports writing plain text, binary data, or JSON-serializable objects as the
/// response body.</remarks>
public sealed class HttpResponseWriter : IHttpResponseWriter<HttpResponse>
{
    private static readonly JsonSerializerOptions s_indentedWriterOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Writes the specified <see cref="HttpResponse"/> to the provided <see
    /// cref="Microsoft.AspNetCore.Http.HttpResponse"/>.
    /// </summary>
    /// <remarks>This method sets the status code, headers, and cookies on the provided <paramref
    /// name="httpResponse"/> based on the values in <paramref name="functionResponse"/>. It also writes the body
    /// content, if present, using the appropriate content type. Supported body types include strings, byte arrays, and
    /// objects  (serialized as JSON).</remarks>
    /// <param name="httpResponse">The HTTP response object to which the data will be written.</param>
    /// <param name="functionResponse">The response data to write, including status code, headers, cookies, and body content.</param>
    /// <returns>A task to indicate when the request is complete.</returns>
    public async Task WriteResponseAsync(Microsoft.AspNetCore.Http.HttpResponse httpResponse, HttpResponse functionResponse)
    {
        httpResponse.StatusCode = functionResponse.StatusCode;

        foreach (DictionaryEntry header in functionResponse.Headers ?? ImmutableDictionary<string, object>.Empty)
        {
            if (header.Key is string stringKey)
            {
                if (header.Value is string stringValue)
                {
                    var stringValuesParsed = stringValue.Split(',').Select(p => p.Trim()).OrderBy(item => item, StringComparer.Ordinal);
                    httpResponse.Headers.TryAdd(stringKey, new StringValues([.. stringValuesParsed]));
                }
                else if (header.Key is IEnumerable<string> stringValues)
                {
                    httpResponse.Headers.TryAdd(stringKey, new StringValues([.. stringValues]));
                }
                else if (header.Value is IEnumerable enumerable)
                {
                    var stringValuesParsed = enumerable.Cast<object>().Select(p => p?.ToString() ?? string.Empty).OrderBy(item => item, StringComparer.Ordinal);
                    httpResponse.Headers.TryAdd(stringKey, new StringValues([.. stringValuesParsed]));
                }
                else
                {
                    httpResponse.Headers.TryAdd(stringKey, header.Value?.ToString());
                }
            }
        }

        if (functionResponse.ContentType is { } contentType)
        {
            httpResponse.ContentType = contentType;
        }

        switch (functionResponse.Body)
        {
            case string { } text:
                httpResponse.ContentType ??= MediaTypeNames.Text.Plain;
                await httpResponse.WriteAsync(text, httpResponse.HttpContext.RequestAborted).ConfigureAwait(false);
                break;
            case byte[] { } bytes:
                httpResponse.ContentType ??= MediaTypeNames.Application.Octet;
                await httpResponse.Body.WriteAsync(bytes, httpResponse.HttpContext.RequestAborted).ConfigureAwait(false);
                break;
            case { } body:
                try
                {
                    httpResponse.ContentType ??= MediaTypeNames.Application.Json;
                    var responseStream = httpResponse.Body;
                    await JsonSerializer.SerializeAsync(responseStream, body, s_indentedWriterOptions, httpResponse.HttpContext.RequestAborted)
                                        .ConfigureAwait(false);
                    await responseStream.FlushAsync(httpResponse.HttpContext.RequestAborted).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (httpResponse.HttpContext.RequestAborted.IsCancellationRequested) { }

                break;
        }
    }
}