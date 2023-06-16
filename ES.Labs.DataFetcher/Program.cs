using System.Collections.Concurrent;
using ES.Labs.Domain.Projections;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ES.Labs.DataFetcher;

public class Program
{
    private static readonly ConcurrentDictionary<string, EqualizerState> EqStates = new();

    public static void Main(string[] args)
    {
        MainAsync(args).GetAwaiter().GetResult();
        Console.ReadKey();

        Console.WriteLine("Cleaning up...");
    }

    public static async Task MainAsync(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        var apiKey = config["X-RapidAPI-Key"];

        Console.WriteLine($"Hello {typeof(Program).Namespace}!");

        var games = await FetchGames(16, apiKey);

        foreach (var game in games)
        {
            await FetchPlayByPlay(game.id, apiKey);
        }
    }

    private static async Task FetchPlayByPlay(int gameId, string apiKey)
    {
        var games = new List<Datum>();
        int? nextPage = 0;

        var client = new HttpClient();

        while (nextPage.HasValue)
        {
            var request = BuildStatsRequest(gameId, page: nextPage.GetValueOrDefault(), apiKey: apiKey);

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var respo = JsonConvert.DeserializeObject<GameStats>(body);

                nextPage = respo.meta.next_page;
                games.AddRange(respo.data);
            }
        }
    }

    private static async Task<IEnumerable<Game>> FetchGames(int teamId, string apiKey)
    {
        var games = await GetGames(teamId, apiKey);
        Console.WriteLine(games);

        await File.WriteAllTextAsync("team-16-games.json", JsonConvert.SerializeObject(games, Formatting.Indented));

        return games;
    }

    private static async Task<IEnumerable<Game>> GetGames(int teamId, string apiKey)
    {
        var games = new List<Game>();
        int? nextPage = 0;

        var client = new HttpClient();

        while (nextPage.HasValue)
        {
            var request = BuildGamesRequest(teamId: teamId, page: nextPage.GetValueOrDefault(), apiKey: apiKey);

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var respo = JsonConvert.DeserializeObject<Root>(body);

                nextPage = respo.meta.next_page;
                games.AddRange(respo.data);
            }
        }

        return games;
    }

    private static HttpRequestMessage BuildStatsRequest(int gameId, int page, string apiKey)
    {
        return new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://free-nba.p.rapidapi.com/stats?page={page}&per_page=50&seasons[]=2018&game_ids[]={gameId}"),
            Headers =
            {
                { "X-RapidAPI-Key", apiKey },
                { "X-RapidAPI-Host", "free-nba.p.rapidapi.com" },
            }
        };
    }

    private static HttpRequestMessage BuildGamesRequest(int teamId, int page, string apiKey)
    {
        return new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://free-nba.p.rapidapi.com/games?page={page}&per_page=50&seasons[]=2018&team_ids[]={teamId}"),
            Headers =
            {
                { "X-RapidAPI-Key", apiKey },
                { "X-RapidAPI-Host", "free-nba.p.rapidapi.com" },
            }
        };
    }
}
