# Ditester - Dependency Injection Tester

Simple and lightweight library for testing code written using the
dependency injection design pattern.

Written in C#, .NET 6.0.

## Installation

Clone this repository and include the *Ditester.csproj* project reference in your own testing project via `<ProjectReference Include="Ditester/src/Ditester.csproj" />`, like [here](https://github.com/ESP-Web-Development/Ditester/blob/main/Examples/Example1/Example1.csproj).

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

Or, if you are the *oldschool* type

``` C#
// Same as StartAndRunAsync().Wait();
ditester.StartAndRun();
```

### 4. Optional: View your results

``` C#
foreach (var testResult in ditester.GetResults())
{
    Console.WriteLine(testResult);
}
```

Easy as pie.

## About Ditester

The dependency injection in this library is done with [`Microsoft.Extensions.DependencyInjection`](https://github.com/dotnet/runtime/tree/main/src/libraries/Microsoft.Extensions.DependencyInjection), the same as ASP.NET Core. This means that you can also test custom services and controllers and expect identical behavior as in your application.

## License

The highly permissive [MIT license](https://github.com/ESP-Web-Development/Ditester/blob/main/LICENSE). Get creative.
