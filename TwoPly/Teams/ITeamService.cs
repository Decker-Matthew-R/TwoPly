namespace TwoPly.Teams;

public interface ITeamService
{
    Team CreateTeam(string teamName);
    List<Team> GetAllTeams();
}