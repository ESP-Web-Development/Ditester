namespace esuite.Ditester;

public class DitesterException : Exception
{
    const string ENTRYASSEMBLY = "Cannot get entry assembly (Assembly.GetEntryAssembly).";
    const string TESTERINCPROP = "Cannot get property {0} of ITester because testing is incomplete (ITester.Complete).";
    const string TESTERINCRES = "Cannot get test results because testing has not completed.";
    const string NOTSTARTED = "Ditester has not yet been started.";

    internal DitesterException(string message) : base(message) {}

    internal static DitesterException CannotGetEntryAssembly() => new(ENTRYASSEMBLY);

    internal static DitesterException TesterIncompleteProperty(string propName)
        => new(string.Format(TESTERINCPROP, propName));

    internal static DitesterException TesterIncompleteResults() => new(TESTERINCRES);

    internal static DitesterException DitesterNotStarted() => new(NOTSTARTED);
}