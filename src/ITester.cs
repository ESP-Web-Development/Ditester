namespace esuite.Ditester;

public interface ITester
{
    /// <summary>
    /// Starts running all <see cref="esuite.Ditester.IDitest" />s.
    /// </summary>
    /// <returns></returns>
    Task RunTests();

    /// <summary>
    /// Whether tests are running.
    /// </summary>
    /// <value></value>
    bool Running { get; }

    /// <summary>
    /// Whether tests are completed.
    /// </summary>
    /// <value></value>
    bool Completed { get; }

    /// <summary>
    /// How many tests were run successfully.
    /// </summary>
    /// <value></value>
    int Successful { get; }

    /// <summary>
    /// How many tests have failed.
    /// </summary>
    /// <value></value>
    int Failed { get; }

    /// <summary>
    /// Total number of identified tests.
    /// </summary>
    /// <value></value>
    int Total { get; }

    /// <summary>
    /// Sorts all classes containing testing methods.
    /// </summary>
    /// <param name="compare"></param>
    void SortTestClasses(Func<string, string, int> compare);

    /// <summary>
    /// Sorts all methods within a test class.
    /// </summary>
    /// <param name="compare"></param>
    void SortTestMethods(Func<string, string, int> compare);
}