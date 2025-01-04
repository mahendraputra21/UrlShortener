using Microsoft.EntityFrameworkCore;
using UrlShortener.Context;

namespace UrlShortener.Services;

public class UrlShorteningService
{
    public const int NumberOfCharsInShortLink = 7;
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    private readonly Random _random = new();

    private readonly ApplicationDBContext _dbContext;

    public UrlShorteningService(ApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GenerateUniqueCode()
    {
        var codeChars = new char[NumberOfCharsInShortLink];

        while (true) 
        {
            for (int i = 0; i < NumberOfCharsInShortLink; i++)
            {
                var randomIndex = _random.Next(Alphabet.Length - 1);

                codeChars[i] = Alphabet[randomIndex];
            }

            var code = new string(codeChars);

            if (!await _dbContext.ShortenUrls.AnyAsync(s => s.Code == code))
            {
                return code;
            }
        }
    }
}
