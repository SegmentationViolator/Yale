namespace Yale.Parsing.Grok;

[Serializable]
public class GrokException : Exception
{
    public GrokException() : base() { }
    public GrokException(string message) : base(message) { }
    public GrokException(string message, Exception inner) : base(message, inner) { }
}
