using System.Threading;
using System.Threading.Tasks;

namespace Aberus.Google.Cloud.Functions.Framework;

public interface IPowerShellRunner
{
    Task<HttpResponse> RunScriptAsync(string script, HttpRequest request, CancellationToken cancellationToken);
}