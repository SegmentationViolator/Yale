using System.Collections.Immutable;

namespace Yale.Parsing.Grok;

internal readonly struct GrokNamedPattern(
    string pattern,
    ImmutableArray<(string Identifier, GrokType Type)> typeMap)
{
    public readonly string Pattern { get; } = pattern;
    public readonly ImmutableArray<(string Identifier, GrokType Type)> TypeMap { get; } = typeMap;
}

internal static class GrokNamedPatterns
{
    // Basic patterns
    private const string USERNAME = """[a-zA-Z0-9.\_-]+""";
    private const string EMAILLOCALPART = """[a-zA-Z0-9!#$%&'\*+\-/=?^\_\`{|}\~]{1,64}(?:\.[a-zA-Z0-9!#$%&'\*+\-/=?^\_\`{|}\~]{1,62}){0,63}""";
    private const string EMAILADDRESS = $$"""{{EMAILLOCALPART}}@%{{HOSTNAME}}""";

    private const string INT = """(?:[+-]?(?:[0-9]+))""";
    private const string BASE10NUM = """(?<![0-9.+-])(?>[+-]?(?:(?:[0-9]+(?:\.[0-9]+)?)|(?:\.[0-9]+)))""";
    private const string NUMBER = $$"""(?:{{BASE10NUM}})""";
    private const string BASE16NUM = """(?<![0-9A-Fa-f])(?:[+-]?(?:0x)?(?:[0-9A-Fa-f]+))""";
    private const string BASE16FLOAT = """\b(?<![0-9A-Fa-f.])(?:[+-]?(?:0x)?(?:(?:[0-9A-Fa-f]+(?:\.[0-9A-Fa-f]*)?)|(?:\.[0-9A-Fa-f]+)))\b""";

    private const string POSINT = """\b(?:[1-9][0-9]*)\b""";
    private const string NONNEGINT = """\b(?:[0-9]+)\b""";
    private const string WORD = """\b\w+\b""";
    private const string NOTSPACE = """\S+""";
    private const string SPACE = """\s*""";
    private const string DATA = """.*?""";
    private const string GREEDYDATA = """.*""";

    private const string QUOTEDSTRING = """(?>(?<!\\)(?>"(?>\\.|[^\\"]+)+"|"|(?>'(?>\\.|[^\\']+)+')|''|(?>`(?>\\.|[^\\`]+)+`)|``))""";
    private const string UUID = """[A-Fa-f0-9]{8}-(?:[A-Fa-f0-9]{4}-){3}[A-Fa-f0-9]{12}""";

    private const string URN = """urn:[0-9A-Za-z][0-9A-Za-z-]{0,31}:(?:%[0-9a-fA-F]{2}|[0-9A-Za-z()+,.:=@;$\_!\*'/?#-])+""";

    // Networking
    private const string MAC = $$"""(?:{{CISCOMAC}}|{{WINDOWSMAC}}|{{COMMONMAC}})""";
    private const string CISCOMAC = """(?:(?:[A-Fa-f0-9]{4}\.){2}[A-Fa-f0-9]{4})""";
    private const string WINDOWSMAC = """(?:(?:[A-Fa-f0-9]{2}-){5}[A-Fa-f0-9]{2})""";
    private const string COMMONMAC = """(?:(?:[A-Fa-f0-9]{2}:){5}[A-Fa-f0-9]{2})""";

    private const string IPV6 = """((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2-4\d|1\d\d|[1-9]?\d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:)))(%.+)?""";

    private const string IPV4 = """(?<![0-9])(?:(?:[0-1]?[0-9]{1,2}|2[0-4][0-9]|25[0-5])[.](?:[0-1]?[0-9]{1,2}|2[0-4][0-9]|25[0-5])[.](?:[0-1]?[0-9]{1,2}|2[0-4][0-9]|25[0-5])[.](?:[0-1]?[0-9]{1,2}|2[0-4][0-9]|25[0-5]))(?![0-9])""";

    private const string IP = $$"""(?:{{IPV6}}|%{{IPV4}})""";
    private const string HOSTNAME = """\b(?:[0-9A-Za-z][0-9A-Za-z-]{0,62})(?:\.(?:[0-9A-Za-z][0-9A-Za-z-]{0,62}))*(\.?|\b)""";
    private const string IPORHOST = $$"""(?:{{IP}}|{{HOSTNAME}})""";
    private const string HOSTPORT = $$"""{{IPORHOST}}:%{{POSINT}}""";

    // Paths
    private const string PATH = $$"""(?:{{UNIXPATH}}|{{WINPATH}})""";
    private const string UNIXPATH = """(/[[[:alnum:]]\_%!$@:.,+\~-]*)+""";
    private const string TTY = """(?:/dev/(pts|tty([pq])?)(\w+)?/?(?:[0-9]+))""";
    private const string WINPATH = """(?>[A-Za-z]+:|\\)(?:\\[^\\?*]*)+""";

    // URIs
    private const string URIPROTO = """[A-Za-z]([A-Za-z0-9+\-.]+)+""";
    private const string URIHOST = $$"""{{IPORHOST}}(?::{{POSINT}})?""";
    private const string URIPATH = """(?:/[A-Za-z0-9$.+!*'(){},~:;=@#%&_\-]*)+""";
    private const string URIQUERY = """[A-Za-z0-9$.+!*'|(){},~@#%&/=:;_?\-\[\]<>]*""";
    private const string URIPARAM = $$"""\?{{URIQUERY}}""";
    private const string URIPATHPARAM = $$"""{{URIPATH}}(?:\?{{URIQUERY}})?""";
    private const string URI = $$"""{{URIPROTO}}://(?:{{USERNAME}}(?::[^@]*)?@)?(?:{{URIHOST}})?(?:{{URIPATH}}(?:\?{{URIQUERY}})?)?""";

    // Dates and times
    private const string MONTH = """\b(?:[Jj]an(?:uary|uar)?|[Ff]eb(?:ruary|ruar)?|[Mm](?:a|ä)?r(?:ch|z)?|[Aa]pr(?:il)?|[Mm]a(?:y|i)?|[Jj]un(?:e|i)?|[Jj]ul(?:y|i)?|[Aa]ug(?:ust)?|[Ss]ep(?:tember)?|[Oo](?:c|k)?t(?:ober)?|[Nn]ov(?:ember)?|[Dd]e(?:c|z)(?:ember)?)\b""";
    private const string MONTHNUM = """(?:0?[1-9]|1[0-2])""";
    private const string MONTHNUM2 = """(?:0[1-9]|1[0-2])""";
    private const string MONTHDAY = """(?:(?:0[1-9])|(?:[12][0-9])|(?:3[01])|[1-9])""";

    private const string DAY = """(?:Mon(?:day)?|Tue(?:sday)?|Wed(?:nesday)?|Thu(?:rsday)?|Fri(?:day)?|Sat(?:urday)?|Sun(?:day)?)""";

    private const string YEAR = """(?>\d\d){1,2}""";
    private const string HOUR = """(?:2[0123]|[01]?[0-9])""";
    private const string MINUTE = """(?:[0-5][0-9])""";
    private const string SECOND = """(?:(?:[0-5]?[0-9]|60)(?:[:.,][0-9]+)?)""";
    private const string TIME = $$"""(?!<[0-9]){{HOUR}}:{{MINUTE}}(?::{{SECOND}})(?![0-9])""";

    private const string DATE_US = $$"""{{MONTHNUM}}[/-]{{MONTHDAY}}[/-]{{YEAR}}""";
    private const string DATE_EU = $$"""{{MONTHDAY}}[./-]{{MONTHNUM}}[./-]{{YEAR}}""";
    private const string ISO8601_TIMEZONE = $$"""(?:Z|[+-]{{HOUR}}(?::?{{MINUTE}}))""";

    private const string TIMESTAMP_ISO8601 = $$"""{{YEAR}}-{{MONTHNUM}}-{{MONTHDAY}}[T ]{{HOUR}}:?{{MINUTE}}(?::?{{SECOND}})?{{ISO8601_TIMEZONE}}?""";
    private const string DATE = $$"""{{DATE_US}}|{{DATE_EU}}""";
    private const string DATESTAMP = $$"""{{DATE}}[- ]{{TIME}}""";
    private const string TZ = """(?:[APMCE][SD]T|UTC)""";

    private const string DATESTAMP_RFC822 = $$"""{{DAY}} {{MONTH}} {{MONTHDAY}} {{YEAR}} {{TIME}} {{TZ}}""";
    private const string DATESTAMP_RFC2822 = $$"""{{DAY}}, {{MONTHDAY}} {{MONTH}} {{YEAR}} {{TIME}} {{ISO8601_TIMEZONE}}""";
    private const string DATESTAMP_OTHER = $$"""{{DAY}} {{MONTH}} {{MONTHDAY}} {{TIME}} {{TZ}} {{YEAR}}""";
    private const string DATESTAMP_EVENTLOG = $$"""{{YEAR}}{{MONTHNUM2}}{{MONTHDAY}}{{HOUR}}{{MINUTE}}{{SECOND}}""";

    // Syslog
    private const string SYSLOGTIMESTAMP = $$"""{{MONTH}} +{{MONTHDAY}} {{TIME}}""";
    private const string PROG = """[\x21-\x5a\x5c\x5e-\x7e]+""";
    private const string SYSLOGPROG = $$"""(?<process_name>{{PROG}})(?:\[(?<process_pid>{{POSINT}})\])?""";
    private const string SYSLOGHOST = GrokNamedPatterns.IPORHOST;
    private const string SYSLOGFACILITY = $$"""<(?<syslog_facility_code>{{NONNEGINT}}).(?<syslog_priority>{{NONNEGINT}})>""";
    private const string HTTPDATE = $$"""{{MONTHDAY}}/{{MONTH}}/{{YEAR}}:{{TIME}} {{INT}}""";

    // Log formats
    private const string SYSLOGBASE = $$"""(?<timestamp>{{SYSLOGTIMESTAMP}}) (?:{{SYSLOGFACILITY}} )?(?<host>{{SYSLOGHOST}}) {{SYSLOGPROG}}:""";

    // Log levels
    private const string LOGLEVEL = """([Aa]lert|ALERT|[Tt]race|TRACE|[Dd]ebug|DEBUG|[Nn]otice|NOTICE|[Ii]nfo?(?:rmation)?|INFO?(?:RMATION)?|[Ww]arn?(?:ing)?|WARN?(?:ING)?|[Ee]rr?(?:or)?|ERR?(?:OR)?|[Cc]rit?(?:ical)?|CRIT?(?:ICAL)?|[Ff]atal|FATAL|[Ss]evere|SEVERE|EMERG(?:ENCY)?|[Ee]merg(?:ency)?)""";

    private static readonly GrokNamedPattern Username = new(GrokNamedPatterns.USERNAME, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern EmailLocalPart = new(GrokNamedPatterns.EMAILLOCALPART, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern EmailAddress = new(GrokNamedPatterns.EMAILADDRESS, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Int = new(GrokNamedPatterns.INT, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Base10Num = new(GrokNamedPatterns.BASE10NUM, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Number = new(GrokNamedPatterns.NUMBER, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Base16Num = new(GrokNamedPatterns.BASE16NUM, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Base16Float = new(GrokNamedPatterns.BASE16FLOAT, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern PosInt = new(GrokNamedPatterns.POSINT, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern NonNegInt = new(GrokNamedPatterns.NONNEGINT, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Word = new(GrokNamedPatterns.WORD, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern NotSpace = new(GrokNamedPatterns.NOTSPACE, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Space = new(GrokNamedPatterns.SPACE, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Data = new(GrokNamedPatterns.DATA, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern GreedyData = new(GrokNamedPatterns.GREEDYDATA, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern QuotedString = new(GrokNamedPatterns.QUOTEDSTRING, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Uuid = new(GrokNamedPatterns.UUID, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Urn = new(GrokNamedPatterns.URN, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Mac = new(GrokNamedPatterns.MAC, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern CiscoMac = new(GrokNamedPatterns.CISCOMAC, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern WindowsMac = new(GrokNamedPatterns.WINDOWSMAC, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern CommonMac = new(GrokNamedPatterns.COMMONMAC, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Ipv6 = new(GrokNamedPatterns.IPV6, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Ipv4 = new(GrokNamedPatterns.IPV4, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Ip = new(GrokNamedPatterns.IP, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Hostname = new(GrokNamedPatterns.HOSTNAME, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern IpOrHost = new(GrokNamedPatterns.IPORHOST, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern HostPort = new(GrokNamedPatterns.HOSTPORT, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Path = new(GrokNamedPatterns.PATH, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern UnixPath = new(GrokNamedPatterns.UNIXPATH, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Tty = new(GrokNamedPatterns.TTY, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern WinPath = new(GrokNamedPatterns.WINPATH, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern UriProto = new(GrokNamedPatterns.URIPROTO, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern UriHost = new(GrokNamedPatterns.URIHOST, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern UriPath = new(GrokNamedPatterns.URIPATH, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern UriQuery = new(GrokNamedPatterns.URIQUERY, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern UriParam = new(GrokNamedPatterns.URIPARAM, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern UriPathParam = new(GrokNamedPatterns.URIPATHPARAM, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Uri = new(GrokNamedPatterns.URI, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Month = new(GrokNamedPatterns.MONTH, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern MonthNum = new(GrokNamedPatterns.MONTHNUM, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern MonthNum2 = new(GrokNamedPatterns.MONTHNUM2, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern MonthDay = new(GrokNamedPatterns.MONTHDAY, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Day = new(GrokNamedPatterns.DAY, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Year = new(GrokNamedPatterns.YEAR, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Hour = new(GrokNamedPatterns.HOUR, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Minute = new(GrokNamedPatterns.MINUTE, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Second = new(GrokNamedPatterns.SECOND, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Time = new(GrokNamedPatterns.TIME, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern DateUs = new(GrokNamedPatterns.DATE_US, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern DateEu = new(GrokNamedPatterns.DATE_EU, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Iso8601TimeZone = new(GrokNamedPatterns.ISO8601_TIMEZONE, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern TimestampIso8601 = new(GrokNamedPatterns.TIMESTAMP_ISO8601, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Date = new(GrokNamedPatterns.DATE, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern DateStamp = new(GrokNamedPatterns.DATESTAMP, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Tz = new(GrokNamedPatterns.TZ, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern DateStampRfc822 = new(GrokNamedPatterns.DATESTAMP_RFC822, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern DateStampRfc2822 = new(GrokNamedPatterns.DATESTAMP_RFC2822, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern DateStampOther = new(GrokNamedPatterns.DATESTAMP_OTHER, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern DateStampEventLog = new(GrokNamedPatterns.DATESTAMP_EVENTLOG, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern SyslogTimestamp = new(GrokNamedPatterns.SYSLOGTIMESTAMP, ImmutableArray<(string, GrokType)>.Empty);
    private static readonly GrokNamedPattern Prog = new(GrokNamedPatterns.PROG, ImmutableArray<(string, GrokType)>.Empty);

    private static readonly GrokNamedPattern SyslogProg = new(
        GrokNamedPatterns.SYSLOGPROG,
        ImmutableArray.Create(("process_pid", GrokType.Int))
    );

    private static readonly GrokNamedPattern SyslogHost = new(GrokNamedPatterns.SYSLOGHOST, ImmutableArray<(string, GrokType)>.Empty);

    private static readonly GrokNamedPattern SyslogFacility = new(
        GrokNamedPatterns.SYSLOGFACILITY,
        ImmutableArray.Create(("syslog_facility_code", GrokType.Int), ("syslog_priority", GrokType.Int))
    );

    private static readonly GrokNamedPattern HttpDate = new(GrokNamedPatterns.HTTPDATE, ImmutableArray<(string, GrokType)>.Empty);

    private static readonly GrokNamedPattern SyslogBase = new(
        GrokNamedPatterns.SYSLOGBASE,
        ImmutableArray.Create(("syslog_facility_code", GrokType.Int), ("syslog_priority", GrokType.Int), ("process_pid", GrokType.Int))
    );

    private static readonly GrokNamedPattern LogLevel = new(GrokNamedPatterns.LOGLEVEL, ImmutableArray<(string, GrokType)>.Empty);

    public static GrokNamedPattern Get(ReadOnlySpan<char> name) => name switch
    {
        "USERNAME" or "USER" => GrokNamedPatterns.Username,
        "EMAILLOCALPART" => GrokNamedPatterns.EmailLocalPart,
        "EMAILADDRESS" => GrokNamedPatterns.EmailAddress,
        "INT" => GrokNamedPatterns.Int,
        "BASE10NUM" => GrokNamedPatterns.Base10Num,
        "NUMBER" => GrokNamedPatterns.Number,
        "BASE16NUM" => GrokNamedPatterns.Base16Num,
        "BASE16FLOAT" => GrokNamedPatterns.Base16Float,
        "POSINT" => GrokNamedPatterns.PosInt,
        "NONNEGINT" => GrokNamedPatterns.NonNegInt,
        "WORD" => GrokNamedPatterns.Word,
        "NOTSPACE" => GrokNamedPatterns.NotSpace,
        "SPACE" => GrokNamedPatterns.Space,
        "DATA" => GrokNamedPatterns.Data,
        "GREEDYDATA" => GrokNamedPatterns.GreedyData,
        "QUOTEDSTRING" or "QS" => GrokNamedPatterns.QuotedString,
        "UUID" => GrokNamedPatterns.Uuid,
        "URN" => GrokNamedPatterns.Urn,
        "MAC" => GrokNamedPatterns.Mac,
        "CISCOMAC" => GrokNamedPatterns.CiscoMac,
        "WINDOWSMAC" => GrokNamedPatterns.WindowsMac,
        "COMMONMAC" => GrokNamedPatterns.CommonMac,
        "IPV6" => GrokNamedPatterns.Ipv6,
        "IPV4" => GrokNamedPatterns.Ipv4,
        "IP" => GrokNamedPatterns.Ip,
        "HOSTNAME" => GrokNamedPatterns.Hostname,
        "IPORHOST" => GrokNamedPatterns.IpOrHost,
        "HOSTPORT" => GrokNamedPatterns.HostPort,
        "PATH" => GrokNamedPatterns.Path,
        "UNIXPATH" => GrokNamedPatterns.UnixPath,
        "TTY" => GrokNamedPatterns.Tty,
        "WINPATH" => GrokNamedPatterns.WinPath,
        "URIPROTO" => GrokNamedPatterns.UriProto,
        "URIHOST" => GrokNamedPatterns.UriHost,
        "URIPATH" => GrokNamedPatterns.UriPath,
        "URIQUERY" => GrokNamedPatterns.UriQuery,
        "URIPARAM" => GrokNamedPatterns.UriParam,
        "URIPATHPARAM" => GrokNamedPatterns.UriPathParam,
        "URI" => GrokNamedPatterns.Uri,
        "MONTH" => GrokNamedPatterns.Month,
        "MONTHNUM" => GrokNamedPatterns.MonthNum,
        "MONTHNUM2" => GrokNamedPatterns.MonthNum2,
        "MONTHDAY" => GrokNamedPatterns.MonthDay,
        "DAY" => GrokNamedPatterns.Day,
        "YEAR" => GrokNamedPatterns.Year,
        "HOUR" => GrokNamedPatterns.Hour,
        "MINUTE" => GrokNamedPatterns.Minute,
        "SECOND" => GrokNamedPatterns.Second,
        "TIME" => GrokNamedPatterns.Time,
        "DATE_US" => GrokNamedPatterns.DateUs,
        "DATE_EU" => GrokNamedPatterns.DateEu,
        "ISO8601_TIMEZONE" => GrokNamedPatterns.Iso8601TimeZone,
        "ISO8601_SECOND" => GrokNamedPatterns.Second,
        "TIMESTAMP_ISO8601" => GrokNamedPatterns.TimestampIso8601,
        "DATE" => GrokNamedPatterns.Date,
        "DATESTAMP" => GrokNamedPatterns.DateStamp,
        "TZ" => GrokNamedPatterns.Tz,
        "DATESTAMP_RFC822" => GrokNamedPatterns.DateStampRfc822,
        "DATESTAMP_RFC2822" => GrokNamedPatterns.DateStampRfc2822,
        "DATESTAMP_OTHER" => GrokNamedPatterns.DateStampOther,
        "DATESTAMP_EVENTLOG" => GrokNamedPatterns.DateStampEventLog,
        "SYSLOGTIMESTAMP" => GrokNamedPatterns.SyslogTimestamp,
        "PROG" => GrokNamedPatterns.Prog,
        "SYSLOGPROG" => GrokNamedPatterns.SyslogProg,
        "SYSLOGHOST" => GrokNamedPatterns.SyslogHost,
        "SYSLOGFACILITY" => GrokNamedPatterns.SyslogFacility,
        "HTTPDATE" => GrokNamedPatterns.HttpDate,
        "SYSLOGBASE" => GrokNamedPatterns.SyslogBase,
        "LOGLEVEL" => GrokNamedPatterns.LogLevel,
        _ => throw new GrokException($"unknown grok pattern: '{name}'"),
    };
}
