namespace TwoPly.Teams;

public class TeamService : ITeamService
{
    private readonly List<Team> _teamList = new ();
    private int _nextId = 1;

    public Team CreateTeam(string teamName)
    {
        var team = new Team(teamName)
        {
            Id = _nextId++
        };
        
        _teamList.Add(team);

        return team;
    }

    public List<Team> GetAllTeams()
    {
        return _teamList;
    }
}