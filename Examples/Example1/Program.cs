using esuite.Ditester;
using esuite.Ditester.Example1;

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