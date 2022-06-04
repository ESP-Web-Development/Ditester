# Ditester - Dependency Injection Tester

Simple and lightweight library for testing code using the
dependency injection design pattern.

Written in C#, .NET 6.0.

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
await ditester.StartAsync(async tester =>
{
    await tester.RunTestsAsync();
});
```

Easy as pie.

## Features

The dependency injection in this library is done by [`Microsoft.Extensions.DependencyInjection`](https://github.com/dotnet/runtime/tree/main/src/libraries/Microsoft.Extensions.DependencyInjection).

## License

The highly permissive [MIT license](https://github.com/ESP-Web-Development/Ditester/blob/main/LICENSE). Get creative.
