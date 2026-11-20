using Yale.Parsing.Grok;
using static Yale.Parsing.Tests.GrokTestHelpers;

namespace Yale.Parsing.Tests;

// Built-ins are exercised through the public GrokPattern API.
public sealed partial class GrokPatternTests
{
    [Theory]
    [InlineData("USERNAME", "user.name")]
    [InlineData("USER", "user.name")]
    [InlineData("EMAILLOCALPART", "alice.smith")]
    [InlineData("EMAILADDRESS", "alice@example.com")]
    [InlineData("INT", "-12")]
    [InlineData("BASE10NUM", "12.5")]
    [InlineData("NUMBER", "12.5")]
    [InlineData("BASE16NUM", "0x1f")]
    [InlineData("BASE16FLOAT", "0x1f.2")]
    [InlineData("POSINT", "1")]
    [InlineData("NONNEGINT", "0")]
    [InlineData("WORD", "hello")]
    [InlineData("NOTSPACE", "hello!")]
    [InlineData("SPACE", " \t")]
    [InlineData("DATA", "hello world")]
    [InlineData("GREEDYDATA", "hello world")]
    [InlineData("QUOTEDSTRING", "\"hello\"")]
    [InlineData("QS", "\"hello\"")]
    [InlineData("UUID", "123e4567-e89b-12d3-a456-426614174000")]
    [InlineData("URN", "urn:example:hello")]
    [InlineData("MAC", "00:1a:2b:3c:4d:5e")]
    [InlineData("CISCOMAC", "001a.2b3c.4d5e")]
    [InlineData("WINDOWSMAC", "00-1a-2b-3c-4d-5e")]
    [InlineData("COMMONMAC", "00:1a:2b:3c:4d:5e")]
    [InlineData("IPV6", "2001:db8::1")]
    [InlineData("IPV4", "192.168.1.1")]
    [InlineData("IP", "192.168.1.1")]
    [InlineData("HOSTNAME", "example.com")]
    [InlineData("IPORHOST", "example.com")]
    [InlineData("HOSTPORT", "example.com:443")]
    [InlineData("PATH", "/var/log")]
    [InlineData("UNIXPATH", "/var/log")]
    [InlineData("TTY", "/dev/pts/1")]
    [InlineData("WINPATH", "C:\\logs\\app.log")]
    [InlineData("URIPROTO", "http")]
    [InlineData("URIHOST", "example.com:443")]
    [InlineData("URIPATH", "/path/to")]
    [InlineData("URIQUERY", "a=1&b=2")]
    [InlineData("URIPARAM", "?a=1&b=2")]
    [InlineData("URIPATHPARAM", "/path?a=1")]
    [InlineData("URI", "http://example.com/path?a=1")]
    [InlineData("MONTH", "December")]
    [InlineData("MONTHNUM", "12")]
    [InlineData("MONTHNUM2", "01")]
    [InlineData("MONTHDAY", "31")]
    [InlineData("DAY", "Monday")]
    [InlineData("YEAR", "2024")]
    [InlineData("HOUR", "23")]
    [InlineData("MINUTE", "59")]
    [InlineData("SECOND", "60")]
    [InlineData("TIME", "23:59:59")]
    [InlineData("DATE_US", "12/31/2024")]
    [InlineData("DATE_EU", "31.12.2024")]
    [InlineData("ISO8601_TIMEZONE", "+01:00")]
    [InlineData("ISO8601_SECOND", "60")]
    [InlineData("TIMESTAMP_ISO8601", "2024-12-31T23:59:59Z")]
    [InlineData("DATE", "12/31/2024")]
    [InlineData("DATESTAMP", "31.12.2024 10:30:00")]
    [InlineData("TZ", "UTC")]
    [InlineData("DATESTAMP_RFC822", "Tue Dec 31 2024 10:30:00 UTC")]
    [InlineData("DATESTAMP_RFC2822", "Tue, 31 Dec 2024 10:30:00 +01:00")]
    [InlineData("DATESTAMP_OTHER", "Tue Dec 31 10:30:00 UTC 2024")]
    [InlineData("DATESTAMP_EVENTLOG", "20241231103000")]
    [InlineData("SYSLOGTIMESTAMP", "Dec 31 10:30:00")]
    [InlineData("PROG", "cron")]
    [InlineData("SYSLOGPROG", "cron[123]")]
    [InlineData("SYSLOGHOST", "example.com")]
    [InlineData("SYSLOGFACILITY", "<3.4>")]
    [InlineData("HTTPDATE", "31/Dec/2024:10:30:00 +0000")]
    [InlineData("SYSLOGBASE", "Dec 31 10:30:00 example.com cron[123]:")]
    [InlineData("LOGLEVEL", "ERROR")]
    public void Built_in_pattern_matches_a_representative_value(string name, string input)
    {
        var pattern = new GrokPattern($"^%{{{name}:value}}$");
        var (fields, _) = Assert.Single(pattern.Parse(Utf8(input)));
        HasValue(fields, "value", input);
    }

    [Theory]
    [InlineData("IPV4", "256.1.1.1")]
    [InlineData("UUID", "not-a-uuid")]
    [InlineData("INT", "12x")]
    [InlineData("POSINT", "0")]
    [InlineData("NONNEGINT", "-1")]
    [InlineData("MONTHNUM", "13")]
    [InlineData("MONTHNUM2", "1")]
    [InlineData("HOUR", "24")]
    [InlineData("MINUTE", "60")]
    [InlineData("SECOND", "61")]
    [InlineData("QUOTEDSTRING", "\"")]
    public void Built_in_pattern_rejects_invalid_whole_input(string name, string input)
    {
        var pattern = new GrokPattern($"^%{{{name}}}$");
        Assert.Empty(pattern.Parse(Utf8(input)));
    }

    [Theory]
    [InlineData("\"\"")]
    [InlineData("''")]
    [InlineData("``")]
    public void Quoted_string_accepts_empty_quoted_values(string input)
    {
        var pattern = new GrokPattern(@"^%{QUOTEDSTRING:value}$");
        var (fields, _) = Assert.Single(pattern.Parse(Utf8(input)));
        HasValue(fields, "value", input);
    }

    [Theory]
    [InlineData("12/31/2024 10:30:00")]
    [InlineData("31.12.2024 10:30:00")]
    public void Datestamp_accepts_us_and_european_dates_with_time(string input)
    {
        var pattern = new GrokPattern(@"^%{DATESTAMP}$");
        Assert.Single(pattern.Parse(Utf8(input)));
    }

    [Theory]
    [InlineData("12/31/2024")]
    [InlineData("31.12.2024")]
    public void Datestamp_requires_a_time(string input)
    {
        var pattern = new GrokPattern(@"^%{DATESTAMP}$");
        Assert.Empty(pattern.Parse(Utf8(input)));
    }

    [Theory]
    [InlineData("USER", "USERNAME", "user.name")]
    [InlineData("QS", "QUOTEDSTRING", "\"hello\"")]
    public void Alias_and_canonical_name_match_the_same_value(string alias, string canonical, string input)
    {
        var firstPattern = new GrokPattern($"^%{{{alias}:value}}$");
        var secondPattern = new GrokPattern($"^%{{{canonical}:value}}$");
        var (first, _) = Assert.Single(firstPattern.Parse(Utf8(input)));
        var (second, _) = Assert.Single(secondPattern.Parse(Utf8(input)));
        HasValue(first, "value", input);
        Assert.Equal(first["value"], second["value"]);
    }
    [Theory]
    [InlineData("SYSLOGPROG", "cron[123]", "process", "cron")]
    [InlineData("SYSLOGPROG", "cron[123]", "pid", "123")]
    [InlineData("SYSLOGFACILITY", "<3.4>", "facility", "3")]
    [InlineData("SYSLOGFACILITY", "<3.4>", "priority", "4")]
    [InlineData("SYSLOGBASE", "Dec 31 10:30:00 example.com cron[123]:", "logsource", "example.com")]
    public void Built_in_captures_follow_upstream_names_and_types(string name, string input, string field, string expected)
    {
        var pattern = new GrokPattern($"^%{{{name}}}$");
        var (fields, _) = Assert.Single(pattern.Parse(Utf8(input)));
        HasValue(fields, field, expected);
    }
}
