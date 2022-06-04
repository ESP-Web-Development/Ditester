using esuite.Ditester;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

void Configure(HostBuilderContext builderContext, IServiceCollection services)
{
    // Add your services here.

    services.AddScoped<PrettyMessageService>(); // in Tests.cs
}

var ditester = new Ditester(Configure, true);

await ditester.StartAsync(async tester =>
{
    // An ILogger service is added by default.
    var logger = ditester.RequestService<ILogger>();

    logger?.LogInformation("Starting tests...");
    await tester.RunTests();
});

// Service to be injected (line 11 & 43).
public class PrettyMessageService
{
    private ILogger _logger;

    public PrettyMessageService(ILogger logger) => _logger = logger;

    public void InfoMessage() => _logger.LogInformation("This is a pretty message :)");
    public void ErrorMessage() => _logger.LogError("This is an ugly message :(");
    public void CustomMessage(string msg) => _logger.LogInformation(msg);
}

// Test class - detected automatically by Ditest.
// No need to be explicitly specified anywhere.
class PrettyMessageTest : IDitest
{
    private PrettyMessageService _prettyMsg;

    public PrettyMessageTest(PrettyMessageService prettyMsg) => _prettyMsg = prettyMsg;

    public void Test1() => _prettyMsg.InfoMessage();
    public void Test2() => _prettyMsg.ErrorMessage();

    public Task Test3()
    {
        _prettyMsg.CustomMessage("Message sent from Task.");
        return Task.FromResult(0);
    }
 
    public async Task Test4()
    {
        await Task.Delay(1000);
        _prettyMsg.CustomMessage("Async Task finished successfully.");
    }
}