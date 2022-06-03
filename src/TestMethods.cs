using System.Reflection;

namespace esuite.Ditester;

internal class TestMethods
{
    private Type _parentType;

    private List<MethodInfo> _methods = new();

    public Type ParentType => _parentType;
    public IEnumerable<MethodInfo> Methods => _methods;

    public TestMethods(Type parentType)
    {
        _parentType = parentType;
    }

    public void AddMethod(MethodInfo method) => _methods.Add(method);

    public void AddMethods(IEnumerable<MethodInfo> methods) => _methods.AddRange(methods);

}