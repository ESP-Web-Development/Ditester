namespace esuite.Ditester;

public class DitesterException : Exception
{
    const string ENTRYASSEMBLY = "Cannot get entry assembly (Assembly.GetEntryAssembly).";
    const string TESTERINCPROP = "Cannot get property {0} of ITester because testing is incomplete (ITester.Complete).";

    public DitesterException(string message) : base(message) {}

    internal static DitesterException CannotGetEntryAssembly() => new(ENTRYASSEMBLY);

    internal static DitesterException TesterIncompleteProperty(string propName)
        => new(string.Format(TESTERINCPROP, propName));
}