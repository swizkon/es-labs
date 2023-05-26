namespace ES.Labs.DataFetcher;

public class Game
{
    public int id { get; set; }
    public DateTime date { get; set; }
    public Team home_team { get; set; }
    public int home_team_score { get; set; }
    public int period { get; set; }
    public bool postseason { get; set; }
    public int season { get; set; }
    public string status { get; set; }
    public string time { get; set; }
    public Team visitor_team { get; set; }
    public int visitor_team_score { get; set; }
}