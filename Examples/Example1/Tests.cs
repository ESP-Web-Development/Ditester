using Microsoft.Extensions.Logging;

namespace esuite.Ditester.Example1;

public class PrettyMessageService
{
    private ILogger _logger;

    public PrettyMessageService(ILogger logger) => _logger = logger;

    public void InfoMessage() => _logger.LogInformation("This is a pretty message :)");
    public void ErrorMessage() => _logger.LogError("This is an ugly message :(");
}

public class PrettyMessageTest : IDitest
{
    private PrettyMessageService _prettyMsg;

    public PrettyMessageTest(PrettyMessageService prettyMsg) => _prettyMsg = prettyMsg;

    public void Test1() => _prettyMsg.InfoMessage();
    void Test2() => _prettyMsg.ErrorMessage();
}