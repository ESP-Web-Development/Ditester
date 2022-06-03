using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Diagnostics;

using Microsoft.Extensions.Logging;

namespace esuite.Ditester;

internal class Tester : ITester
{
    private ILogger _logger;
    private IEnumerable<Type> _tests = Enumerable.Empty<Type>();
    private int _runTests = 0;
    private int _successTests = 0;

    public bool ThrowOnFail { get; set; }
    public IServiceProvider? ServiceProvider { get; set; }

    public Tester(ILogger logger)
    {
        _logger = logger;
    }

    public async Task RunTests()
    {
        if (ServiceProvider is null)
            return;

        var typeMethods = GetMethods();

        foreach (var typeMethod in typeMethods)
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

    public void AddTestClasses(IEnumerable<Type>? types)
    {
        var builder = new StringBuilder("Tester:");

        _tests = types ?? Enumerable.Empty<Type>();

        foreach (var type in _tests)
            builder.AppendLine($"Adding test class \"{type.Name}\"");
    }

    private IEnumerable<TestMethods> GetMethods()
    {
        var foundTypeMethods = new List<TestMethods>();

        foreach (var testClass in _tests)
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
        if (methodInfo.GetParameters().Length != 0)
            return false;

        return
            methodInfo.ReturnType == typeof(void) ||
            methodInfo.ReturnType == typeof(Task) ||
            methodInfo.ReturnType.IsAssignableFrom(typeof(Task));
    }

    private async Task<bool> RunMethod(MethodInfo method, object objInstance, Type objType)
    {
        try
        {
            if (IsAsyncMethod(method))
                await InvokeAsyncMethod(method, objInstance)!;
            else
                method.Invoke(objInstance, null);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to run {method.Name} from {objType.Name}.");

            var stackTrace = new StackTrace(ex, true);
            var frame = stackTrace.GetFrame(1);

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

    private Task? InvokeAsyncMethod(MethodInfo method, object objInstance)
    {
        var result = method.Invoke(objInstance, null) as Task;
        return result;
    }

    private static bool IsAsyncMethod(MethodInfo method)
    {
        var returnType = method.ReturnType;
        var asyncAttribute = typeof(AsyncStateMachineAttribute);
        var attribute = method.GetCustomAttribute(asyncAttribute) as AsyncStateMachineAttribute;

        return returnType.IsAssignableTo(typeof(Task)) && attribute is not null;
    }
}