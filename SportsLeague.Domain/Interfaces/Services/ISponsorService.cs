using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;

namespace SportsLeague.Domain.Interfaces.Services;

public interface ISponsorService
{
    Task<IEnumerable<Sponsor>> GetAllAsync();
    Task<Sponsor?> GetByIdAsync(int id);
    Task<Sponsor> CreateAsync(Sponsor Sponsor);
    Task UpdateAsync(int id, Sponsor Sponsor);
    Task DeleteAsync(int id);
    Task<IEnumerable<Tournament>> GetTournamentsBySponsorAsync(int id);
    Task<TournamentSponsor?> LinkSponsorAsync(TournamentSponsor tournamentSponsor);
    Task UnlinkSponsorAsync(int sponsorId, int tournamentId);
}
