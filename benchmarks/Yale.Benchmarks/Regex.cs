using System.Text;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using PCRE;

[MemoryDiagnoser]
public partial class RegexBenchmarks
{
    private const string _pattern = @"(?<month>(?:(?:0[1-9])|(?:[12][0-9])|(?:3[01])|[1-9]))-(?<day>(?:(?:0[1-9])|(?:[12][0-9])|(?:3[01])|[1-9]))-(?<year>(?:(?:0[1-9])|(?:[12][0-9])|(?:3[01])|[1-9])) (?<![0-9])(?<timestamp>(?:2[0-3]|[01]?[0-9]):[0-5][0-9](?::(?:[0-5]?[0-9]|60)(?:[:.,][0-9]+)?))(?![0-9]);\b(?<id>\w+)\b;(?<loglevel>[Aa]lert|ALERT|[Tt]race|TRACE|[Dd]ebug|DEBUG|[Nn]otice|NOTICE|[Ii]nfo|INFO|[Ww]arn?(?:ing)?|WARN?(?:ING)?|[Ee]rr?(?:or)?|ERR?(?:OR)?|[Cc]rit?(?:ical)?|CRIT?(?:ICAL)?|[Ff]atal|FATAL|[Ss]evere|SEVERE|EMERG(?:ENCY)?|[Ee]merg(?:ency)?);\b(?<func>\w+)\b;(?<msg>.*)";

    private byte[] _input = null!;

    private Regex _compiled = null!;
    private Regex _generated = null!;
    private int[] _groupNumbers = null!;

    private PcreRegexUtf8 _pcreCompiled = null!;
    private PcreMatchBuffer8Bit _pcreBuffer = null!;

    [GeneratedRegex(RegexBenchmarks._pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex SourceGeneratedRegex();

    [Params(10_000)]
    public int LineCount;

    [GlobalSetup]
    public void Setup()
    {
        this._compiled = new Regex(
            RegexBenchmarks._pattern,
            RegexOptions.Compiled | RegexOptions.CultureInvariant
        );

        this._generated = SourceGeneratedRegex();

        this._pcreCompiled = new PcreRegexUtf8(
            RegexBenchmarks._pattern,
            PcreOptions.Compiled | PcreOptions.Ucp
        );

        this._groupNumbers = this._compiled.GetGroupNumbers();

        this._pcreBuffer = this._pcreCompiled.PatternInfo.GroupNames.CreateMatchBuffer();

        string[] samples =
        {
            "11-10-26 10:55:01.123;a1b2c3;INFO;Main;Service started",
            "11-10-26 10:55:02;userthis._42;WARN;Parse;Unexpected token near line 12",
            "1-5-26 9:03:44,500;req7;Error;Handler;Request failed with status 500",
            "11-10-26 23:59:59;job9;DEBUG;Worker;Heartbeat ok",
            "11-10-26 10:55:03;id5;INFO;Main;Größe geändert ✓",
            "this line does not match the log format at all",
        };

        string input = string.Join(
            "\n",
            Enumerable.Range(0, LineCount)
                .Select(i => samples[i % samples.Length]));

        this._input = Encoding.UTF8.GetBytes(input);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        this._pcreBuffer?.Dispose();
    }

    [Benchmark]
    public long SystemRegexSourceGenerated()
    {
        string input = Encoding.UTF8.GetString(this._input);
        long checksum = 0;

        Regex regex = SourceGeneratedRegex();

        for (Match m = regex.Match(input); m.Success; m = m.NextMatch())
        {
            foreach (int n in this._groupNumbers)
            {
                if (n == 0)
                    continue;

                Group g = m.Groups[n];
                if (g.Success)
                    checksum += n + 1L;
            }
        }

        return checksum;
    }

    [Benchmark(Baseline = true)]
    public long SystemRegex()
    {
        string input = Encoding.UTF8.GetString(this._input);
        long checksum = 0;

        for (Match m = this._compiled.Match(input); m.Success; m = m.NextMatch())
        {
            foreach (int n in this._groupNumbers)
            {
                if (n == 0) continue;

                Group g = m.Groups[n];
                if (g.Success)
                    checksum += n + 1L;
            }
        }

        return checksum;
    }

    [Benchmark]
    public long PcreNet()
    {
        long checksum = 0;

        foreach (var match in this._pcreBuffer.Matches(this._input, 0, PcreMatchOptions.NoUtfCheck, null))
        {
            for (int n = 1; n <= match.CaptureCount; n++)
            {
                if (match.TryGetGroup(n, out var group) && group.Success)
                    checksum += n + 1L;
            }
        }

        return checksum;
    }
}
