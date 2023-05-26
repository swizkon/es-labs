namespace ES.Labs.DataFetcher;

public class Root
{
    public List<Game> data { get; set; }
    public Meta meta { get; set; }
}


// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class Datum
{
    public int id { get; set; }
    public int? ast { get; set; }
    public int? blk { get; set; }
    public int? dreb { get; set; }
    public double? fg3_pct { get; set; }
    public int? fg3a { get; set; }
    public int? fg3m { get; set; }
    public double? fg_pct { get; set; }
    public int? fga { get; set; }
    public int? fgm { get; set; }
    public double? ft_pct { get; set; }
    public int? fta { get; set; }
    public int? ftm { get; set; }
    public StatsGame game { get; set; }
    public string min { get; set; }
    public int? oreb { get; set; }
    public int? pf { get; set; }
    public Player player { get; set; }
    public int? pts { get; set; }
    public int? reb { get; set; }
    public int? stl { get; set; }
    public Team team { get; set; }
    public int? turnover { get; set; }
}

public class StatsGame
{
    public int id { get; set; }
    public DateTime date { get; set; }
    public int home_team_id { get; set; }
    public int home_team_score { get; set; }
    public int period { get; set; }
    public bool postseason { get; set; }
    public int season { get; set; }
    public string status { get; set; }
    public string time { get; set; }
    public int visitor_team_id { get; set; }
    public int visitor_team_score { get; set; }
}

//public class Meta
//{
//    public int total_pages { get; set; }
//    public int current_page { get; set; }
//    public int next_page { get; set; }
//    public int per_page { get; set; }
//    public int total_count { get; set; }
//}

public class Player
{
    public int id { get; set; }
    public string first_name { get; set; }
    public int? height_feet { get; set; }
    public int? height_inches { get; set; }
    public string last_name { get; set; }
    public string position { get; set; }
    public int team_id { get; set; }
    public int? weight_pounds { get; set; }
}

public class GameStats
{
    public List<Datum> data { get; set; }
    public Meta meta { get; set; }
}
