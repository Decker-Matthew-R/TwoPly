using Microsoft.EntityFrameworkCore;
using TwoPly.Data;

namespace TwoPly.Teams;

public class TeamService(ApplicationDbContext context, ILogger<TeamService> logger) : ITeamService
{
    public async Task<Team> CreateTeamAsync(string teamName)
    {
        if (string.IsNullOrWhiteSpace(teamName))
        {
            logger.LogError("Team Service: Team name cannot be null or empty");
            throw new ArgumentException("Team Service: Team name cannot be null or empty", nameof(teamName));
        }

        Team team = new Team(teamName);
        
        context.Teams.Add(team);
        
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
        {
            logger.LogError("Team ServiceFailed to save team to databas");
            throw new InvalidOperationException("Failed to save team to database", exception);
        }
        
        return team;
    }

    public async Task<List<Team>> GetAllTeamsAsync()
    {
        return await context.Teams.ToListAsync();
    }
}