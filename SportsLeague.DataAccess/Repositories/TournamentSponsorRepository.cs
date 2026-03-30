using Microsoft.EntityFrameworkCore;
using SportsLeague.DataAccess.Context;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.DataAccess.Repositories;

public class TournamentSponsorRepository : GenericRepository<TournamentSponsor>, ITournamentSponsorRepository
{
    public TournamentSponsorRepository(LeagueDbContext context) : base(context)
    {
    }

    public async Task<TournamentSponsor?> GetByTournamentAndSponsorAsync(int tournamentId, int sponsorId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(tt =>
                tt.TournamentId == tournamentId && tt.SponsorId == sponsorId);
    }

    public async Task<IEnumerable<TournamentSponsor>> GetByTournamentIdAsync(int tournamentId)
    {
        return await _dbSet
            .Where(tt => tt.TournamentId == tournamentId)
            .Include(tt => tt.Sponsor)
            .ToListAsync();
    }

    public async Task<IEnumerable<TournamentSponsor>> GetBySponsorIdAsync(int sponsorId)
    {
        return await _dbSet
            .Where(tt => tt.SponsorId == sponsorId)
            .Include(tt => tt.Tournament)
            .ToListAsync();
    }
    public async Task<TournamentSponsor?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.TournamentSponsors
            .Include(ts => ts.Tournament)
            .Include(ts => ts.Sponsor)
            .FirstOrDefaultAsync(ts => ts.Id == id);
    }
}
