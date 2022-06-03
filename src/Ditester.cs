using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace esuite.Ditester
{
    public class Ditester : IDisposable
    {
        public enum TestType
        {
            Singleton, Transient, Scoped
        }

        const string _defaultConfigJsonPath = "appsettings.json";

        private bool _disposed = false;

        private bool _throw;
        private IHostBuilder _hostBuilder;
        private IHost? _host;
        private Action<HostBuilderContext, IServiceCollection> _configureDelegate;

        private IEnumerable<Type>? _tests;

        public Ditester(Action<HostBuilderContext, IServiceCollection> configureDelegate, bool @throw = false)
            : this(new string[] {}, null, configureDelegate, @throw) {}

        public Ditester(string[] args, Action<HostBuilderContext, IServiceCollection> configureDelegate, bool @throw = false)
            : this(args, null, configureDelegate, @throw) {}

        public Ditester(string[] args, string? configJsonPath, Action<HostBuilderContext, IServiceCollection> configureDelegate, bool @throw = false)
        {
            _throw = @throw;
            _configureDelegate = configureDelegate;
            _hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(hostConfig =>
                {
                    if (!string.IsNullOrEmpty(configJsonPath))
                        hostConfig.AddJsonFile(configJsonPath);
                });
        }

        public IEnumerable<Type> IdentifyTests()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsAssignableTo(typeof(IDitest)) && t != typeof(IDitest));
        }

        public async Task StartAsync(Func<ITester, Task> start)
        {
            _tests = IdentifyTests();

            _host = _hostBuilder.ConfigureServices((builder, services) =>
            {
                services.AddSingleton<Tester>();
                services.AddSingleton<ILogger, Logger<Tester>>();

                if (_tests is not null)
                    foreach (var test in _tests)
                        services.AddTransient(test);

                _configureDelegate.Invoke(builder, services);
            })
            .Build();

            var provider = GetProvider();

            var instance = provider.GetRequiredService<Tester>();
            instance.ThrowOnFail = _throw;
            instance.ServiceProvider = GetProvider();
            instance.AddTestClasses(_tests);

            await start.Invoke(instance);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
                _host?.Dispose();
 
            _disposed = true;
        }

        private IServiceProvider GetProvider() => _host?.Services!;
    }
}