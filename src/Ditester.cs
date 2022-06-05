using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace esuite.Ditester
{
    public class Ditester : IDisposable
    {
        const string _defaultConfigJsonPath = "appsettings.json";

        private bool _started = false;
        private bool _disposed = false;

        private bool _throw;
        private IHostBuilder _hostBuilder;
        private IHost? _host;
        private Action<HostBuilderContext, IServiceCollection> _configureDelegate;

        private IEnumerable<Type>? _tests;

        /// <summary>
        /// Create a new Ditester instance.
        /// </summary>
        /// <param name="configureDelegate">
        /// Function that configures the <see cref="Microsoft.Extensions.Hosting.HostBuilderContext" /> and
        /// <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </param>
        /// <param name="throw">Whether to continue throwing exceptions thrown by tests.</param>
        public Ditester(Action<HostBuilderContext, IServiceCollection> configureDelegate, bool @throw = false)
            : this(new string[] {}, null, configureDelegate, @throw) {}

        /// <summary>
        /// Create a new Ditester instance.
        /// </summary>
        /// <param name="args">
        /// Arguments to be passed to the underlying <see cref="Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(string[])" />.
        /// </param>
        /// <param name="configureDelegate">
        /// Function that configures the <see cref="Microsoft.Extensions.Hosting.HostBuilderContext" /> and
        /// <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </param>
        /// <param name="throw">Whether to continue throwing exceptions thrown by tests.</param>
        public Ditester(string[] args, Action<HostBuilderContext, IServiceCollection> configureDelegate, bool @throw = false)
            : this(args, null, configureDelegate, @throw) {}

        /// <summary>
        /// Create a new Ditester instance.
        /// </summary>
        /// <param name="args">
        /// Arguments to be passed to the underlying <see cref="Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(string[])" />
        /// </param>
        /// <param name="configJsonPath">
        /// JSON file path to be used as host configuration and to be injected as <see cref="Microsoft.Extensions.Configuration.IConfiguration" />.
        /// Default: <c>appsettings.json</c>.
        /// </param>
        /// <param name="configureDelegate">
        /// Function that configures the <see cref="Microsoft.Extensions.Hosting.HostBuilderContext" /> and
        /// <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </param>
        /// <param name="throw">Whether to continue throwing exceptions thrown by tests.</param>
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

        /// <summary>
        /// Start the Dependency Injection Tester (Ditester).
        /// </summary>
        /// <param name="start">
        /// Async function that exposes the <see cref="esuite.Ditester.ITester" /> to the user.
        /// </param>
        /// <returns></returns>
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
            instance.ServiceProvider = provider;
            instance.AddTestClasses(_tests);

            _started = true;

            await start.Invoke(instance);
        }

        /// <summary>
        /// Start the Dependency Injection Tester (Ditester)
        /// and run testing.
        /// </summary>
        /// <returns></returns>
        public Task StartAndRunAsync()
        {
            return StartAsync(async itester =>
            {
                await itester.RunTestsAsync();
            });
        }

        /// <summary>
        /// Start the Dependency Injection Tester (Ditester)
        /// and run testing synchronously.
        /// </summary>
        public void StartAndRun()
        {
            StartAndRunAsync().Wait();
        }

        /// <summary>
        /// Get the results after testing.
        /// </summary>
        /// <exception cref="DitesterException">
        /// Thrown if this instance has not been started or
        /// the <see cref="ITester" /> has not been run.
        /// </exception>
        /// <returns></returns>
        public TestResultCollection GetResults()
        {
            if (!_started)
                throw DitesterException.DitesterNotStarted();

            return GetTesterInstance().GetResults();
        }

        /// <summary>
        /// Get service of type <c>T</c>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>
        /// The service instance or null if there is no service of type <c>T</c>.
        /// </returns>
        public T? RequestService<T>() where T : class
        {
            return GetProvider().GetService<T>();
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

        private Tester GetTesterInstance()
        {
            return GetProvider().GetRequiredService<Tester>();
        }

        private static IEnumerable<Type> IdentifyTests()
        {
            var assembly = Assembly.GetEntryAssembly(); 

            if (assembly is null)
                throw DitesterException.CannotGetEntryAssembly();

            return assembly.GetTypes().Where(t => IsValidType(t));
        }

        private static bool IsValidType(Type t)
        {
            return
                // Types should be able to be instantiated:
                t.GetConstructors().Length != 0 &&
                !t.IsAbstract &&
                !t.IsInterface &&
                // We only care about IDitest implementations:
                t.IsAssignableTo(typeof(IDitest)) &&
                t != typeof(IDitest);
        }
    }
}