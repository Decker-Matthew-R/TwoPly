using Microsoft.AspNetCore.Mvc;

namespace TwoPly.Teams;

[ApiController]
[Route("api/teams")]
public class TeamController(ITeamService iTeamService, ILogger<TeamController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequest createTeamRequest)
    {
        try
        {
            ErrorResponse? validationError = ValidateTeamName(createTeamRequest.TeamName);
            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            Team createdTeam = await iTeamService.CreateTeamAsync(createTeamRequest.TeamName);

            TeamResponse response = MapToResponse(createdTeam);

            return StatusCode(201, response);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error creating team with name: {TeamName}", createTeamRequest.TeamName);
            return StatusCode(500, new ErrorResponse { Error = "An error occurred while creating the team" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTeams()
    {
        try
        {
            List<Team> teamList = await iTeamService.GetAllTeamsAsync();

            List<TeamResponse> response = teamList.ConvertAll(MapToResponse);

            return StatusCode(200, response);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error retrieving all teams");
            return StatusCode(500, new ErrorResponse { Error = "An error occurred returning all teams" });
        }
    }

    private ErrorResponse? ValidateTeamName(string teamName)
    {
        if (string.IsNullOrWhiteSpace(teamName))
        {
            logger.LogWarning("Create Team Controller Blank Name Error: team name cannot be blank.");
            return new ErrorResponse { Error = "Team name cannot be empty" };
        }

        if (teamName.Length > 20)
        {
            logger.LogWarning("Create Team Controller Name Too Large Error: size = {teamNameLength} characters.",
                teamName.Length);
            return new ErrorResponse { Error = "Team name cannot be greater than 20 characters" };
        }

        return null;
    }

    private static TeamResponse MapToResponse(Team team)
    {
        return new TeamResponse
        {
            Id = team.Id,
            TeamName = team.TeamName,
            CreatedDate = team.CreatedDate
        };
    }
}