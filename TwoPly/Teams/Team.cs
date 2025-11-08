namespace TwoPly.Teams;

public class Team
{
    public int Id { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }


    public Team(string teamName)
    {
        TeamName = teamName;
        CreatedDate = DateTime.UtcNow;
    }

    public Team()
    {
    }
}