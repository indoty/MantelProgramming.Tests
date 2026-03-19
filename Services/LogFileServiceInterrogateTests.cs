

using MantelProgrammingTest.Services;

namespace MantelProgrammingTest.Tests.Services;

public class LogFileServiceInterrogateTests
{
    private readonly LogFileService _sut = new();

    private static string MakeLine(string ip, string url) =>
        $"""{ip} - - [10/Jul/2018:22:21:28 +0200] "GET {url} HTTP/1.1" 200 3574 "-" "Mozilla/5.0" """;

    [Fact]
    public void InterrogateLogFile_WithNullContent_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.InterrogateLogFile(null!));
    }

    [Fact]
    public void InterrogateLogFile_WithEmptyContent_ReturnsZeroCounts()
    {
        var result = _sut.InterrogateLogFile(string.Empty);

        Assert.Equal(0, result.UniqueIpAddressCount);
        Assert.Empty(result.TopVisitedUrls);
        Assert.Empty(result.TopActiveIpAddresses);
    }

    [Fact]
    public void InterrogateLogFile_CountsUniqueIpAddresses()
    {
        var content = string.Join('\n',
            MakeLine("1.1.1.1", "/"),
            MakeLine("2.2.2.2", "/"),
            MakeLine("1.1.1.1", "/about"));

        var result = _sut.InterrogateLogFile(content);

        Assert.Equal(2, result.UniqueIpAddressCount);
    }

    [Fact]
    public void InterrogateLogFile_ReturnsTop3MostVisitedUrls()
    {
        var content = string.Join('\n',
            MakeLine("1.1.1.1", "/page-a"),
            MakeLine("1.1.1.1", "/page-a"),
            MakeLine("1.1.1.1", "/page-a"),
            MakeLine("2.2.2.2", "/page-b"),
            MakeLine("2.2.2.2", "/page-b"),
            MakeLine("3.3.3.3", "/page-c"),
            MakeLine("4.4.4.4", "/page-d"));

        var result = _sut.InterrogateLogFile(content);

        Assert.Equal(3, result.TopVisitedUrls.Count);
        Assert.Equal("/page-a", result.TopVisitedUrls[0].Url);
        Assert.Equal(3, result.TopVisitedUrls[0].VisitCount);
        Assert.Equal("/page-b", result.TopVisitedUrls[1].Url);
        Assert.Equal(2, result.TopVisitedUrls[1].VisitCount);
    }

    [Fact]
    public void InterrogateLogFile_ReturnsTop3MostActiveIps()
    {
        var content = string.Join('\n',
            MakeLine("10.0.0.1", "/"),
            MakeLine("10.0.0.1", "/"),
            MakeLine("10.0.0.1", "/"),
            MakeLine("10.0.0.2", "/"),
            MakeLine("10.0.0.2", "/"),
            MakeLine("10.0.0.3", "/"),
            MakeLine("10.0.0.4", "/"));

        var result = _sut.InterrogateLogFile(content);

        Assert.Equal(3, result.TopActiveIpAddresses.Count);
        Assert.Equal("10.0.0.1", result.TopActiveIpAddresses[0].IpAddress);
        Assert.Equal(3, result.TopActiveIpAddresses[0].RequestCount);
        Assert.Equal("10.0.0.2", result.TopActiveIpAddresses[1].IpAddress);
        Assert.Equal(2, result.TopActiveIpAddresses[1].RequestCount);
    }

    [Fact]
    public void InterrogateLogFile_LimitsToTop3WhenMoreExist()
    {
        var content = string.Join('\n',
            MakeLine("1.1.1.1", "/a"),
            MakeLine("2.2.2.2", "/b"),
            MakeLine("3.3.3.3", "/c"),
            MakeLine("4.4.4.4", "/d"),
            MakeLine("5.5.5.5", "/e"));

        var result = _sut.InterrogateLogFile(content);

        Assert.Equal(5, result.UniqueIpAddressCount);
        Assert.Equal(3, result.TopVisitedUrls.Count);
        Assert.Equal(3, result.TopActiveIpAddresses.Count);
    }

    [Fact]
    public void InterrogateLogFile_SkipsInvalidLines()
    {
        var content = string.Join('\n',
            MakeLine("1.1.1.1", "/valid"),
            "this is not a valid log line",
            MakeLine("2.2.2.2", "/also-valid"));

        var result = _sut.InterrogateLogFile(content);

        Assert.Equal(2, result.UniqueIpAddressCount);
    }
}