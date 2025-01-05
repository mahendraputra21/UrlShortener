using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using UrlShortener.Context;
using UrlShortener.Entities;
using UrlShortener.Services;

namespace UrlShortener.Test.Services;

public class UrlShorteningServiceTests
{
    private readonly Mock<ApplicationDBContext> _dbContextMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly UrlShorteningService _service;

    public UrlShorteningServiceTests()
    {
        _dbContextMock = new Mock<ApplicationDBContext>(new DbContextOptions<ApplicationDBContext>());
        _cacheMock = new Mock<IDistributedCache>();
        _service = new UrlShorteningService(_dbContextMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task GetLongUrl_ReturnsUrlFromCache_WhenExists()
    {
        // Arrange
        var code = "abc123";
        var longUrl = "https://example.com";
        _cacheMock.Setup(c => c.GetStringAsync(code, It.IsAny<CancellationToken>())).ReturnsAsync(longUrl);

        // Act
        var result = await _service.GetLongUrlAsync(code);

        // Assert
        Assert.Equal(longUrl, result);
        _cacheMock.Verify(c => c.GetStringAsync(code, It.IsAny<CancellationToken>()), Times.Once);
        _dbContextMock.Verify(db => db.ShortenUrls.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ShortenUrl, bool>>>(), It.IsAny<CancellationToken>()), Times.Never);

    }

    [Fact]
    public async Task GetLongUrl_ReturnsUrlFromDatabase_WhenNotInCache()
    {
        // Arrange
        var code = "abc123";
        var longUrl = "https://example.com";
        var shortenUrl = new ShortenUrl { Code = code, LongUrl = longUrl };
        _cacheMock.Setup(c => c.GetStringAsync(code, It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);
        _dbContextMock.Setup(db => db.ShortenUrls.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ShortenUrl, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(shortenUrl);

        // Act
        var result = await _service.GetLongUrlAsync(code);

        // Assert
        Assert.Equal(longUrl, result);
        _cacheMock.Verify(c => c.GetStringAsync(code, It.IsAny<CancellationToken>()), Times.Once);
        _dbContextMock.Verify(db => db.ShortenUrls.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ShortenUrl, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.SetStringAsync(code, longUrl, It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetLongUrl_ReturnsNull_WhenNotInCacheOrDatabase()
    {
        // Arrange
        var code = "abc123";
        _cacheMock.Setup(c => c.GetStringAsync(code, It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);
        _dbContextMock.Setup(db => db.ShortenUrls.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ShortenUrl, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync((ShortenUrl?)null);

        // Act
        var result = await _service.GetLongUrlAsync(code);

        // Assert
        Assert.Null(result);
        _cacheMock.Verify(c => c.GetStringAsync(code, It.IsAny<CancellationToken>()), Times.Once);
        _dbContextMock.Verify(db => db.ShortenUrls.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ShortenUrl, bool>>>(), It.Is<CancellationToken>(ct => ct == default)), Times.Once);
        _cacheMock.Verify(c => c.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}


