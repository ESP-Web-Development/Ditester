# ![Ditester](DitesterIconSmall.png) Ditester - Dependency Injection Tester

Simple and lightweight library for testing code written using the
dependency injection design pattern.

Written in C#, .NET 6.0.

## Installation

### NuGet

Ditester is on NuGet as [`esuite.Ditester`](https://www.nuget.org/packages/esuite.Ditester/0.0.9). Install it from your package manager, or use the .NET CLI:
> `dotnet add package esuite.Ditester`

### Clone

Clone this repository and include the *Ditester.csproj* project reference in your own testing project via `<ProjectReference Include="Ditester/src/Ditester.csproj" />`, like [here](https://github.com/ESP-Web-Development/Ditester/blob/main/Examples/Example1/Example1.csproj).

Please note that if your IDE does not resolve the dependencies automatically when you include the project reference, all you have to do is install [Microsoft.Extensions.Hosting via Nuget](https://www.nuget.org/packages/Microsoft.Extensions.Hosting).

## Usage

### 1. Create an instance

``` C#
var ditester = new Ditester((_, services) =>
{
    services.AddSingleton<IMyService, MyService>();
});
```

### 2. Create a test class

``` C#
public class MyTestClass : IDitest
{
    private IMyService _myService;

    public MyTestClass(IMyService myService)
        => _myService = myService;

    public void Test1() =>
        _myService.DoSomething();

    public async Task Test2() =>
        await _myService.DoSomethingAsync();
}
```

### 3. Run your tests

``` C#
await ditester.StartAndRunAsync();
```

Or, if you are the hands-on type:

``` C#
await ditester.StartAsync(async tester =>
{
    var logger = ditester.RequestService<ILogger>();
    logger?.LogInformation("Starting testing!");

    await tester.RunAsync(log: false);
});
```

Or, if you are the *old school* type (**yikes**):

``` C#
ditester.StartAndRun();
// Don't worry, async test methods will still be run.
```

### 4. Optional: View the results

``` C#
foreach (var testResult in ditester.GetResults())
{
    Console.WriteLine(testResult);
}
```

Easy as pie.

## About Ditester

The dependency injection in this library is done with the help of [`Microsoft.Extensions.DependencyInjection`](https://github.com/dotnet/runtime/tree/main/src/libraries/Microsoft.Extensions.DependencyInjection), the same as ASP.NET Core. This means that you can also test custom services and controllers written for ASP.NET Core and expect identical behavior as in your application.

There's no need to set up an [`IHost`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.ihost). Ditester already does that and it also exposes the [`HostBuilderContext`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.hostbuildercontext) to the user during instantiation. Keep in mind that the `IHost` never actually gets started and it is used internally solely for its [`IServiceProvider`](https://docs.microsoft.com/en-us/dotnet/api/system.iserviceprovider).

This library is meant to be kept small and simple to use, and currently there are no other features on the TODO list. At the moment, any users that would like to see new major features are encouraged to start their own fork of this project.

## License

The highly permissive [MIT license](https://github.com/ESP-Web-Development/Ditester/blob/main/LICENSE). Get creative.
