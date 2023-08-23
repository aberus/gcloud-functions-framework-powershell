using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Collections.Immutable;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Aberus.Google.Cloud.Functions.Framework;

public sealed class HttpResponseWriter : IHttpResponseWriter<HttpResponse>
{
    private static readonly JsonSerializerOptions s_indentedWriterOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    public async Task WriteResponseAsync(Microsoft.AspNetCore.Http.HttpResponse httpResponse, HttpResponse functionResponse)
    {
        httpResponse.StatusCode = functionResponse.StatusCode;

        foreach (DictionaryEntry header in functionResponse.Headers ?? ImmutableDictionary<string, object>.Empty)
        {
            if (header.Key is string stringKey)
            {
                httpResponse.Headers[stringKey] = header.Value?.ToString();
            }
        }

        if (functionResponse.ContentType is { } contentType)
        {
            httpResponse.ContentType = contentType;
        }

        switch (functionResponse.Body)
        {
            case string { } text:
                httpResponse.ContentType ??= "text/plain";
                await httpResponse.WriteAsync(text, httpResponse.HttpContext.RequestAborted).ConfigureAwait(false);
                break;
            case byte[] { } bytes:
                httpResponse.ContentType ??= "application/octet-stream";
                await httpResponse.Body.WriteAsync(bytes, httpResponse.HttpContext.RequestAborted).ConfigureAwait(false);
                break;
            case { } body:
                try
                {
                    httpResponse.ContentType ??= "application/json";
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