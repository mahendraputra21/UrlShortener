using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using UrlShortener.Context;

namespace UrlShortener.Services;
public class UrlShorteningService
{
    public const int NumberOfCharsInShortLink = 7;
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    private readonly ApplicationDBContext _dbContext;
    private readonly IMemoryCache _cache;

    public UrlShorteningService(ApplicationDBContext dbContext, IMemoryCache cache)
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
        if (_cache.TryGetValue(code, out string? longUrl))
            return longUrl;
        
        var shortenUrl = await _dbContext.ShortenUrls.SingleOrDefaultAsync(s => s.Code == code);
        if (shortenUrl == null)
            return null;
        
        _cache.Set(code, shortenUrl.LongUrl, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });

        return shortenUrl.LongUrl;
    }
}
