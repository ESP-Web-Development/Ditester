using esuite.Ditester;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var ditester = new Ditester((_, services) =>
{
    services.AddSingleton<Service>();
});

await ditester.StartAsync(async tester =>
{
    await tester.RunTestsAsync();
});

public class Service
{
    private ILogger _logger;

    public Service(ILogger logger) => _logger = logger;

    public void Exception1() => throw new Exception("Exception1");
    public void Exception2() => throw new Exception("Exception2");
    public void Exception3() => _logger.LogInformation("No exception.");
}

public class TestClass : IDitest
{
    private Service _service;

    public TestClass(Service service) => _service = service;

    public void Test1() => _service.Exception1();
    public void Test2() => _service.Exception2();
    public void Test3() => _service.Exception3();
}