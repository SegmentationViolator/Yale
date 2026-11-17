using Yale.Parsing.Grok;

namespace Yale.Parsing.Tests;

public sealed class GrokExceptionTests
{
    [Fact]
    public void Preserves_inner_exception()
    {
        var cause = new InvalidOperationException();
        var error = new GrokException("message", cause);
        Assert.Same(cause, error.InnerException);
    }
}
