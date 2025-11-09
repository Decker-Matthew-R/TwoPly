namespace TwoPly.Teams;

public interface ITeamService
{
    Task<Team> CreateTeamAsync(string teamName);
    Task<List<Team>> GetAllTeamsAsync();
}