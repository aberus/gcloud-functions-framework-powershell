using Aberus.Google.Cloud.Functions.Framework;
using Microsoft.Extensions.Logging;

namespace Aberus.Google.Cloud.Functions.Examples.PSUntypedEventFunction;

public class Function(
    IPowerShellRunner powerShellRunner,
    ILogger<PowerShellFunction> logger) : PowerShellFunction(powerShellRunner, logger)
{
}