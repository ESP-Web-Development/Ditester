using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Diagnostics;

using Microsoft.Extensions.Logging;

namespace esuite.Ditester;

internal class Tester : ITester
{
    private ILogger _logger;
    private List<TestMethods> _testMethodsList = new();
    private int _totalTests = 0;
    private int _runTests = 0;
    private int _successTests = 0;

    // Public properties (ITester)
    public bool Running { get; private set; }
    public bool Completed { get; private set; }
    public int Successful
    {
        get
        {
            if (!Completed)
                throw DitesterException.TesterIncompleteProperty(nameof(Successful));

            return _successTests;
        }
    }
    public int Failed
    {
        get
        {
            if (!Completed)
                throw DitesterException.TesterIncompleteProperty(nameof(Failed));

            return _runTests - _successTests;
        }
    }
    public int Total => _totalTests;

    // Implementation properties
    public bool ThrowOnFail { get; set; }
    public IServiceProvider? ServiceProvider { get; set; }

    public Tester(ILogger logger)
    {
        _logger = logger;
    }

    public async Task RunTestsAsync()
    {
        if (ServiceProvider is null)
            return;

        foreach (var typeMethod in _testMethodsList)
        {
            var instance = ServiceProvider.GetService(typeMethod.ParentType);
            if (instance is null)
                continue;
 
            foreach (var method in typeMethod.Methods)
            {
                _runTests++;
 
                if (await RunMethod(method, instance, typeMethod.ParentType))
                    _successTests++;
            }
        }
    }

    public T? RequestService<T>() where T : class
    {
        return ServiceProvider?.GetService(typeof(T)) as T;
    }

    public void SortTestClasses(Func<string, string, int> compare)
    {
        _testMethodsList.Sort((x, y) => compare(x.ParentType.Name, y.ParentType.Name));
    }

    public void SortTestMethods(Func<string, string, int> compare)
    {
        foreach (var testMethods in _testMethodsList)
            testMethods.SortMethods(compare);
    }

    // Public because it is called by Ditester.
    // It is not included in a constructor in order to
    // simplify injection of ITester based on this instance.
    public void AddTestClasses(IEnumerable<Type>? types)
    {
        _testMethodsList = GetTestMethods(types ?? Enumerable.Empty<Type>());
        _totalTests = _testMethodsList.Sum(tm => tm.MethodsCount);
    }

    private List<TestMethods> GetTestMethods(IEnumerable<Type> testClasses)
    {
        var foundTypeMethods = new List<TestMethods>();

        foreach (var testClass in testClasses)
        {
            var testMethods = new TestMethods(testClass);

            var methods = testClass.GetMethods();
            foreach (var method in methods)
            {
                if (IsValidMethod(method))
                    testMethods.AddMethod(method);
            }

            foundTypeMethods.Add(testMethods);
        }

        return foundTypeMethods;
    }

    private bool IsValidMethod(MethodInfo methodInfo)
    {
        // Valid methods are parameterless and should return
        // void, Task or types derived from Task.

        if (methodInfo.GetParameters().Length != 0)
            return false;

        return
            methodInfo.ReturnType == typeof(void) ||
            methodInfo.ReturnType == typeof(Task) ||
            methodInfo.ReturnType.IsAssignableTo(typeof(Task));
    }

    private async Task<bool> RunMethod(MethodInfo method, object objInstance, Type objType)
    {
        try
        {
            if (IsAsyncMethod(method))
                await InvokeAsyncMethod(method, objInstance)!;
            else
                InvokeMethod(method, objInstance);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to run {method.Name} from {objType.Name}.");

            var stackTrace = new StackTrace(ex, true);
            // 0 - RunMethod, 1 - Invoke(Async)Method, 2 - The method
            var frame = stackTrace.GetFrame(2);

            string methodName = frame?.GetMethod()?.Name ?? "notfound";
            string fileName = frame?.GetFileName() ?? "notfound";
            int lineNo = frame?.GetFileLineNumber() ?? 0;
            int columnNo = frame?.GetFileColumnNumber() ?? 0;
 
            string exInfo = $"Error at {fileName}:{lineNo}:{columnNo} in method {methodName}";
            _logger.LogInformation(exInfo);

            if (ThrowOnFail)
                throw;
        }
 
        return false;
    }

    private void InvokeMethod(MethodInfo method, object objInstance)
    {
        method.Invoke(objInstance, null);
    }

    private Task? InvokeAsyncMethod(MethodInfo method, object objInstance)
    {
        return method.Invoke(objInstance, null) as Task;
    }

    private static bool IsAsyncMethod(MethodInfo method)
    {
        var returnType = method.ReturnType;
        var asyncAttribute = typeof(AsyncStateMachineAttribute);
        var attribute = method.GetCustomAttribute(asyncAttribute) as AsyncStateMachineAttribute;

        return returnType.IsAssignableTo(typeof(Task)) && attribute is not null;
    }
}