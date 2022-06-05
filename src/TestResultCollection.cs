using System.Collections;

namespace esuite.Ditester;

public class TestResultCollection : IEnumerable<TestResult>
{
    private List<TestResult> _results = new();

    private int _total;
    private int _success;

    public int Count => _total;
    public int Success => _success;
    public int Fail => _total - _success;

    public TestResultCollection() {}

    internal void AddResult(TestResult result)
    {
        _results.Add(result);
        _total++;
 
        if (result.Success)
            _success++;
    }

    internal void AddResults(IEnumerable<TestResult> results)
    {
        _results.AddRange(results);
        _total += results.Count();
        _success += results.Count(r => r.Success);
    }

    public IEnumerator<TestResult> GetEnumerator()
    {
        return _results.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}