using System.Reflection;

namespace esuite.Ditester;

public class TestResult
{
    const string ToStringSuccess = "Test {0} from {1} was successful.";
    const string ToStringFail = "Test {0} from {1} has failed.";

    private Type _testClassType;
    private MethodInfo _testMethod;
    private bool _success;

    public string TestClassName => _testClassType.Name;
    public string TestMethodName => _testMethod.Name;
    public Exception? Exception { get; internal set; }
    public bool Success
    {
        get => _success;
        internal set => _success = value;
    }

    internal TestResult(Type testClassType, MethodInfo testMethod, bool success)
    {
        _testClassType = testClassType;
        _testMethod = testMethod;
        _success = success;
    }

    public override string ToString()
    {
        return string.Format(
            _success ? ToStringSuccess : ToStringFail,
            TestMethodName,
            TestClassName
        );
    }
}