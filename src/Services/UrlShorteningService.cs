using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography;
using UrlShortener.Context;

namespace UrlShortener.Services;
public class UrlShorteningService
{
    public const int NumberOfCharsInShortLink = 7;
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    private readonly ApplicationDBContext _dbContext;
    private readonly IDistributedCache _cache;

    public UrlShorteningService(ApplicationDBContext dbContext, IDistributedCache cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task<string> GenerateUniqueCode()
    {
        var codeChars = new char[NumberOfCharsInShortLink];
        var alphabetLength = Alphabet.Length;

        while (true)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var randomBytes = new byte[NumberOfCharsInShortLink];
                rng.GetBytes(randomBytes);

                for (int i = 0; i < NumberOfCharsInShortLink; i++)
                {
                    var randomIndex = randomBytes[i] % alphabetLength;
                    codeChars[i] = Alphabet[randomIndex];
                }
            }

            var code = new string(codeChars);

            if (!await _dbContext.ShortenUrls.AnyAsync(s => s.Code == code))
            {
                return code;
            }
        }
    }

    public async Task<string?> GetLongUrl(string code)
    {
        var cachedLongUrl = await _cache.GetStringAsync(code);

        if (cachedLongUrl != null) return cachedLongUrl;

        var shortenUrl = await _dbContext.ShortenUrls
                                         .SingleOrDefaultAsync(s => s.Code == code);
        
        if (shortenUrl == null) return null;

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        await _cache.SetStringAsync(code, shortenUrl.LongUrl, options);

        return shortenUrl.LongUrl;
    }
}
