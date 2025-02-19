using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IDistributedCache _redisCache;
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly DistributedCacheEntryOptions cacheOptions;
    ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
    private readonly IDatabase db;

    public IndexModel(
        ILogger<IndexModel> logger,
        IDistributedCache redisCache,
        IConnectionMultiplexer redisConnection)
    {
        IDatabase db = redis.GetDatabase();
        _logger = logger;
        _redisCache = redisCache;
        _redisConnection = redisConnection;
    }

    public async Task<IActionResult> OnPostAsync(string text)
    {
        string id = Guid.NewGuid().ToString();

        // Сохранение текста
        await _redisCache.SetStringAsync($"TEXT-EXAMPLE", "EXAMPLE");
        _logger.LogWarning($"TEXT-{id}");
        // Расчет Rank
        double rank = CalculateRank(text);
        _redisCache.SetString($"RANK-{id}", rank.ToString());

        double similarity = await CalculateSimilarityAsync(text);
        _redisCache.SetString($"SIMILARITY-{id}", similarity.ToString());

        return Redirect($"summary?id={id}");
    }

    private async Task<double> CalculateSimilarityAsync(string currentText)
    {
        var db = _redisConnection.GetDatabase();
        var server = _redisConnection.GetServer("localhost:6379");

        var keys = server.Keys(pattern: "TEXT-*");
        foreach (var key in keys)
        {
            _logger.LogWarning($"{key}");
            try
            {
                var stringValue = await _redisCache.GetStringAsync(key.ToString());
                if (stringValue != null)
                {
                    _logger.LogWarning($"{stringValue}");
                    if (currentText == stringValue) return 1.0;
                }
            } catch(Exception ex)
            {
                continue;
            }
          
        }
        return 0.0;
    }

    private double CalculateRank(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        int nonAlphabeticCount = 0;
        foreach (char c in text)
        {
            if (!IsAlphabetic(c))
                nonAlphabeticCount++;
        }
        return (double)nonAlphabeticCount / text.Length;
    }

    private bool IsAlphabetic(char c)
    {
        // Проверка на русские и латинские буквы
        return char.IsLetter(c) &&
               (c <= 0x007F ||  // ASCII
                c >= 0x0410 && c <= 0x044F); // Русские буквы
    }

}