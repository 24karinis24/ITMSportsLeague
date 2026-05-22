using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.API.Controllers;

[ApiController]
[Route("api/match/{matchId}/lineup")]
public class MatchLineupController : ControllerBase
{
    private readonly IMatchLineupService _lineupService;
    private readonly IMapper _mapper;

    public MatchLineupController(IMatchLineupService lineupService, IMapper mapper)
    {
        _lineupService = lineupService;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> AddToLineup(int matchId, [FromBody] MatchLineupRequestDTO dto)
    {
        try
        {
            var lineup = await _lineupService.AddPlayerToLineupAsync(
                matchId, dto.PlayerId, dto.IsStarter, dto.Position);
            var response = _mapper.Map<MatchLineupResponseDTO>(lineup);
            return CreatedAtAction(nameof(GetLineup), new { matchId }, response);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpGet]
    public async Task<IActionResult> GetLineup(int matchId)
    {
        var lineups = await _lineupService.GetLineupByMatchAsync(matchId);
        return Ok(_mapper.Map<IEnumerable<MatchLineupResponseDTO>>(lineups));
    }

    [HttpGet("team/{teamId}")]
    public async Task<IActionResult> GetLineupByTeam(int matchId, int teamId)
    {
        var lineups = await _lineupService.GetLineupByMatchAndTeamAsync(matchId, teamId);
        return Ok(_mapper.Map<IEnumerable<MatchLineupResponseDTO>>(lineups));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFromLineup(int matchId, int id)
    {
        try
        {
            await _lineupService.DeleteLineupEntryAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }
}