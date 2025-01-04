using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using UrlShortener.Context;

namespace UrlShortener.Services;

/// <summary>
/// Service for generating and managing shortened URLs.
/// </summary>
public class UrlShorteningService
{
    public const int NumberOfCharsInShortLink = 7;
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    private readonly ApplicationDBContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlShorteningService"/> class.
    /// </summary>
    /// <param name="dbContext">The database context to use.</param>
    public UrlShorteningService(ApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Generates a unique code for the shortened URL.
    /// </summary>
    /// <returns>A unique code as a string.</returns>
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
}
