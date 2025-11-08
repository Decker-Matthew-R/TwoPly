using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TwoPly.Teams;

namespace TwoPly.Tests.Teams;

public class TeamControllerTests
{
    private readonly Mock<ITeamService> _mockTeamService;
    private readonly Mock<ILogger<TeamController>> _mockLogger;
    private readonly TeamController _controller;

    public TeamControllerTests()
    {
        _mockTeamService = new Mock<ITeamService>();
        _mockLogger = new Mock<ILogger<TeamController>>();
        _controller = new TeamController(_mockTeamService.Object, _mockLogger.Object);
    }

    [Fact]
    public void CreateTeam_WithValidName_ReturnsCreatedResult()
    {
        string mockTeamName = "Engineering";
        int mockTeamId = 1;
        DateTime mockCreationDate = DateTime.UtcNow;

        CreateTeamRequest request = new CreateTeamRequest { TeamName = mockTeamName };
        Team mockTeam = new Team(mockTeamName) { Id = mockTeamId, CreatedDate = mockCreationDate };

        _mockTeamService
            .Setup(s => s.CreateTeam(request.TeamName))
            .Returns(mockTeam);

        IActionResult result = _controller.CreateTeam(request);

        ObjectResult? objectResult = result as ObjectResult;
        TeamResponse? returnedTeam = objectResult?.Value as TeamResponse;

        _mockTeamService.Verify(service => service.CreateTeam(mockTeamName), Times.Once);
        Assert.Equal(mockTeamName, returnedTeam?.TeamName);
        Assert.Equal(mockTeamId, returnedTeam?.Id);
        Assert.Equal(mockCreationDate, returnedTeam?.CreatedDate);
        Assert.Equal(201, objectResult?.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void CreateTeam_WithEmptyName_ReturnsBadRequest(string invalidTeamName)
    {
        CreateTeamRequest request = new CreateTeamRequest { TeamName = invalidTeamName };

        IActionResult result = _controller.CreateTeam(request);

        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Create Team Controller Blank Name Error: team name cannot be blank.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        BadRequestObjectResult? badRequestResult = result as BadRequestObjectResult;
        ErrorResponse? errorResponse = badRequestResult?.Value as ErrorResponse;
        Assert.Equal("Team name cannot be empty", errorResponse?.Error);
    }

    [Theory]
    [InlineData("This team name is waaaaaaaaaay tooooooooo looooooooong")]
    [InlineData("ThisTeamNameIsWaaaaaaaaaayToooooooooLooooooooong")]
    public void CreateTeam_WithNameTooLong_ReturnsBadRequest(string longTeamName)
    {
        int nameLength = longTeamName.Length;
        CreateTeamRequest request = new CreateTeamRequest { TeamName = longTeamName };

        IActionResult result = _controller.CreateTeam(request);

        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(
                        $"Create Team Controller Name Too Large Error: size = {nameLength} characters.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        BadRequestObjectResult? badRequestResult = result as BadRequestObjectResult;
        ErrorResponse? errorResponse = badRequestResult?.Value as ErrorResponse;
        Assert.Equal("Team name cannot be greater than 20 characters", errorResponse?.Error);
    }

    [Fact]
    public void CreateTeam_WhenServiceThrowsException_ReturnsServerError()
    {
        string teamName = "Engineering";

        CreateTeamRequest request = new CreateTeamRequest { TeamName = teamName };
        Exception expectedException = new Exception("Database connection failed");

        _mockTeamService
            .Setup(s => s.CreateTeam(It.IsAny<string>()))
            .Throws(expectedException);


        IActionResult result = _controller.CreateTeam(request);

        ObjectResult? objectResult = result as ObjectResult;
        Assert.NotNull(objectResult);
        Assert.Equal(500, objectResult.StatusCode);

        string json = JsonSerializer.Serialize(objectResult.Value);
        Assert.Contains("An error occurred while creating the team", json);

        _mockTeamService.Verify(service => service.CreateTeam(teamName), Times.Once);
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Error creating team with name: {teamName}")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetAllTeams_ReturnsAllTeams()
    {
        DateTime createdDate = DateTime.UtcNow;

        List<Team> mockTeams = new();
        Team team1 = new Team { TeamName = "Prometheus", Id = 1, CreatedDate = createdDate };
        Team team2 = new Team { TeamName = "Mercury", Id = 2, CreatedDate = createdDate };

        mockTeams.Add(team1);
        mockTeams.Add(team2);

        _mockTeamService
            .Setup(service => service.GetAllTeams())
            .Returns(mockTeams);

        IActionResult result = _controller.GetAllTeams();

        ObjectResult? objectResult = result as ObjectResult;
        List<TeamResponse>? returnedTeams = objectResult?.Value as List<TeamResponse>;

        _mockTeamService.Verify(service => service.GetAllTeams(), Times.Once);
        Assert.Equal(200, objectResult?.StatusCode);
        
        Assert.Equal(2, returnedTeams?.Count);
        
        Assert.Equal("Prometheus", returnedTeams?[0].TeamName);
        Assert.Equal(1, returnedTeams?[0].Id);
        Assert.Equal(createdDate, returnedTeams?[0].CreatedDate);

        Assert.Equal("Mercury", returnedTeams?[1].TeamName);
        Assert.Equal(2, returnedTeams?[1].Id);
        Assert.Equal(createdDate, returnedTeams?[1].CreatedDate);
    }

    [Fact]
    public void GetAllTeams_WhenServiceThrowsException_ReturnsServerError()
    {
        Exception expectedException = new Exception("Database connection failed");

        _mockTeamService
            .Setup(s => s.GetAllTeams())
            .Throws(expectedException);

        IActionResult result = _controller.GetAllTeams();

        ObjectResult? objectResult = result as ObjectResult;
        Assert.NotNull(objectResult);
        Assert.Equal(500, objectResult.StatusCode);

        string json = JsonSerializer.Serialize(objectResult.Value);
        Assert.Contains("An error occurred returning all teams", json);

        _mockTeamService.Verify(service => service.GetAllTeams(), Times.Once);
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error retrieving all teams")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}