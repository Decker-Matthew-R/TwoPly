using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TwoPly.Data;
using TwoPly.Teams;

namespace TwoPly.Tests.Teams;

public class TeamServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<TeamService>> _mockLogger;
    private readonly TeamService _service;
    
    public TeamServiceTests()
    {
        _context = Helpers.DbContextHelper.CreateInMemoryContext();
        _mockLogger = new Mock<ILogger<TeamService>>();
        _service = new TeamService(_context, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateTeamAsync_SavesTeamToDatabase()
    {
        string teamName = "Prometheus";
        
        Team createdTeam = await _service.CreateTeamAsync(teamName);
        
        Assert.Equal(1, createdTeam.Id);
        Assert.Equal(teamName, createdTeam.TeamName);
        
        Team? dbTeam = await _context.Teams.FindAsync(createdTeam.Id);
        Assert.Equal(1, dbTeam?.Id);
        Assert.Equal(teamName, dbTeam?.TeamName);

    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData(null)]
    public async Task CreateTeamAsync_WithInvalidName_ThrowsArgumentException(string? invalidName)
    {
        
        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.CreateTeamAsync(invalidName!)
        );

        Assert.Equal("teamName", exception.ParamName);
        Assert.Contains("Team name cannot be null or empty", exception.Message);

        
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Team name cannot be null or empty")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
    
    [Fact]
    public async Task GetAllTeamsAsync_GetsAllTeamsFromDatabase()
    {
        DateTime dateAddedToDb = DateTime.UtcNow;
        Team team1 = new Team("Team1") { Id = 1, CreatedDate = dateAddedToDb};
        Team team2 = new Team("Team2") { Id = 2, CreatedDate = dateAddedToDb};
        
        _context.Teams.AddRange(
            team1, team2
            );
        
        await _context.SaveChangesAsync();
        
        List<Team> responseTeamList = await _service.GetAllTeamsAsync();

        Assert.Equal(2, responseTeamList.Count);
            
        Assert.Equal(1, responseTeamList[0].Id);
        Assert.Equal("Team1", responseTeamList[0].TeamName);
        Assert.Equal(dateAddedToDb, responseTeamList[0].CreatedDate);
        
        Assert.Equal(2, responseTeamList[1].Id);
        Assert.Equal("Team2", responseTeamList[1].TeamName);
        Assert.Equal(dateAddedToDb, responseTeamList[1].CreatedDate);
    }


    public void Dispose()
    {
        _context.Dispose();
    }
}