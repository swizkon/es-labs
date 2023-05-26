using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using ES.Labs.Domain;
using ES.Labs.Domain.Projections;
using EventStore.Client;
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
        Console.WriteLine($"Hello {typeof(Program).Namespace}!");

        var games = await FetchGames(16);

        foreach (var game in games)
        {
            await FetchPlayByPlay(game.id);
        }

        //var client = new HttpClient();
        //var request = new HttpRequestMessage
        //{
        //    Method = HttpMethod.Get,
        //    RequestUri = new Uri("https://free-nba.p.rapidapi.com/stats?page=0&per_page=25&game_ids[]=47179"),
        //    Headers =
        //    {
        //        { "X-RapidAPI-Key", "a2cbeae557msh805898c556b96f3p1deb37jsncf7afb577490" },
        //        { "X-RapidAPI-Host", "free-nba.p.rapidapi.com" },
        //    },
        //};
        //using (var response = await client.SendAsync(request))
        //{
        //    response.EnsureSuccessStatusCode();
        //    var body = await response.Content.ReadAsStringAsync();
        //    Console.WriteLine(body);
        //}
    }

    private static async Task FetchPlayByPlay(int gameId)
    {
        var games = new List<Datum>();
        int? nextPage = 0;

        var client = new HttpClient();

        while (nextPage.HasValue)
        {
            var request = BuildStatsRequest(gameId, page: nextPage.GetValueOrDefault());

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

    private static async Task<IEnumerable<Game>> FetchGames(int teamId)
    {
        var games = await GetGames(teamId);
        Console.WriteLine(games);

        await File.WriteAllTextAsync("team-16-games.json", JsonConvert.SerializeObject(games, Formatting.Indented));

        return games;
    }

    private static async Task<IEnumerable<Game>> GetGames(int teamId)
    {
        var games = new List<Game>();
        int? nextPage = 0;

        var client = new HttpClient();

        while (nextPage.HasValue)
        {
            var request = BuildGamesRequest(teamId: teamId, page: nextPage.GetValueOrDefault());

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

    private static HttpRequestMessage BuildStatsRequest(int gameId, int page)
    {
        return new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://free-nba.p.rapidapi.com/stats?page={page}&per_page=50&seasons[]=2018&game_ids[]={gameId}"),
            Headers =
            {
                { "X-RapidAPI-Key", "a2cbeae557msh805898c556b96f3p1deb37jsncf7afb577490" },
                { "X-RapidAPI-Host", "free-nba.p.rapidapi.com" },
            }
        };
    }

    private static HttpRequestMessage BuildGamesRequest(int teamId, int page)
    {
        return new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://free-nba.p.rapidapi.com/games?page={page}&per_page=50&seasons[]=2018&team_ids[]={teamId}"),
            Headers =
            {
                { "X-RapidAPI-Key", "a2cbeae557msh805898c556b96f3p1deb37jsncf7afb577490" },
                { "X-RapidAPI-Host", "free-nba.p.rapidapi.com" },
            }
        };
    }
}

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);