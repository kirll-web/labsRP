using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Valuator.Pages;
public class SummaryModel : PageModel
{
    private readonly IDistributedCache _redis;
    private readonly ILogger<SummaryModel> _logger;

    public SummaryModel(ILogger<SummaryModel> logger, IDistributedCache redis)
    {
        _logger = logger;
        _redis = redis;
    }

    public double Rank { get; set; }
    public double Similarity { get; set; }

    public async Task OnGetAsync(string id)
    {
        string rankKey = "RANK-" + id;
        string similarityKey = "SIMILARITY-" + id;

        string rankValue = await _redis.GetStringAsync(rankKey);
        string similarityValue = await _redis.GetStringAsync(similarityKey);

        Rank = double.Parse(rankValue);
        Similarity = double.Parse(similarityValue);
    }
}
