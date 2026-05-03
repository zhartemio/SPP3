using Xunit;
using FileAnalyzer.Core;


namespace FileAnalyzer.Tests
{
    public class ScannerTests
    {
        [Fact]
        public async Task Scan_ShouldReturnResult()
        {
            var scanner = new DirectoryScanner(2, CancellationToken.None);

            var result = await scanner.ScanAsync("C:\\");

            Assert.NotNull(result);
            Assert.True(result.Size >= 0);
        }
    }
}