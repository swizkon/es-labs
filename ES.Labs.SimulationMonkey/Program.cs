// See https://aka.ms/new-console-template for more information

using System.Net.Http.Json;
using RetailRhythmRadar.Domain.Projections;

Console.WriteLine("Hello, World!");

// Simulate every minute from 08:00 to 22:00

var httpClient = new HttpClient();

// var currentTime = DateTime.UtcNow.Date.AddDays(-29).AddHours(8);
var currentTime = DateTime.UtcNow.Date.AddHours(8);
var end = currentTime.AddHours(16);

while (currentTime < end)
{
    var next = currentTime.AddSeconds(10);
    Console.WriteLine($"Simulating {currentTime} to {next}");
    currentTime = next;

    var storeNumber = Random.Shared.Next(1, 10);

    // Get store state and do some calculation on what to do next...

    var stateUrl = $"http://localhost:4000/queries/store-{storeNumber}/{currentTime.Date:yyyy-MM-dd}";
    
    var data = await httpClient.GetAsync(stateUrl);
    var json = await data.Content.ReadFromJsonAsync<SingleStoreState>();

    // For this iteration eval the possibility for enter and exit + zone transitions for each zone...

    Console.WriteLine($"Store {storeNumber} A: {json.ZoneA}, B: {json.ZoneB}, C: {json.ZoneC}, D: {json.ZoneD}");

    var turnstileActions = Simulator.GetSimulatedActions(storeNumber, json, currentTime);

    var tasks = turnstileActions.Select(t => httpClient.PostAsJsonAsync("http://localhost:4000/events/TurnstilePassageDetected", t)).ToArray();

    Parallel.ForEach(tasks, t => t.Wait());

    //foreach (var turnstileAction in turnstileActions)
    //{
    //    httpClient.PostAsJsonAsync("http://localhost:4000/events/TurnstilePassageDetected", turnstileAction);
    //}
}

httpClient.Dispose();