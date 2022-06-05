using esuite.Ditester;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var ditester = new Ditester((_, services) =>
{
    //services.AddSingleton<NonInjectedService>();
});

ditester.StartAndRun();

foreach (var tr in ditester.GetResults())
{
    Console.WriteLine(tr);
    if (!tr.Success)
        Console.WriteLine($"Exception:\n{tr.Exception?.Message}\n{tr.Exception?.InnerException?.Message}");
}

// Woops, I forgot to add this service to the IServiceCollection,
// I wonder what might happen...?
public class NonInjectedService
{
    private ILogger _logger;

    public NonInjectedService(ILogger logger) => _logger = logger;

    public void Msg1() => _logger.LogInformation("message 1");
}

public class TestClass : IDitest
{
    private NonInjectedService _nis;

    public TestClass(NonInjectedService nis) => _nis = nis;

    public void Test1() => _nis.Msg1();
}