using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.Domain.Services;

public class MatchLineupService : IMatchLineupService
{
    private readonly IMatchLineupRepository _lineupRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<MatchLineupService> _logger;

    public MatchLineupService(
        IMatchLineupRepository lineupRepository,
        IMatchRepository matchRepository,
        IPlayerRepository playerRepository,
        ILogger<MatchLineupService> logger)
    {
        _lineupRepository = lineupRepository;
        _matchRepository = matchRepository;
        _playerRepository = playerRepository;
        _logger = logger;
    }

    public async Task<MatchLineup> AddPlayerToLineupAsync(int matchId, int playerId, bool isStarter, string position)
    {
        // Validation 1 - Partido existente
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");

        // Validation 2 - Jugador existente
        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null)
            throw new KeyNotFoundException($"No se encontró el jugador con ID {playerId}");

        // Validation 3 - Jugador perteneciente a HomeTeam o AwayTeam
        if (player.TeamId != match.HomeTeamId && player.TeamId != match.AwayTeamId)
            throw new InvalidOperationException("El jugador no pertenece a ninguno de los equipos del partido");

        // Validation 4 - Un jugador no puede estar registrado dos veces
        var alreadyExists = await _lineupRepository.ExistsByMatchAndPlayerAsync(matchId, playerId);
        if (alreadyExists)
            throw new InvalidOperationException("El jugador ya está registrado en la alineación de este partido");

        // Validation 5 - Máximo 11 titulares por equipo por partido
        if (isStarter)
        {
            var starterCount = await _lineupRepository.CountStartersByMatchAndTeamAsync(matchId, player.TeamId, true);
            if (starterCount >= 11)
                throw new InvalidOperationException("El equipo ya tiene 11 titulares registrados en este partido");
        }

        // Validation 6 - El partido debe estar en estado Scheduled
        if (match.Status != MatchStatus.Scheduled)
            throw new InvalidOperationException("Solo se pueden registrar alineaciones en partidos Scheduled");

        var lineup = new MatchLineup
        {
            MatchId = matchId,
            PlayerId = playerId,
            IsStarter = isStarter,
            Position = position
        };

        return await _lineupRepository.CreateAsync(lineup);
    }

    public async Task<IEnumerable<MatchLineup>> GetLineupByMatchAsync(int matchId)
        => await _lineupRepository.GetByMatchAsync(matchId);

    public async Task<IEnumerable<MatchLineup>> GetLineupByMatchAndTeamAsync(int matchId, int teamId)
        => await _lineupRepository.GetByMatchAndTeamAsync(matchId, teamId);

    public async Task DeleteLineupEntryAsync(int id)
    {
        var entry = await _lineupRepository.GetByIdAsync(id);
        if (entry == null)
            throw new KeyNotFoundException($"No se encontró la entrada de alineación con ID {id}");

        await _lineupRepository.DeleteAsync(id);
    }
}