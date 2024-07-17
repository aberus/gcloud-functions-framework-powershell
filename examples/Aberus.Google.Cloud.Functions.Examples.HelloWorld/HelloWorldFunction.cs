using Aberus.Google.Cloud.Functions.Framework;
using Google.Cloud.Functions.Framework;
using Microsoft.Extensions.Logging;

namespace CloudHttpFunction1
{
    public class HelloWorldFunction : PowerShellFunction
    {
        public HelloWorldFunction(
            IPowerShellRunner powerShellRunner,
            IHttpRequestReader<HttpRequest> requestReader,
            IHttpResponseWriter<HttpResponse> responseWriter,
            ILogger<PowerShellFunction> logger)
            : base(powerShellRunner, requestReader, responseWriter, logger) { }
    }
}
