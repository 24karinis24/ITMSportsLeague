using SportsLeague.Domain.Entities;

namespace SportsLeague.Domain.Interfaces.Services;

public interface IMatchLineupService
{
    Task<MatchLineup> AddPlayerToLineupAsync(int matchId, int playerId, bool isStarter, string position);
    Task<IEnumerable<MatchLineup>> GetLineupByMatchAsync(int matchId);
    Task<IEnumerable<MatchLineup>> GetLineupByMatchAndTeamAsync(int matchId, int teamId);
    Task DeleteLineupEntryAsync(int id);
}