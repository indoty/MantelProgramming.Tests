using MantelProgrammingTest.Services;
using Xunit;

namespace MantelProgrammingTest.Tests.Services;

public class LogFileServiceValidateTests
{
    private readonly LogFileService _sut = new();

    private const string ValidLine =
        """177.71.128.21 - - [10/Jul/2018:22:21:28 +0200] "GET /intranet-analytics/ HTTP/1.1" 200 3574 "-" "Mozilla/5.0" """;


    [Fact]
    public void Validate_WithNullContent_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.Validate(null!));
    }

    [Fact]
    public void Validate_WithEmptyContent_ReturnsFailure()
    {
        var result = _sut.Validate(string.Empty);

        Assert.False(result.IsValid);
        Assert.Equal("The file is empty.", result.ErrorMessage);
    }

    [Fact]
    public void Validate_WithSingleValidLine_ReturnsSuccess()
    {
        var result = _sut.Validate(ValidLine);

        Assert.True(result.IsValid);
        Assert.Equal(1, result.LineCount);
    }

    [Fact]
    public void Validate_WithMultipleValidLines_ReturnsCorrectLineCount()
    {
        var content = string.Join('\n', ValidLine, ValidLine, ValidLine);

        var result = _sut.Validate(content);

        Assert.True(result.IsValid);
        Assert.Equal(3, result.LineCount);
    }

    [Fact]
    public void Validate_WithInvalidLine_ReturnsFailureWithLineNumber()
    {
        var content = $"{ValidLine}\nthis is not a valid log line";

        var result = _sut.Validate(content);

        Assert.False(result.IsValid);
        Assert.Single(result.InvalidLines);
        Assert.Equal(2, result.InvalidLines[0].LineNumber);
    }

    [Fact]
    public void Validate_WithMixedLines_ReportsAllInvalidLines()
    {
        var content = $"bad line 1\n{ValidLine}\nbad line 3";

        var result = _sut.Validate(content);

        Assert.False(result.IsValid);
        Assert.Equal(2, result.InvalidLines.Count);
    }

    [Theory]
    [InlineData("""50.112.00.11 - admin [11/Jul/2018:17:33:01 +0200] "GET /asset.css HTTP/1.1" 200 3574 "-" "Mozilla/5.0" """)]
    [InlineData("""72.44.32.10 - - [09/Jul/2018:15:48:20 +0200] "GET / HTTP/1.1" 200 3574 "-" "some-agent" """)]
    [InlineData("""168.41.191.40 - - [09/Jul/2018:10:11:30 +0200] "GET http://example.net/blog/category/meta/ HTTP/1.1" 200 3574 "-" "Safari" """)]
    public void Validate_WithVariousValidFormats_ReturnsSuccess(string line)
    {
        var result = _sut.Validate(line);

        Assert.True(result.IsValid);
    }
}